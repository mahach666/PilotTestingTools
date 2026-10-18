using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using PilotUITesting.Data;

namespace PilotUITesting
{
    /// <summary>
    /// Клиент Pilot, в котором выполняются тесты.
    /// </summary>
    public enum PilotClientKind
    {
        PilotIce,
        PilotBim,
    }

    /// <summary>Что делать с окнами «Отправка изменений» (сервер не принял изменения) при уборке окон перед тестом.</summary>
    public enum UnsentChangesPolicy
    {
        /// <summary>Остановить тест с текстом окна: решать, стирать ли изменения, должен человек.</summary>
        Fail,

        /// <summary>Нажать «Удалить изменения» - стирает ВСЕ неотправленные изменения клиента, в том числе пользовательские.</summary>
        Discard,
    }

    /// <summary>Учётная запись базы Pilot (логин и пароль) - для подключения Server API под другим пользователем.</summary>
    public sealed class PilotAccount
    {
        public string Login { get; set; }

        public string Password { get; set; }
    }

    /// <summary>
    /// Настройки запуска UI-тестов. Источник (по убыванию приоритета): переменные окружения PILOT_UI_*,
    /// файл pilot-ui-tests.local.json (не хранится в git: учётки), файл pilot-ui-tests.json рядом с тестовой сборкой, значения по умолчанию.
    /// </summary>
    public sealed class PilotUiConfig
    {
        public const string DefaultBimExe = @"C:\Program Files\ASCON\Pilot-BIM\Ascon.Pilot.PilotBIM.exe";

        public const string DefaultIceExe = @"C:\Program Files\ASCON\Pilot-ICE Enterprise\Ascon.Pilot.PilotEnterprise.exe";

        public const string FileName = "pilot-ui-tests.json";

        public const string LocalFileName = "pilot-ui-tests.local.json";

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PilotClientKind Client { get; set; } = PilotClientKind.PilotBim;

        /// <summary>Путь к exe клиента; пусто - стандартный путь для <see cref="Client"/>.</summary>
        public string ExePath { get; set; }

        /// <summary>Адрес сервера с именем базы, например http://host:5545/database.</summary>
        public string ServerUrl { get; set; } = "http://localhost:5545/database";

        /// <summary>Логин для входа в клиент и для Server API (тестовые данные).</summary>
        public string Login { get; set; } = "mcp";

        public string Password { get; set; } = "mcp";

        /// <summary>Логин отдельной учётки для Server API; пусто - тот же, что <see cref="Login"/>.</summary>
        public string ApiLogin { get; set; }

        public string ApiPassword { get; set; }

        /// <summary>
        /// Дополнительные учётки базы по имени (ключ "accounts": { "имя": { "login": …, "password": … } }) - подключения Server API
        /// от имени других пользователей (PilotUiTestBase.ServerAs). Пароли держать в pilot-ui-tests.local.json.
        /// Переменные PILOT_UI_ACCOUNT_&lt;ИМЯ&gt;_LOGIN / _PASSWORD перекрывают файл. Имена без учёта регистра.
        /// </summary>
        public Dictionary<string, PilotAccount> Accounts { get; set; } = new Dictionary<string, PilotAccount>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Администратор сервера Pilot (не базы): вход LoginServerAdministrator для административного API. Пусто - не задан.</summary>
        public string AdminLogin { get; set; }

        public string AdminPassword { get; set; }

        /// <summary>Не закрывать клиент после тестов (удобно при отладке).</summary>
        public bool KeepClientOpen { get; set; } = true;

        /// <summary>Переиспользовать уже запущенный клиент вместо запуска нового.</summary>
        public bool AttachToRunning { get; set; } = true;

        /// <summary>Папка для скриншотов и дампов дерева при падениях; пусто - %TEMP%\PilotUITesting\artifacts.</summary>
        public string ArtifactsDir { get; set; }

        public int DefaultTimeoutSeconds { get; set; } = 30;

        public int ClientStartTimeoutSeconds { get; set; } = 120;

        /// <summary>Сколько ждать данные сервера в интерфейсе и загрузку модулей (реестр замечаний грузится до 35 с).</summary>
        public int DataTimeoutSeconds { get; set; } = 90;

        /// <summary>
        /// Под кем должен быть клиент: подпись кнопки профиля (ItemStatus AUIDCurrentUserProfile) - «отображаемое имя (должность)»,
        /// например «bot2 (bot)». Задана - StartOrAttach сверяет точно и при расхождении падает. Логина клиент в UI Automation не показывает.
        /// </summary>
        public string ClientUser { get; set; }

