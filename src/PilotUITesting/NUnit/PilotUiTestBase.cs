using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using PilotUITesting.Client;
using PilotUITesting.Data;
using PilotUITesting.Uia;

namespace PilotUITesting.NUnit
{
    /// <summary>Сколько живёт вкладка или представление модуля, открытые тестом.</summary>
    public enum TabLifetime
    {
        /// <summary>Закрывается после теста.</summary>
        Test,

        /// <summary>Закрывается после всех тестов класса.</summary>
        Fixture,
    }

    /// <summary>
    /// Базовый класс NUnit-фикстуры: один клиент Pilot и одно подключение Server API на класс тестов,
    /// артефакты (экран, дерево окон, журнал шагов, окна ошибок Pilot, журналы модулей) при падении теста.
    /// Модуль, открывающийся плиткой «Новой вкладки», задаёт <see cref="ModuleButtonTitle"/>; модуль, открывающийся иначе
    /// (например, пунктом меню проводника), его не задаёт и открывает своё представление сам (<see cref="PilotModuleView.OpenedBy"/>).
    /// Новое в 0.2: <see cref="TestData"/>, <see cref="Step"/>, <see cref="OpenExplorer"/>, <see cref="Track{T}"/>, <see cref="RestoreFileAfterTests"/>,
    /// <see cref="AssertNoPilotErrors"/>, <see cref="AssertNoPluginErrors"/>. Всё, что меняет поведение прежних тестов (уборка окон перед
    /// тестом, DPI, перезапуск клиента), включается только ключами pilot-ui-tests.json.
    /// </summary>
    public abstract class PilotUiTestBase
    {
        private readonly List<(PilotTab Tab, TabLifetime Lifetime)> _tabs = new List<(PilotTab, TabLifetime)>();
        private readonly List<(PilotModuleView View, TabLifetime Lifetime)> _views = new List<(PilotModuleView, TabLifetime)>();
        private readonly List<LocalFileBackup> _files = new List<LocalFileBackup>();
        private readonly Dictionary<string, PilotServerClient> _accounts = new Dictionary<string, PilotServerClient>(StringComparer.OrdinalIgnoreCase);
        private readonly List<(string Description, Action Restore)> _afterTest = new List<(string, Action)>();
        private readonly List<(string Description, Action Restore)> _afterTests = new List<(string, Action)>();
        private string _clientRestartReason;
        private PilotSandbox _testData;
        private PluginLogWatch _pluginLogs;
        private DateTime _testStartedUtc;
        private bool _setupCompleted;

        protected PilotUiConfig Config { get; private set; }

        protected PilotApp App { get; private set; }

        protected PilotServerClient Server { get; private set; }

        /// <summary>Название кнопки модуля на странице «Новая вкладка» (для <see cref="OpenModuleTab"/>); null - модуль открывается иначе.</summary>
        protected virtual string ModuleButtonTitle => null;

        protected TimeSpan Timeout => Config.DefaultTimeout;

        /// <summary>Сколько ждать данные сервера в интерфейсе и загрузку модулей (dataTimeoutSeconds).</summary>
        protected TimeSpan DataTimeout => Config.DataTimeout;

        /// <summary>Песочница тестовых данных класса (секция "sandbox" pilot-ui-tests.json): создаётся при первом обращении, убирается после всех тестов класса.</summary>
        protected PilotSandbox TestData => _testData ?? (_testData = PilotSandbox.Create(Server, Config.Sandbox, GetType().Name));

        /// <summary>Перед каждым тестом закрывать меню и окна, оставшиеся от упавшего теста (по умолчанию - ключ resetUiBeforeEachTest).</summary>
        protected virtual bool ResetUiBeforeEachTest => Config.ResetUiBeforeEachTest;

