using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.WindowsAPI;
using PilotUITesting.Uia;

namespace PilotUITesting.Client
{
    /// <summary>Состояние клиента, прочитанное из HelpText/ItemStatus/ItemType элементов главного окна (форматы подтверждены на Pilot-ICE 26.11).</summary>
    public sealed class ClientState
    {
        internal ClientState(ClientConnection connection, bool isLoadingTabs, string userCaption, string lastCommand)
        {
            Connection = connection;
            IsLoadingTabs = isLoadingTabs;
            UserCaption = userCaption;
            LastCommand = lastCommand;
        }

        /// <summary>tabHostView HelpText «ConnectionState:…».</summary>
        public ClientConnection Connection { get; }

        /// <summary>tabHostView ItemStatus «IsLoadingTabs:…».</summary>
        public bool IsLoadingTabs { get; }

        /// <summary>
        /// Подпись пользователя - ItemStatus кнопки профиля как есть: «отображаемое имя (должность)», например «bot2 (bot)»;
        /// пустая строка - у пользователя нет должности; null - кнопки профиля нет. Логина клиент в UI Automation не показывает.
        /// </summary>
        public string UserCaption { get; }

        /// <summary>Последняя команда, пришедшая в клиент по pipe (tabHostView ItemType), например «mniShowDocumentCommand:&lt;id&gt;/-1/&lt;annot&gt;».</summary>
        public string LastCommand { get; }
    }

    public sealed partial class PilotApp
    {
        private bool _moduleViewOpened;

        /// <summary>Окна процесса клиента: верхнего уровня и вложенные (дочерние окна главного - диалоги модулей, окна ошибок).</summary>
        public PilotWindows Windows { get; }

        /// <summary>Вкладки клиента.</summary>
        public PilotTabBar TabBar { get; }

        /// <summary>Главное меню клиента (mainMenu: «СЕРВИС» и пункты модулей).</summary>
        public PilotMenu MainMenu { get; }

        /// <summary>Фактический режим DPI процесса тестов.</summary>
        public static DpiStatus Dpi => ProcessDpi.Current();

        /// <summary>
        /// Объявить процесс осведомлённым о DPI (Per-Monitor V2) - до первого обращения к UI Automation. Нужно своим инструментам и тестам
        /// без <see cref="PilotUiConfig.DpiAware"/>: на экране больше 100 % без этого клики мышью попадают мимо элементов.
        /// </summary>
        public static DpiStatus EnableDpiAwareness()
        {
            PilotLog.Action(ProcessDpi.Apply());
            return ProcessDpi.Current();
        }

        /// <summary>Id процесса клиента.</summary>
        public int ProcessId => Process.Id;

        /// <summary>Состояние клиента (читается при каждом обращении).</summary>
        public ClientState State
        {
            get
            {
                Window main = MainWindowStrict();
                AutomationElement host = UiQuery.Read(() => main.FindFirstChild(cf => cf.ByAutomationId(PilotIds.MainWindow.TabHost)), Wait.DefaultTimeout, "tabHostView")
                    ?? throw new InvalidOperationException("В главном окне нет tabHostView.");
                AutomationElement profile = UiQuery.Read(() => main.FindFirstChild(cf => cf.ByAutomationId(PilotIds.MainWindow.UserProfile)), Wait.DefaultTimeout, "профиль");
                string help = UiQuery.Read(() => host.Properties.HelpText.ValueOrDefault ?? "", Wait.DefaultTimeout, "HelpText tabHostView");
                string status = UiQuery.Read(() => host.Properties.ItemStatus.ValueOrDefault ?? "", Wait.DefaultTimeout, "ItemStatus tabHostView");
                string command = UiQuery.Read(() => host.Properties.ItemType.ValueOrDefault ?? "", Wait.DefaultTimeout, "ItemType tabHostView");
                string user = profile == null ? null : UiQuery.Read(() => profile.Properties.ItemStatus.ValueOrDefault ?? "", Wait.DefaultTimeout, "ItemStatus профиля");
                return new ClientState(PilotStatus.Connection(help), PilotStatus.IsLoadingTabs(status), user, command);
            }
        }

