using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;
using PilotUITesting.Uia;

namespace PilotUITesting.Client
{
    /// <summary>
    /// Запущенный клиент Pilot-ICE / Pilot-BIM: главное окно, вкладки, страница «Новая вкладка» с кнопками модулей,
    /// модальные окна, скриншоты и дампы для отчётов.
    /// Новое в 0.2: <see cref="Windows"/>, <see cref="TabBar"/>, <see cref="MainMenu"/>, <see cref="State"/>,
    /// <see cref="ShowObject"/>, <see cref="Quit"/>, <see cref="Restart"/> (файл PilotApp.More.cs).
    /// </summary>
    public sealed partial class PilotApp : IDisposable
    {
        // AutomationId элементов клиента Pilot (одинаковы в Pilot-ICE Enterprise и Pilot-BIM 26.x)
        public const string MainWindowId = "mainPilotView";
        public const string TabHostId = "tabHostView";
        public const string AddTabButtonId = "AddTabButton";
        public const string TabItemId = "AUIDTabItemLayoutDocument";
        public const string TabTitleId = "DocumentTitleTextBlock";
        public const string TabCloseButtonId = "DocumentCloseButton";
        public const string UserProfileButtonId = "AUIDCurrentUserProfile";
        public const string NewTabTitle = "Новая вкладка";

        private readonly PilotUiConfig _config;
        private readonly bool _startedByUs;
        private UIA3Automation _automation;
        private Application _application;

        public PilotUiConfig Config => _config;

        public AutomationBase Automation => _automation;

        public Process Process { get; }

        private PilotApp(PilotUiConfig config, Process process, bool startedByUs)
        {
            _config = config;
            Process = process;
            _startedByUs = startedByUs;
            _automation = new UIA3Automation();
            _application = Application.Attach(process);
            Windows = new PilotWindows(this);
            TabBar = new PilotTabBar(this);
            MainMenu = new PilotMenu(this, () => MainWindow?.TryFind(By.Id(PilotIds.MainMenu.Container)), PilotIds.MainMenu.Container);
        }

        /// <summary>
        /// Подключиться к уже запущенному клиенту или запустить новый и дождаться готовности.
        /// Осведомлённость о DPI включается здесь, до создания UI Automation, если в конфигурации "dpiAware": true.
        /// Выставляет <see cref="Wait.DefaultTimeout"/> из конфигурации.
        /// </summary>
        public static PilotApp StartOrAttach(PilotUiConfig config)
        {
            if (config.DpiAware)
            {
                PilotLog.Action(ProcessDpi.Apply());
            }
            Wait.DefaultTimeout = config.DefaultTimeout;
            Process running = FindRunning(config);
            if (running != null && config.AttachToRunning)
            {
                PilotApp attached = new PilotApp(config, running, false);
                attached.WaitMainWindow();
                if (config.RestartOnAttach)
                {
                    PilotLog.Action("клиент уже был запущен - перезапуск для свежего дерева UI Automation (restartOnAttach)");
                    return attached.Restart();
                }
                return attached;
            }
            if (running != null)
            {
                throw new InvalidOperationException($"Клиент {config.ProcessName} уже запущен, а AttachToRunning=false. Закройте его или разрешите подключение.");
            }
            if (!File.Exists(config.ResolvedExePath))
            {
                throw new FileNotFoundException("Не найден exe клиента Pilot", config.ResolvedExePath);
            }
            PilotLog.Action("запуск клиента " + config.ResolvedExePath);
            Process process = Process.Start(new ProcessStartInfo(config.ResolvedExePath) { UseShellExecute = true });
            PilotApp app = new PilotApp(config, process, true);
            app.WaitMainWindow();
            return app;
        }

        /// <summary>Процесс клиента в текущем сеансе Windows (процессы других сеансов не подходят).</summary>
        private static Process FindRunning(PilotUiConfig config)
        {
            int session = Process.GetCurrentProcess().SessionId;
            return Process.GetProcessesByName(config.ProcessName).FirstOrDefault(p => p.SessionId == session);
        }

        /// <summary>Главное окно клиента (после входа).</summary>
        public Window MainWindow
        {
            get
            {
                return Wait.Retry(() =>
                {
                    AutomationElement desktop = _automation.GetDesktop();
                    AutomationElement window = desktop.FindFirstChild(cf => cf.ByProcessId(Process.Id).And(cf.ByAutomationId(MainWindowId)));
                    return window?.AsWindow();
                });
            }
        }

        /// <summary>Все окна верхнего уровня процесса (главное + модальные), последние открытые - в конце.</summary>
        public AutomationElement[] TopLevelWindows()
        {
            return Wait.Retry(() => _automation.GetDesktop().FindAllChildren(cf => cf.ByProcessId(Process.Id)));
        }