        [OneTimeSetUp]
        public void PilotOneTimeSetUp()
        {
            Config = PilotUiConfig.Load(TestContext.CurrentContext.TestDirectory);
            PilotLog.Sink = line => TestContext.Progress.WriteLine(line);
            PilotLog.Action("конфигурация: " + Config.Describe());
            foreach (string problem in Config.Validate())
            {
                PilotLog.Warn("конфигурация: " + problem);
            }
            if (string.IsNullOrWhiteSpace(Config.ResolvedApiLogin))
            {
                throw new InvalidOperationException("Не задана учётная запись Server API: login и password (или apiLogin и apiPassword) в "
                    + PilotUiConfig.LocalFileName + " рядом с тестами или переменные PILOT_UI_API_LOGIN и PILOT_UI_API_PASSWORD.");
            }
            Server = PilotServerClient.Connect(Config.ServerUrl, Config.ResolvedApiLogin, Config.ResolvedApiPassword);
            Server.DefaultDataTimeout = Config.DataTimeout;
            App = PilotApp.StartOrAttach(Config);
            _pluginLogs = new PluginLogWatch(PluginLogWatch.DefaultDirectory(Config.Client), Config.PluginLogs ?? new string[0]);
            TestArtifacts.Prune(Config);
            _setupCompleted = true;
            OnBeforeTests();
        }

        [OneTimeTearDown]
        public void PilotOneTimeTearDown()
        {
            List<Exception> errors = new List<Exception>();
            if (_setupCompleted)
            {
                Try(OnAfterTests, errors);
            }
            RunRestores(_afterTests, errors);
            if (App != null)
            {
                Try(() => CloseViews(TabLifetime.Fixture), errors);
                if (NeedsRestart(out string _))
                {
                    // клиент оставляем со свежим деревом UI Automation (следующий прогон подключится к нему); «свои» вкладки закрываются после перезапуска
                    Try(RestartClient, errors);
                }
                else
                {
                    Try(() => CloseTabs(null), errors);
                }
            }
            foreach (LocalFileBackup backup in _files)
            {
                Try(backup.Restore, errors);
            }
            if (_testData != null)
            {
                Try(_testData.Dispose, errors);
            }
            Try(() => App?.Dispose(), errors);
            foreach (PilotServerClient account in _accounts.Values)
            {
                Try(account.Dispose, errors);
            }
            _accounts.Clear();
            Try(() => Server?.Dispose(), errors);
            App = null;
            Server = null;
            if (errors.Count == 1)
            {
                ExceptionDispatchInfo.Capture(errors[0]).Throw();
            }
            if (errors.Count > 1)
            {
                throw new AggregateException("Уборка после тестов класса завершилась ошибками", errors);
            }
        }

        /// <summary>Перед тестом: журнал, метки журналов модулей, при необходимости - перезапуск клиента и уборка окон.</summary>
        [SetUp]
        public void PilotSetUp()
        {
            if (App == null)
            {
                return;
            }
            PilotLog.BeginTest(TestContext.CurrentContext.Test.FullName);
            _testStartedUtc = DateTime.UtcNow;
            _pluginLogs?.Mark();
            if (NeedsRestart(out string reason))
            {
                PilotLog.Action("перезапуск клиента перед тестом: " + reason);
                RestartClient();
            }
            if (ResetUiBeforeEachTest)
            {
                UiResetReport report = App.Windows.ResetUi(new UiResetOptions
                {
                    CancelButtonIds = new[] { PilotIds.MessageBox.ClientCancel, PilotIds.MessageBox.Cancel }.Concat(DialogCancelButtonIds ?? new string[0]).Distinct().ToList(),
                    UnsentChanges = Config.UnsentChanges,
                });
                if (!report.Clean)
                {
                    throw new InvalidOperationException("Перед тестом не удалось закрыть окна: " + string.Join("; ", report.Remaining));
                }
            }
        }

        [TearDown]
        public void PilotTearDown()
        {
            if (App == null)
            {
                return;
            }
            TestStatus status = TestContext.CurrentContext.Result.Outcome.Status;
            if (status == TestStatus.Failed || status == TestStatus.Inconclusive)
            {
                SaveFailureArtifacts();
            }
            List<Exception> errors = new List<Exception>();
            if (ResetUiBeforeEachTest)
            {
                Try(() => App.Windows.ResetUi(new UiResetOptions { UnsentChanges = Config.UnsentChanges }), errors);
            }
            Try(() => CloseViews(TabLifetime.Test), errors);
            Try(() => CloseTabs(TabLifetime.Test), errors);
            RunRestores(_afterTest, errors);
            ReportProblems();
            if (errors.Count == 1)
            {
                ExceptionDispatchInfo.Capture(errors[0]).Throw();
            }
            if (errors.Count > 1)
            {
                throw new AggregateException("Уборка после теста завершилась ошибками", errors);
            }
        }