        /// <summary>
        /// Включить осведомлённость процесса тестов о DPI (Per-Monitor V2) до создания UI Automation. Нужна для кликов мышью
        /// на экране с масштабом больше 100 %. По умолчанию выключена, чтобы не менять поведение существующих проектов.
        /// </summary>
        public bool DpiAware { get; set; }

        /// <summary>Перед каждым тестом закрывать меню и окна, оставшиеся от упавшего теста (с записью в журнал).</summary>
        public bool ResetUiBeforeEachTest { get; set; }

        /// <summary>Что делать с окнами «Отправка изменений» при уборке окон.</summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public UnsentChangesPolicy UnsentChanges { get; set; } = UnsentChangesPolicy.Fail;

        /// <summary>
        /// После того как в сеансе открывалось представление модуля (вкладка-HwndHost, например реестр замечаний), дерево UI Automation
        /// главного окна перестаёт обновляться до перезапуска клиента. true - базовый класс перезапускает клиент перед следующим тестом.
        /// </summary>
        public bool RestartAfterModuleView { get; set; } = true;

        /// <summary>
        /// Подключаясь к уже запущенному клиенту, перезапустить его (-quit и запуск). Нужно, если в клиенте могли открывать представления
        /// модулей (дерево UI Automation главного окна тогда устаревает, а по самому дереву этого не определить). По умолчанию выключено.
        /// </summary>
        public bool RestartOnAttach { get; set; }

        /// <summary>Имена журналов модулей (Units Logs, без .log) для проверки ошибок и артефактов.</summary>
        public string[] PluginLogs { get; set; } = new string[0];

        /// <summary>Сколько последних папок артефактов хранить.</summary>
        public int ArtifactsKeep { get; set; } = 50;

        /// <summary>Типы песочницы тестовых данных (<see cref="PilotSandbox"/>).</summary>
        public PilotSandboxConfig Sandbox { get; set; }

        /// <summary>Ключи файлов настроек, которых библиотека не знает (опечатка или устаревший ключ).</summary>
        [JsonIgnore]
        public IReadOnlyList<string> UnknownKeys { get; private set; } = new string[0];

        /// <summary>Откуда взяты значения (файлы и переменные окружения) - для журнала.</summary>
        [JsonIgnore]
        public IReadOnlyList<string> Sources { get; private set; } = new string[0];

        public string ResolvedExePath => string.IsNullOrWhiteSpace(ExePath)
            ? (Client == PilotClientKind.PilotBim ? DefaultBimExe : DefaultIceExe)
            : ExePath;

        public string ProcessName => Path.GetFileNameWithoutExtension(ResolvedExePath);

        public string ResolvedApiLogin => string.IsNullOrWhiteSpace(ApiLogin) ? Login : ApiLogin;

        public string ResolvedApiPassword => string.IsNullOrWhiteSpace(ApiLogin) ? Password : ApiPassword;

        public string ResolvedArtifactsDir => string.IsNullOrWhiteSpace(ArtifactsDir)
            ? Path.Combine(Path.GetTempPath(), "PilotUITesting", "artifacts")
            : ArtifactsDir;

        public TimeSpan DefaultTimeout => TimeSpan.FromSeconds(DefaultTimeoutSeconds);

        public TimeSpan DataTimeout => TimeSpan.FromSeconds(DataTimeoutSeconds);

        public TimeSpan ClientStartTimeout => TimeSpan.FromSeconds(ClientStartTimeoutSeconds);

        /// <summary>Имя базы из <see cref="ServerUrl"/>.</summary>
        public string DatabaseName
        {
            get
            {
                Uri uri = new Uri(ServerUrl);
                return uri.AbsolutePath.Trim('/');
            }
        }

        public static PilotUiConfig Load(string baseDirectory = null)
        {
            string dir = baseDirectory ?? AppDomain.CurrentDomain.BaseDirectory;
            List<string> sources = new List<string>();
            JsonObject merged = new JsonObject();
            foreach (string name in new[] { FileName, LocalFileName })
            {
                string file = Path.Combine(dir, name);
                if (!File.Exists(file))
                {
                    continue;
                }
                JsonNode node = JsonNode.Parse(File.ReadAllText(file), null, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
                if (!(node is JsonObject obj))
                {
                    throw new InvalidDataException($"{file}: ожидается JSON-объект");
                }
                Overlay(merged, obj);
                sources.Add(file);
            }
            JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true };
            PilotUiConfig config = JsonSerializer.Deserialize<PilotUiConfig>(merged.ToJsonString(), options) ?? new PilotUiConfig();
            // сериализатор создаёт словарь со сравнением по умолчанию - имена учёток без учёта регистра
            config.Accounts = new Dictionary<string, PilotAccount>(config.Accounts ?? new Dictionary<string, PilotAccount>(), StringComparer.OrdinalIgnoreCase);
            config.UnknownKeys = UnknownKeysOf(merged);
            sources.AddRange(config.ApplyEnvironment());
            config.Sources = sources;
            return config;
        }