        /// <summary>Окна верхнего уровня одним запросом, без повторов при занятости (для диагностики, которая не должна ждать клиент).</summary>
        internal AutomationElement[] TopLevelWindowsOnce()
        {
            return _automation.GetDesktop().FindAllChildren(cf => cf.ByProcessId(Process.Id));
        }

        /// <summary>Окно верхнего уровня с указанным заголовком (модальные окна Pilot и модулей). Вложенные окна - <see cref="Windows"/>.</summary>
        public Window WaitWindow(string title, TimeSpan? timeout = null)
        {
            return Wait.For(() => TryWindow(title), timeout ?? _config.DefaultTimeout, $"окно '{title}'");
        }

        public Window WaitWindowContains(string titlePart, TimeSpan? timeout = null)
        {
            return Wait.For(() => TopLevelWindows().FirstOrDefault(w => w.SafeName().IndexOf(titlePart, StringComparison.OrdinalIgnoreCase) >= 0)?.AsWindow(),
                timeout ?? _config.DefaultTimeout, $"окно, заголовок которого содержит '{titlePart}'");
        }

        public Window TryWindow(string title)
        {
            return TopLevelWindows().FirstOrDefault(w => string.Equals(w.SafeName(), title, StringComparison.Ordinal))?.AsWindow();
        }

        public void WaitWindowClosed(string title, TimeSpan? timeout = null)
        {
            Wait.Until(() => TryWindow(title) == null, timeout ?? _config.DefaultTimeout, $"закрытие окна '{title}'");
        }