        /// <summary>Подготовка данных/клиента перед всеми тестами класса (после подключения).</summary>
        protected virtual void OnBeforeTests()
        {
        }

        /// <summary>Очистка после всех тестов класса (до отключения). Не вызывается, если подготовка класса не завершилась.</summary>
        protected virtual void OnAfterTests()
        {
        }

        /// <summary>Открыть вкладку модуля со страницы «Новая вкладка».</summary>
        protected void OpenModuleTab()
        {
            if (string.IsNullOrEmpty(ModuleButtonTitle))
            {
                throw new InvalidOperationException("ModuleButtonTitle не задан: модуль открывается не плиткой «Новой вкладки» (см. PilotModuleView.OpenedBy).");
            }
            App.OpenModuleFromNewTab(ModuleButtonTitle);
        }

        /// <summary>AutomationId кнопок отмены модальных окон модуля и Pilot (закрываются перед закрытием вкладки).</summary>
        protected virtual string[] DialogCancelButtonIds => new[] { "btnCancel" };

        /// <summary>Закрыть модальные окна и вкладку модуля, если открыта (заголовок вкладки = название модуля).</summary>
        protected void CloseModuleTab()
        {
            App.CloseDialogs(DialogCancelButtonIds);
            App.CloseTab(ModuleButtonTitle);
            App.CloseTab(PilotApp.NewTabTitle);
            App.CloseDialogs(DialogCancelButtonIds);
        }

        /// <summary>Шаг сценария в журнал (виден в выводе теста и в артефактах при падении).</summary>
        protected void Step(string text)
        {
            PilotLog.Step(text);
        }

        /// <summary>Пропустить тесты класса (Assert.Ignore), если в базе нет нужных типов.</summary>
        protected void RequireTypes(params string[] typeNames)
        {
            string[] missing = typeNames.Where(t => !string.IsNullOrEmpty(t) && !Server.HasType(t)).ToArray();
            if (missing.Length > 0)
            {
                Assert.Ignore("В базе нет типов: " + string.Join(", ", missing));
            }
        }

        /// <summary>Своя вкладка «Проекты и документы» (закрывается по окончании теста или класса).</summary>
        protected PilotExplorer OpenExplorer(TabLifetime lifetime = TabLifetime.Fixture)
        {
            return Track(PilotExplorer.Open(App), lifetime);
        }

        /// <summary>Взять вкладку (PilotTabPage) или представление модуля (PilotModuleView) на учёт: закроется после теста или класса.</summary>
        protected T Track<T>(T page, TabLifetime lifetime = TabLifetime.Test) where T : PilotPage
        {
            switch (page)
            {
                case PilotModuleView view:
                    _views.Add((view, lifetime));
                    break;
                case PilotTabPage tabPage:
                    _tabs.Add((tabPage.Tab, lifetime));
                    break;
                default:
                    throw new ArgumentException($"{page} не вкладка и не представление модуля - закрывать нечего.", nameof(page));
            }
            return page;
        }

        /// <summary>
        /// Подключение Server API от имени учётки из конфигурации (ключ "accounts"): одно на класс, закрывается после всех тестов.
        /// Учётки нет - тест пропускается (Assert.Ignore) с её именем.
        /// </summary>
        protected PilotServerClient ServerAs(string accountName)
        {
            if (string.IsNullOrWhiteSpace(accountName))
            {
                throw new ArgumentException("Нужно имя учётки", nameof(accountName));
            }
            if (_accounts.TryGetValue(accountName, out PilotServerClient existing))
            {
                return existing;
            }
            if (!Config.Accounts.TryGetValue(accountName, out PilotAccount account) || account == null || string.IsNullOrWhiteSpace(account.Login) || string.IsNullOrEmpty(account.Password))
            {
                Assert.Ignore($"Нужна учётка «{accountName}»: ключ accounts в pilot-ui-tests.local.json или PILOT_UI_ACCOUNT_{accountName.ToUpperInvariant()}_LOGIN/_PASSWORD.");
            }
            PilotServerClient client = PilotServerClient.Connect(Config.ServerUrl, account.Login, account.Password);
            client.DefaultDataTimeout = Config.DataTimeout;
            _accounts[accountName] = client;
            PilotLog.Action($"Server API под учёткой «{accountName}» ({account.Login})");
            return client;
        }