        /// <summary>
        /// Главное окно для строгих проверок: если поиск на рабочем столе UI Automation его не вернул, а процесс клиента жив - ждать
        /// (не дольше <see cref="Wait.DefaultTimeout"/>; так было в живом прогоне сразу после Ctrl+W во вкладке модуля, причина в UIA
        /// не установлена), задержка пишется в журнал. Процесс завершён - исключение сразу. Прежний <see cref="MainWindow"/> не ждёт.
        /// </summary>
        internal Window MainWindowStrict()
        {
            Window main = MainWindow;
            if (main != null)
            {
                return main;
            }
            Stopwatch sw = Stopwatch.StartNew();
            main = Wait.ForStrict(() =>
            {
                if (Process.HasExited)
                {
                    throw new InvalidOperationException($"Процесс клиента Pilot завершён (код {Process.ExitCode}).");
                }
                return MainWindow;
            }, Wait.DefaultTimeout, "главное окно клиента Pilot в UI Automation", null);
            PilotLog.Warn($"главное окно клиента Pilot не находилось в UI Automation {sw.ElapsedMilliseconds} мс (процесс жив)");
            return main;
        }

        /// <summary>
        /// Полоса вкладок (единственный потомок tabHostView) без подмены сбоев пустым результатом, в отличие от <see cref="TabStrip"/>:
        /// клиент занят - повтор, затем <see cref="ClientBusyException"/>; структуры нет - исключение.
        /// </summary>
        internal AutomationElement TabStripStrict()
        {
            Window main = MainWindowStrict();
            AutomationElement host = UiQuery.TryFind(main, By.Id(TabHostId), Wait.DefaultTimeout, 1)
                ?? throw new InvalidOperationException($"В главном окне клиента нет {TabHostId}.");
            return UiQuery.Read(() => host.FindAllChildren().FirstOrDefault(), Wait.DefaultTimeout, "полоса вкладок")
                ?? throw new InvalidOperationException($"У {TabHostId} нет полосы вкладок.");
        }

        /// <summary>Заголовки вкладок по полному обходу полосы (в отличие от <see cref="Tabs"/>, занятость клиента не даёт пустой список).</summary>
        internal IReadOnlyList<AutomationElement> TabHeaders()
        {
            return UiQuery.FindAllComplete(TabStripStrict(), By.Id(TabItemId), Wait.DefaultTimeout, 1);
        }

        /// <summary>Контейнер содержимого вкладок (класс TabView) по полному обходу полосы; null - его нет.</summary>
        internal AutomationElement TabViewStrict()
        {
            return UiQuery.FindAllComplete(TabStripStrict(), By.ClassName("TabView"), Wait.DefaultTimeout, 1).FirstOrDefault();
        }

        /// <summary>
        /// В этом сеансе клиента открывалось представление модуля (<see cref="PilotModuleView"/>): после этого дерево UI Automation
        /// главного окна может не обновляться до перезапуска клиента (проверено на реестре замечаний, Pilot-ICE 26.11).
        /// </summary>
        public bool ModuleViewOpened => _moduleViewOpened;

        internal void MarkModuleViewOpened()
        {
            if (!_moduleViewOpened)
            {
                PilotLog.Action("открыто представление модуля: дерево UI Automation главного окна может перестать обновляться до перезапуска клиента");
            }
            _moduleViewOpened = true;
        }