        /// <summary>
        /// Готовность клиента: главное окно, подключение к серверу (HelpText tabHostView «ConnectionState:Online»; прежний признак -
        /// текст «ПОДКЛЮЧЕНО» в шапке - есть только в развёрнутом окне) и вкладки загружены (ItemStatus «IsLoadingTabs:False»).
        /// </summary>
        private void WaitMainWindow()
        {
            TimeSpan timeout = _config.ClientStartTimeout;
            Wait.Until(() => MainWindow != null, timeout, "главное окно клиента Pilot (вход в базу выполняется автоматически по сохранённым данным)");
            Wait.Until(() => IsConnected, timeout, "клиент подключён к серверу (tabHostView: ConnectionState:Online)");
            Wait.Until(() => !State.IsLoadingTabs, timeout, "вкладки клиента загружены (tabHostView: IsLoadingTabs:False)");
            WaitDiagnostics.Provider = DescribeForDiagnostics;
            string user = State.UserCaption;
            if (!string.IsNullOrEmpty(_config.ClientUser) && !string.Equals(user, _config.ClientUser, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Клиент Pilot вошёл под пользователем «{user}» (подпись кнопки профиля), а в конфигурации clientUser = «{_config.ClientUser}». Тесты не запущены: они рассчитаны на другого пользователя.");
            }
            PilotLog.Action($"клиент готов: pid {Process.Id}, пользователь «{user}»");
        }

        /// <summary>Клиент подключён к серверу: HelpText tabHostView «ConnectionState:Online» (или текст «ПОДКЛЮЧЕНО» в развёрнутой шапке).</summary>
        public bool IsConnected
        {
            get
            {
                AutomationElement host = MainWindow?.ChildById(TabHostId);
                string help = host.SafeHelpText();
                if (help.StartsWith("ConnectionState:", StringComparison.Ordinal))
                {
                    return PilotStatus.Connection(help) == ClientConnection.Online;
                }
                AutomationElement profile = MainWindow?.ChildById(UserProfileButtonId);
                return profile != null && profile.Texts().Any(t => t.IndexOf("ПОДКЛЮЧЕНО", StringComparison.OrdinalIgnoreCase) >= 0);
            }
        }

        /// <summary>Подпись пользователя из шапки клиента («BOT1 • ПОДКЛЮЧЕНО» -> BOT1; только в развёрнутом окне). Точнее - <see cref="ClientState.UserCaption"/>.</summary>
        public string CurrentUserCaption
        {
            get
            {
                AutomationElement profile = MainWindow?.ChildById(UserProfileButtonId);
                string text = profile == null ? "" : string.Join(" ", profile.Texts());
                int dot = text.IndexOf('•');
                return dot > 0 ? text.Substring(0, dot).Trim() : text.Trim();
            }
        }

        // ------------------------------------------------------------------ вкладки

        /// <summary>Контейнер вкладок: tabHostView -> единственный потомок, в нём содержимое (TabView), элементы вкладок и «+».</summary>
        public AutomationElement TabStrip
        {
            get
            {
                AutomationElement host = MainWindow?.ChildById(TabHostId);
                return host?.SafeChildren().FirstOrDefault();
            }
        }

        /// <summary>Содержимое активной вкладки (элемент класса TabView внутри контейнера вкладок). У вкладок модулей-HwndHost (реестр) - null.</summary>
        public AutomationElement TabContent
        {
            get
            {
                return TabStrip?.SafeChildren().FirstOrDefault(c => c.SafeClassName() == "TabView");
            }
        }

        public AutomationElement[] Tabs()
        {
            return (TabStrip?.SafeChildren() ?? new AutomationElement[0]).Where(c => c.SafeAutomationId() == TabItemId).ToArray();
        }

        public string TabTitle(AutomationElement tab)
        {
            AutomationElement title = tab.FindDescendantBfs(e => e.SafeAutomationId() == TabTitleId, 4);
            return title?.SafeName() ?? tab.SafeName();
        }

        public AutomationElement FindTab(string title)
        {
            return Tabs().FirstOrDefault(t => TabTitle(t) == title);
        }

        /// <summary>Открыть страницу «Новая вкладка» и нажать кнопку модуля с указанным названием.</summary>
        public void OpenModuleFromNewTab(string moduleButtonTitle, TimeSpan? timeout = null)
        {
            TimeSpan t = timeout ?? _config.DefaultTimeout;
            AutomationElement addTab = Wait.For(() => TabStrip?.ChildById(AddTabButtonId), t, "кнопка «+» (новая вкладка)");
            addTab.Press();
            AutomationElement button = Wait.For(
                () => TabContent?.FindDescendantBfs(e => e.SafeControlType() == ControlType.Button && e.SafeName() == moduleButtonTitle, 12),
                t, $"кнопка модуля '{moduleButtonTitle}' на странице «Новая вкладка»");
            button.Press();
        }

        /// <summary>Закрыть вкладку по заголовку (если открыта). true - вкладка действительно закрылась.</summary>
        public bool CloseTab(string title)
        {
            AutomationElement tab = FindTab(title);
            if (tab == null)
            {
                return false;
            }
            AutomationElement close = tab.FindDescendantBfs(e => e.SafeAutomationId() == TabCloseButtonId, 4);
            if (close == null)
            {
                PilotLog.Warn($"У вкладки «{title}» нет кнопки закрытия {TabCloseButtonId} в дереве UI Automation.");
                return false;
            }
            close.Press();
            bool closed = Wait.TryUntil(() => FindTab(title) == null, TimeSpan.FromSeconds(10));
            if (!closed)
            {
                PilotLog.Warn($"Вкладка «{title}» не закрылась за 10 с после нажатия {TabCloseButtonId}.");
            }
            return closed;
        }

        /// <summary>Закрыть все вкладки, кроме перечисленных. Клиент может быть общим с пользователем - осторожно.</summary>
        public void CloseTabsExcept(params string[] keepTitles)
        {
            foreach (AutomationElement tab in Tabs().ToArray())
            {
                string title = TabTitle(tab);
                if (keepTitles.Contains(title))
                {
                    continue;
                }
                CloseTab(title);
            }
        }

        /// <summary>
        /// Закрыть все окна процесса, кроме главного: нажать первую найденную кнопку отмены из списка AutomationId,
        /// иначе закрыть окно. Нужно перед закрытием вкладки модуля: модальные окна модуля иначе остаются «висеть».
        /// Возвращает число окон, закрытие которых подтверждено; неудачи пишутся в журнал.
        /// </summary>
        public int CloseDialogs(params string[] cancelButtonIds)
        {
            int closed = CloseClientErrorWindows();
            for (int attempt = 0; attempt < 10; attempt++)
            {
                AutomationElement dialog = TopLevelWindows().LastOrDefault(w => w.SafeAutomationId() != MainWindowId && w.SafeControlType() == ControlType.Window);
                if (dialog == null)
                {
                    break;
                }
                string dialogName = dialog.Describe();
                // вложенные окна (системный диалог файла, справочник) закрываем первыми, иначе карточка модальна
                for (int nested = 0; nested < 5; nested++)
                {
                    AutomationElement child = dialog.FindDescendantBfs(e => e.SafeControlType() == ControlType.Window, 2);
                    if (child == null)
                    {
                        break;
                    }
                    AutomationElement childCancel = new[] { PilotObjectDialog.FileDialogCancelButtonId, PilotObjectDialog.CancelButtonId }
                        .Concat(cancelButtonIds)
                        .Select(id => child.TryById(id))
                        .FirstOrDefault(b => b != null && b.SafeIsEnabled());
                    PressOrClose(child, childCancel);
                    if (!Wait.TryUntil(() => !child.SafeIsAvailable(), TimeSpan.FromSeconds(10)))
                    {
                        PilotLog.Warn("Вложенное окно не закрылось: " + child.Describe());
                        break;
                    }
                }
                AutomationElement cancel = cancelButtonIds.Select(id => dialog.TryById(id)).FirstOrDefault(b => b != null && b.SafeIsEnabled());
                PressOrClose(dialog, cancel);
                // ждём исчезновения именно этого окна (а не уменьшения числа окон, снятого после нажатия)
                if (!Wait.TryUntil(() => !dialog.SafeIsAvailable() || !TopLevelWindows().Any(w => w.Equals(dialog)), TimeSpan.FromSeconds(10)))
                {
                    PilotLog.Warn("Окно не закрылось: " + dialogName);
                    break;
                }
                PilotLog.Action("закрыто окно " + dialogName);
                closed++;
            }
            return closed;
        }

        private static void PressOrClose(AutomationElement window, AutomationElement button)
        {
            try
            {
                if (button != null)
                {
                    button.Press();
                }
                else
                {
                    window.AsWindow().Close();
                }
            }
            catch (Exception ex)
            {
                PilotLog.Warn($"Закрытие окна {window.Describe()} ({(button != null ? "кнопка " + button.SafeAutomationId() : "WindowPattern.Close")}) завершилось ошибкой: {ex}");
            }
        }

        /// <summary>
        /// Окно «Ошибка в …» клиента Pilot показывается внутри главного окна; текст пишется в журнал, окно закрывается кнопкой «Закрыть».
        /// Возвращает число окон, закрытие которых подтверждено.
        /// </summary>
        public int CloseClientErrorWindows()
        {
            int closed = 0;
            for (int attempt = 0; attempt < 5; attempt++)
            {
                AutomationElement error = MainWindow?.SafeChildren().FirstOrDefault(c => c.SafeAutomationId().StartsWith("AUIDPilotError", StringComparison.Ordinal));
                if (error == null)
                {
                    break;
                }
                AutomationElement message = error.TryById("exceptionMessageText");
                PilotLog.Warn($"Окно ошибки Pilot «{error.SafeName()}»: {message.SafeValue()}");
                AutomationElement close = error.TryById("closeButton") ?? error.TryById("btnClose");
                PressOrClose(error, close);
                if (!Wait.TryUntil(() => !error.SafeIsAvailable() || MainWindow?.SafeChildren().All(c => !c.Equals(error)) == true, TimeSpan.FromSeconds(5)))
                {
                    PilotLog.Warn("Окно ошибки Pilot не закрылось: " + error.Describe());
                    break;
                }
                closed++;
            }
            return closed;
        }

        // ------------------------------------------------------------------ артефакты

        public string Screenshot(string name)
        {
            Directory.CreateDirectory(_config.ResolvedArtifactsDir);
            string path = Path.Combine(_config.ResolvedArtifactsDir, $"{DateTime.Now:yyyyMMdd_HHmmss}_{Sanitize(name)}.png");
            try
            {
                AutomationElement target = TopLevelWindows().LastOrDefault() ?? MainWindow;
                target?.CaptureToFile(path);
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.ChangeExtension(path, ".txt"), "Скриншот не снят: " + ex);
            }
            return path;
        }