        /// <summary>
        /// Вернуть состояние после теста (в том числе упавшего): действия выполняются в обратном порядке регистрации в TearDown,
        /// после закрытия вкладок теста; ошибки собираются и роняют TearDown.
        /// </summary>
        protected void AfterTest(string description, Action restore)
        {
            _afterTest.Add((description ?? throw new ArgumentNullException(nameof(description)), restore ?? throw new ArgumentNullException(nameof(restore))));
        }

        /// <summary>
        /// Вернуть состояние после всех тестов класса: в обратном порядке, после <see cref="OnAfterTests"/> и до возврата файлов
        /// и уборки песочницы; ошибки собираются.
        /// </summary>
        protected void AfterTests(string description, Action restore)
        {
            _afterTests.Add((description ?? throw new ArgumentNullException(nameof(description)), restore ?? throw new ArgumentNullException(nameof(restore))));
        }

        /// <summary>
        /// Перезапустить клиент перед следующим тестом и в конце класса: состояние процесса клиента изменено (кэши модулей, права
        /// пользователя, заместители), как и после представления модуля. Флаг сбрасывается перезапуском.
        /// </summary>
        protected void RequireClientRestart(string reason)
        {
            _clientRestartReason = string.IsNullOrWhiteSpace(reason) ? throw new ArgumentException("Нужна причина", nameof(reason)) : reason;
            PilotLog.Action("клиент будет перезапущен перед следующим тестом: " + reason);
        }

        /// <summary>Строки журналов модулей (pluginLogs) с начала теста, подходящие под условие (null - все).</summary>
        protected IReadOnlyList<PluginLogLine> PluginLogLines(Func<PluginLogLine, bool> match = null)
        {
            IReadOnlyList<PluginLogLine> lines = _pluginLogs?.NewLines() ?? new PluginLogLine[0];
            return match == null ? lines : lines.Where(match).ToList();
        }

        /// <summary>Дождаться строки журналов модулей (pluginLogs) с начала теста; timeout null - DataTimeout.</summary>
        protected PluginLogLine WaitPluginLog(Func<PluginLogLine, bool> match, TimeSpan? timeout = null, [CallerArgumentExpression("match")] string description = null)
        {
            if (_pluginLogs == null)
            {
                throw new InvalidOperationException("Журналы модулей не отслеживаются: класс не подготовлен.");
            }
            return _pluginLogs.WaitLine(match, timeout ?? DataTimeout, description);
        }

        /// <summary>Снять копию локального файла сейчас и вернуть его после всех тестов класса (с побайтной проверкой).</summary>
        protected void RestoreFileAfterTests(string path)
        {
            _files.Add(LocalFileBackup.Take(path));
        }

        /// <summary>
        /// Перезапустить клиент (-quit и запуск). Вкладки, взятые на учёт, клиент восстанавливает при запуске - они закрываются по типу
        /// содержимого и последнему заголовку (только при единственном совпадении, иначе предупреждение).
        /// </summary>
        protected void RestartClient()
        {
            List<(string ContentId, string Title)> own = _tabs.Select(t => (SafeContentId(t.Tab), t.Tab.LastKnownTitle)).ToList();
            _views.Clear();
            App = App.Restart();
            _clientRestartReason = null;
            _tabs.Clear();
            foreach ((string contentId, string title) in own)
            {
                List<PilotTab> matches = App.TabBar.All().Where(t => t.LastKnownTitle == title && (contentId == null || t.ContentId == contentId)).ToList();
                if (matches.Count == 1)
                {
                    matches[0].Close();
                }
                else if (matches.Count > 1)
                {
                    PilotLog.Warn($"После перезапуска вкладок «{title}» несколько - не закрываю ни одну.");
                }
            }
        }