        /// <summary>
        /// Показать объект в клиенте: exe текущего клиента с аргументом «piloturi://&lt;id&gt;» (для замечания - «piloturi://&lt;документ&gt;/-1/&lt;замечание&gt;»),
        /// запуск без оболочки (обработчик протокола в Windows может быть зарегистрирован на другой клиент). Второй экземпляр передаёт команду
        /// работающему клиенту и завершается; ожидается код 0 и команда в ItemType tabHostView. Клиент показывает объект в активной вкладке
        /// проводника, иначе в другой вкладке проводника, иначе в новой; строка объекта выделяется и получает фокус.
        /// </summary>
        public void ShowObject(Guid objectId, Guid? annotationId = null, TimeSpan? timeout = null)
        {
            TimeSpan limit = timeout ?? _config.DefaultTimeout;
            string uri = annotationId == null
                ? "piloturi://" + objectId.ToString("D")
                : "piloturi://" + objectId.ToString("D") + "/-1/" + annotationId.Value.ToString("D");
            string expected = "mniShowDocumentCommand:" + objectId.ToString("D");
            PilotLog.Action("переход к объекту: " + uri);
            RunClientCommand(uri, limit);
            Wait.Strict(() => State.LastCommand.StartsWith(expected, StringComparison.OrdinalIgnoreCase), limit,
                "клиент принял команду перехода к объекту " + objectId, () => "последняя команда: " + State.LastCommand);
        }

        /// <summary>Штатно закрыть клиент (exe с аргументом «-quit») и дождаться завершения процесса. Не завершился - исключение (без Kill).</summary>
        public void Quit(TimeSpan? timeout = null)
        {
            TimeSpan limit = timeout ?? _config.ClientStartTimeout;
            PilotLog.Action("закрытие клиента: -quit");
            RunClientCommand("-quit", limit);
            if (!Process.WaitForExit((int)limit.TotalMilliseconds))
            {
                throw new PilotWaitTimeoutException("завершение процесса клиента после «-quit» (открыто модальное окно?)", limit, limit, null, null);
            }
        }

        /// <summary>Закрыть клиент (<see cref="Quit"/>), запустить заново и дождаться готовности. Этот объект освобождается.</summary>
        public PilotApp Restart()
        {
            PilotLog.Action("перезапуск клиента");
            Quit();
            Dispose();
            PilotUiConfig startConfig = _config;
            return StartOrAttach(startConfig);
        }

        /// <summary>
        /// Вывести главное окно клиента на передний план (с обходом запрета Windows на смену переднего плана фоновым
        /// процессом). Нужен перед <see cref="SendKeys"/>, когда пользователь работает в другом окне.
        /// </summary>
        public void BringToFront()
        {
            AutomationElement main = MainWindow ?? throw new InvalidOperationException("Главное окно клиента не найдено.");
            UiAct.BringToFront(main);
        }

        /// <summary>Нажать клавиши в клиенте без смены фокуса (например, «Меню» для выделенной строки). Фокус должен быть в окне клиента.</summary>
        public void SendKeys(string description, params VirtualKeyShort[] keys)
        {
            UiAct.KeysToForeground(Process.Id, description, keys);
        }

        private void RunClientCommand(string argument, TimeSpan timeout)
        {
            ProcessStartInfo info = new ProcessStartInfo(_config.ResolvedExePath, "\"" + argument + "\"") { UseShellExecute = false };
            using (Process second = Process.Start(info))
            {
                if (second == null)
                {
                    throw new InvalidOperationException("Не удалось запустить " + _config.ResolvedExePath + " " + argument);
                }
                if (!second.WaitForExit((int)timeout.TotalMilliseconds))
                {
                    throw new PilotWaitTimeoutException($"передача команды «{argument}» работающему клиенту (второй экземпляр не завершился)", timeout, timeout, null, null);
                }
                if (second.ExitCode != 0)
                {
                    throw new InvalidOperationException($"Второй экземпляр клиента с аргументом «{argument}» завершился с кодом {second.ExitCode}.");
                }
            }
        }

        /// <summary>Диагностика для сообщений о таймаутах: окна ошибок Pilot и пометка об устаревающем дереве.</summary>
        private string DescribeForDiagnostics()
        {
            List<string> parts = new List<string>();
            if (Process.HasExited)
            {
                return "Процесс клиента Pilot завершён (код " + Process.ExitCode + ").";
            }
            if (_moduleViewOpened)
            {
                parts.Add("В этом сеансе клиента открывалось представление модуля: дерево UI Automation главного окна могло перестать обновляться (нужен перезапуск клиента).");
            }
            parts.AddRange(Windows.DescribeErrorsOnce());
            return string.Join(Environment.NewLine, parts);
        }
    }
}