        public string DumpWindows(string name)
        {
            Directory.CreateDirectory(_config.ResolvedArtifactsDir);
            string path = Path.Combine(_config.ResolvedArtifactsDir, $"{DateTime.Now:yyyyMMdd_HHmmss}_{Sanitize(name)}.txt");
            string text = string.Join(Environment.NewLine + Environment.NewLine, TopLevelWindows().Select(w => w.DumpTree()));
            File.WriteAllText(path, text);
            return path;
        }

        internal static string Sanitize(string s)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                s = s.Replace(c, '_');
            }
            return s.Length > 60 ? s.Substring(0, 60) : s;
        }

        public void Dispose()
        {
            if (_startedByUs && !_config.KeepClientOpen && !Process.HasExited)
            {
                try
                {
                    MainWindow?.Close();
                }
                catch (Exception ex)
                {
                    PilotLog.Warn("Главное окно клиента не закрылось штатно: " + ex.Message);
                }
                if (!Process.WaitForExit(15000))
                {
                    PilotLog.Warn("Клиент, запущенный тестами, не завершился за 15 с - процесс завершается принудительно.");
                    Process.Kill();
                }
            }
            if (WaitDiagnostics.Provider == (Func<string>)DescribeForDiagnostics)
            {
                WaitDiagnostics.Provider = null;
            }
            _application?.Dispose();
            _automation?.Dispose();
            _application = null;
            _automation = null;
        }
    }
}