        /// <summary>Проблемы конфигурации (печатаются базовым классом; тест из-за них не падает).</summary>
        public IReadOnlyList<string> Validate()
        {
            List<string> problems = new List<string>();
            if (string.IsNullOrWhiteSpace(ServerUrl))
            {
                problems.Add("не задан serverUrl");
            }
            foreach (string key in UnknownKeys)
            {
                problems.Add($"неизвестный ключ «{key}» (опечатка?)");
            }
            if (DefaultTimeoutSeconds <= 0 || DataTimeoutSeconds <= 0 || ClientStartTimeoutSeconds <= 0)
            {
                problems.Add("таймауты должны быть положительными");
            }
            if (Sandbox != null)
            {
                problems.AddRange(Sandbox.Validate().Select(p => "sandbox: " + p));
            }
            foreach (KeyValuePair<string, PilotAccount> account in Accounts)
            {
                if (account.Value == null || string.IsNullOrWhiteSpace(account.Value.Login) || string.IsNullOrEmpty(account.Value.Password))
                {
                    problems.Add($"accounts: у учётки «{account.Key}» нет логина или пароля");
                }
            }
            if (string.IsNullOrWhiteSpace(AdminLogin) != string.IsNullOrEmpty(AdminPassword))
            {
                problems.Add("adminLogin и adminPassword задаются вместе");
            }
            return problems;
        }