        /// <summary>Нет открытых окон ошибок Pilot (иначе - провал с заголовками и текстами).</summary>
        protected void AssertNoPilotErrors()
        {
            IReadOnlyList<PilotErrorWindow> errors = App.Windows.Errors();
            if (errors.Count > 0)
            {
                Assert.Fail("Открыты окна ошибок Pilot: " + string.Join("; ", errors.Select(e => $"«{e.Title}»: {e.Message}")));
            }
        }

        /// <summary>В журналах модулей (pluginLogs) с начала теста нет ошибок (по умолчанию - уровень ERROR или FATAL).</summary>
        protected void AssertNoPluginErrors(Func<PluginLogLine, bool> isError = null)
        {
            Func<PluginLogLine, bool> error = isError ?? (line => line.Level == "ERROR" || line.Level == "FATAL");
            PluginLogLine[] found = (_pluginLogs?.NewLines() ?? new PluginLogLine[0]).Where(error).ToArray();
            if (found.Length > 0)
            {
                Assert.Fail("Ошибки в журналах модулей:" + Environment.NewLine + string.Join(Environment.NewLine, found.Select(l => l.ToString())));
            }
        }

        /// <summary>Путь для файла, который создаст тест (например, экспорт): в папке артефактов теста; файла там ещё нет.</summary>
        protected string NewOutputPath(string fileName)
        {
            string folder = Path.Combine(Config.ResolvedArtifactsDir, "output", DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + "_" + TestArtifacts.Sanitize(TestContext.CurrentContext.Test.Name, 80));
            Directory.CreateDirectory(folder);
            string path = Path.Combine(folder, fileName);
            if (File.Exists(path))
            {
                throw new IOException("Файл уже существует: " + path);
            }
            return path;
        }

        private void CloseViews(TabLifetime lifetime)
        {
            foreach ((PilotModuleView view, TabLifetime _) in _views.Where(v => v.Lifetime == lifetime).ToList())
            {
                if (view.IsVisible)
                {
                    view.Close();
                }
                _views.RemoveAll(v => ReferenceEquals(v.View, view));
            }
        }

        /// <summary>Закрыть «свои» вкладки заданного срока жизни (null - все). После представления модуля - через перезапуск клиента.</summary>
        private void CloseTabs(TabLifetime? lifetime)
        {
            List<(PilotTab Tab, TabLifetime Lifetime)> tabs = _tabs.Where(t => lifetime == null || t.Lifetime == lifetime).ToList();
            if (tabs.Count == 0)
            {
                return;
            }
            if (NeedsRestart(out string _))
            {
                // дерево UI Automation главного окна могло устареть: вкладки закрываются после перезапуска клиента -
                // перед следующим тестом (SetUp) или здесь, в конце класса
                if (lifetime == null)
                {
                    RestartClient();
                }
                return;
            }
            foreach ((PilotTab tab, TabLifetime _) in tabs)
            {
                if (tab.IsOpen)
                {
                    tab.Close();
                }
                _tabs.RemoveAll(t => ReferenceEquals(t.Tab, tab));
            }
        }

        /// <summary>Нужен ли перезапуск клиента: открывалось представление модуля (restartAfterModuleView) или тест попросил (<see cref="RequireClientRestart"/>).</summary>
        private bool NeedsRestart(out string reason)
        {
            if (_clientRestartReason != null)
            {
                reason = _clientRestartReason;
                return true;
            }
            if (App.ModuleViewOpened && Config.RestartAfterModuleView)
            {
                reason = "в сеансе открывалось представление модуля (restartAfterModuleView)";
                return true;
            }
            reason = null;
            return false;
        }