        /// <summary>Конфигурация одной строкой без паролей - для журнала.</summary>
        public string Describe()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"клиент {Client} ({ResolvedExePath}); сервер {ServerUrl}; Server API под «{ResolvedApiLogin}»");
            if (!string.IsNullOrEmpty(ClientUser))
            {
                sb.Append($"; клиент под «{ClientUser}»");
            }
            if (Accounts.Count > 0)
            {
                sb.Append("; учётки: ").Append(string.Join(", ", Accounts.Keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase)));
            }
            if (!string.IsNullOrWhiteSpace(AdminLogin))
            {
                sb.Append($"; администратор сервера «{AdminLogin}»");
            }
            sb.Append($"; таймауты {DefaultTimeoutSeconds}/{DataTimeoutSeconds}/{ClientStartTimeoutSeconds} с");
            sb.Append($"; dpiAware={DpiAware}; resetUiBeforeEachTest={ResetUiBeforeEachTest}; restartOnAttach={RestartOnAttach}; restartAfterModuleView={RestartAfterModuleView}; unsentChanges={UnsentChanges}");
            if (Sources.Count > 0)
            {
                sb.Append("; источники: ").Append(string.Join(", ", Sources));
            }
            return sb.ToString();
        }

        private static void Overlay(JsonObject target, JsonObject source)
        {
            foreach (KeyValuePair<string, JsonNode> pair in source.ToList())
            {
                string existing = target.Select(p => p.Key).FirstOrDefault(k => string.Equals(k, pair.Key, StringComparison.OrdinalIgnoreCase));
                if (existing != null)
                {
                    target.Remove(existing);
                }
                JsonNode value = pair.Value;
                source.Remove(pair.Key);
                target[pair.Key] = value;
            }
        }

        private static IReadOnlyList<string> UnknownKeysOf(JsonObject merged)
        {
            HashSet<string> known = new HashSet<string>(
                typeof(PilotUiConfig).GetProperties().Where(p => p.CanWrite && p.GetSetMethod() != null).Select(p => p.Name),
                StringComparer.OrdinalIgnoreCase);
            return merged.Select(p => p.Key).Where(k => !known.Contains(k)).ToList();
        }

        private IEnumerable<string> ApplyEnvironment()
        {
            List<string> used = new List<string>();
            string client = Env("PILOT_UI_CLIENT", null, used);
            if (!string.IsNullOrWhiteSpace(client))
            {
                Client = client.IndexOf("ice", StringComparison.OrdinalIgnoreCase) >= 0 ? PilotClientKind.PilotIce : PilotClientKind.PilotBim;
            }
            ExePath = Env("PILOT_UI_EXE", ExePath, used);
            ServerUrl = Env("PILOT_UI_SERVER_URL", ServerUrl, used);
            Login = Env("PILOT_UI_LOGIN", Login, used);
            Password = Env("PILOT_UI_PASSWORD", Password, used);
            ApiLogin = Env("PILOT_UI_API_LOGIN", ApiLogin, used);
            ApiPassword = Env("PILOT_UI_API_PASSWORD", ApiPassword, used);
            ArtifactsDir = Env("PILOT_UI_ARTIFACTS", ArtifactsDir, used);
            AdminLogin = Env("PILOT_UI_ADMIN_LOGIN", AdminLogin, used);
            AdminPassword = Env("PILOT_UI_ADMIN_PASSWORD", AdminPassword, used);
            ApplyAccountEnvironment(used);
            ClientUser = Env("PILOT_UI_CLIENT_USER", ClientUser, used);
            KeepClientOpen = Flag("PILOT_UI_KEEP_CLIENT", KeepClientOpen, used);
            AttachToRunning = Flag("PILOT_UI_ATTACH", AttachToRunning, used);
            RestartOnAttach = Flag("PILOT_UI_RESTART_ON_ATTACH", RestartOnAttach, used);
            DpiAware = Flag("PILOT_UI_DPI_AWARE", DpiAware, used);
            ResetUiBeforeEachTest = Flag("PILOT_UI_RESET_UI", ResetUiBeforeEachTest, used);
            string dataTimeout = Env("PILOT_UI_DATA_TIMEOUT", null, used);
            if (!string.IsNullOrWhiteSpace(dataTimeout))
            {
                if (!int.TryParse(dataTimeout, out int seconds) || seconds <= 0)
                {
                    throw new FormatException("PILOT_UI_DATA_TIMEOUT: ожидается число секунд, получено «" + dataTimeout + "»");
                }
                DataTimeoutSeconds = seconds;
            }
            string unsent = Env("PILOT_UI_UNSENT_CHANGES", null, used);
            if (!string.IsNullOrWhiteSpace(unsent))
            {
                if (!Enum.TryParse(unsent, true, out UnsentChangesPolicy policy) || !Enum.IsDefined(typeof(UnsentChangesPolicy), policy))
                {
                    throw new FormatException("PILOT_UI_UNSENT_CHANGES: ожидается Fail или Discard, получено «" + unsent + "»");
                }
                UnsentChanges = policy;
            }
            return used.Count == 0 ? new string[0] : new[] { "окружение: " + string.Join(", ", used) };
        }

        /// <summary>PILOT_UI_ACCOUNT_&lt;ИМЯ&gt;_LOGIN / _PASSWORD: создаёт или перекрывает учётку &lt;ИМЯ&gt;.</summary>
        private void ApplyAccountEnvironment(List<string> used)
        {
            const string prefix = "PILOT_UI_ACCOUNT_";
            foreach (string variable in Environment.GetEnvironmentVariables().Keys.Cast<string>().OrderBy(k => k, StringComparer.Ordinal))
            {
                if (!variable.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                string rest = variable.Substring(prefix.Length);
                bool isLogin = rest.EndsWith("_LOGIN", StringComparison.OrdinalIgnoreCase);
                bool isPassword = rest.EndsWith("_PASSWORD", StringComparison.OrdinalIgnoreCase);
                if (!isLogin && !isPassword)
                {
                    continue;
                }
                string name = rest.Substring(0, rest.Length - (isLogin ? "_LOGIN".Length : "_PASSWORD".Length));
                string value = Environment.GetEnvironmentVariable(variable);
                if (name.Length == 0 || string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }
                if (!Accounts.TryGetValue(name, out PilotAccount account) || account == null)
                {
                    account = new PilotAccount();
                    Accounts[name] = account;
                }
                if (isLogin)
                {
                    account.Login = value;
                }
                else
                {
                    account.Password = value;
                }
                used.Add(variable);
            }
        }

        private static string Env(string name, string fallback, List<string> used)
        {
            string value = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrWhiteSpace(value))
            {
                return fallback;
            }
            used.Add(name);
            return value;
        }

        private static bool Flag(string name, bool fallback, List<string> used)
        {
            string value = Env(name, null, used);
            if (string.IsNullOrWhiteSpace(value))
            {
                return fallback;
            }
            return value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase);
        }
    }
}