        private static void RunRestores(List<(string Description, Action Restore)> restores, List<Exception> errors)
        {
            for (int i = restores.Count - 1; i >= 0; i--)
            {
                (string description, Action restore) = restores[i];
                PilotLog.Action("возврат: " + description);
                Try(() =>
                {
                    try
                    {
                        restore();
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException("Не удалось вернуть «" + description + "»: " + ex.Message, ex);
                    }
                }, errors);
            }
            restores.Clear();
        }

        private void SaveFailureArtifacts()
        {
            TestContext.TestAdapter test = TestContext.CurrentContext.Test;
            TestArtifacts artifacts = TestArtifacts.For(Config, test.ClassName, test.Name, DateTime.Now);
            List<string> saved = new List<string>();
            List<string> failed = new List<string>();
            Save(() => artifacts.WriteText("exception.txt", TestContext.CurrentContext.Result.Message + Environment.NewLine + TestContext.CurrentContext.Result.StackTrace, "Сообщение и стек"), saved, failed, "exception.txt");
            Save(() => artifacts.WriteText("steps.log", PilotLog.Dump(), "Журнал шагов теста"), saved, failed, "steps.log");
            Save(() => artifacts.Screen(), saved, failed, "screen.png");
            Save(() => artifacts.WriteText("pilot-errors.txt", string.Join(Environment.NewLine, App.Windows.Errors().Select(e => $"«{e.Title}» ({e.Id}): {e.Message}")), "Окна ошибок Pilot"), saved, failed, "pilot-errors.txt");
            Save(() => artifacts.WriteText("server-errors.txt", string.Join(Environment.NewLine, Server.ErrorsSince(_testStartedUtc)), "Отказы сервера"), saved, failed, "server-errors.txt");
            Save(() => artifacts.WriteText("plugin-logs.txt", string.Join(Environment.NewLine, _pluginLogs?.NewLines().Select(l => l.ToString()) ?? new string[0]), "Журналы модулей за тест"), saved, failed, "plugin-logs.txt");
            if (App.ModuleViewOpened)
            {
                Save(() => artifacts.WriteText("note.txt", "В сеансе клиента открывалось представление модуля: дерево UI Automation главного окна могло перестать обновляться.", "Примечание"), saved, failed, "note.txt");
            }
            Save(() => artifacts.Windows(App), saved, failed, "windows.txt");
            TestContext.Out.WriteLine("Артефакты: " + artifacts.Folder + (failed.Count > 0 ? "; не сохранены: " + string.Join("; ", failed) : ""));
        }

        private static void Save(Func<string> save, List<string> saved, List<string> failed, string what)
        {
            try
            {
                saved.Add(save());
            }
            catch (Exception ex)
            {
                // артефакт - диагностика: его сбой не должен заслонить причину падения теста, но пропадать молча не должен
                failed.Add(what + ": " + ex);
            }
        }

        /// <summary>Вывести в результат теста окна ошибок Pilot, отказы сервера и ошибки журналов модулей. Сбой диагностики пишется туда же и тест не роняет.</summary>
        private void ReportProblems()
        {
            try
            {
                foreach (PilotErrorWindow error in App.Windows.Errors())
                {
                    TestContext.Out.WriteLine($"Открыто окно ошибки Pilot «{error.Title}»: {error.Message}");
                }
                foreach (ServerError error in Server.ErrorsSince(_testStartedUtc))
                {
                    TestContext.Out.WriteLine("Отказ сервера: " + error);
                }
                foreach (PluginLogLine line in (_pluginLogs?.NewLines() ?? new PluginLogLine[0]).Where(l => l.Level == "ERROR" || l.Level == "FATAL"))
                {
                    TestContext.Out.WriteLine("Журнал модуля: " + line);
                }
            }
            catch (Exception ex)
            {
                TestContext.Out.WriteLine("Диагностика после теста не собрана: " + ex);
            }
        }

        private static string SafeContentId(PilotTab tab)
        {
            try
            {
                return tab.ContentId;
            }
            catch (InvalidOperationException)
            {
                // вкладка уже не видна (устаревшее дерево после представления модуля) - сравнение только по заголовку
                return null;
            }
        }

        private static void Try(Action action, List<Exception> errors)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                errors.Add(ex);
            }
        }
    }
}
