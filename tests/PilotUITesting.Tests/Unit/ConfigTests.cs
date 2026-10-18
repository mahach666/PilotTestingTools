using System;
using System.IO;
using NUnit.Framework;
using PilotUITesting.Data;

namespace PilotUITesting.Tests.Unit
{
    [TestFixture]
    [NonParallelizable]
    public class ConfigTests
    {
        private static readonly string[] EnvNames =
        {
            "PILOT_UI_CLIENT", "PILOT_UI_EXE", "PILOT_UI_SERVER_URL", "PILOT_UI_LOGIN", "PILOT_UI_PASSWORD", "PILOT_UI_API_LOGIN", "PILOT_UI_API_PASSWORD",
            "PILOT_UI_ARTIFACTS", "PILOT_UI_KEEP_CLIENT", "PILOT_UI_ATTACH", "PILOT_UI_DPI_AWARE", "PILOT_UI_RESET_UI", "PILOT_UI_DATA_TIMEOUT",
            "PILOT_UI_UNSENT_CHANGES", "PILOT_UI_CLIENT_USER", "PILOT_UI_ADMIN_LOGIN", "PILOT_UI_ADMIN_PASSWORD",
            "PILOT_UI_ACCOUNT_EXPERT_LOGIN", "PILOT_UI_ACCOUNT_EXPERT_PASSWORD", "PILOT_UI_ACCOUNT_NEWONE_LOGIN", "PILOT_UI_ACCOUNT_NEWONE_PASSWORD",
        };

        private string _dir;
        private string[] _env;

        [SetUp]
        public void Prepare()
        {
            _dir = Path.Combine(Path.GetTempPath(), "PilotUITesting.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dir);
            _env = Array.ConvertAll(EnvNames, Environment.GetEnvironmentVariable);
            foreach (string name in EnvNames)
            {
                Environment.SetEnvironmentVariable(name, null);
            }
        }

        [TearDown]
        public void Restore()
        {
            for (int i = 0; i < EnvNames.Length; i++)
            {
                Environment.SetEnvironmentVariable(EnvNames[i], _env[i]);
            }
            Directory.Delete(_dir, true);
        }

        [Test]
        public void Accounts_are_read_case_insensitive_and_environment_overrides_them()
        {
            File.WriteAllText(Path.Combine(_dir, PilotUiConfig.FileName), @"{ ""serverUrl"": ""http://host/db"" }");
            File.WriteAllText(Path.Combine(_dir, PilotUiConfig.LocalFileName), @"{
  ""accounts"": { ""expert"": { ""login"": ""e1"", ""password"": ""p1"" }, ""broken"": { ""login"": ""b"" } },
  ""adminLogin"": ""root"", ""adminPassword"": ""secret""
}");
            Environment.SetEnvironmentVariable("PILOT_UI_ACCOUNT_EXPERT_PASSWORD", "p2");
            Environment.SetEnvironmentVariable("PILOT_UI_ACCOUNT_NEWONE_LOGIN", "n1");
            Environment.SetEnvironmentVariable("PILOT_UI_ACCOUNT_NEWONE_PASSWORD", "np");
            PilotUiConfig config = PilotUiConfig.Load(_dir);
            Assert.That(config.UnknownKeys, Is.Empty);
            Assert.That(config.Accounts["EXPERT"].Login, Is.EqualTo("e1"), "имя учётки без учёта регистра");
            Assert.That(config.Accounts["expert"].Password, Is.EqualTo("p2"), "переменная окружения перекрывает файл");
            Assert.That(config.Accounts["newone"].Login, Is.EqualTo("n1"), "учётка только из окружения");
            Assert.That(config.AdminLogin, Is.EqualTo("root"));
            Assert.That(config.Validate(), Has.Some.Contains("broken"), "учётка без пароля - проблема конфигурации");
            Assert.That(config.Describe(), Does.Not.Contain("secret").And.Not.Contain("p2"), "пароли в журнал не пишутся");
            Assert.That(config.Describe(), Does.Contain("expert").And.Contain("root"));
        }

        [Test]
        public void Admin_login_without_password_is_a_problem()
        {
            File.WriteAllText(Path.Combine(_dir, PilotUiConfig.FileName), @"{ ""serverUrl"": ""http://host/db"", ""adminLogin"": ""root"" }");
            Assert.That(PilotUiConfig.Load(_dir).Validate(), Has.Some.Contains("adminLogin"));
        }

        [Test]
        public void Defaults_without_files()
        {
            PilotUiConfig config = PilotUiConfig.Load(_dir);
            Assert.That(config.Client, Is.EqualTo(PilotClientKind.PilotBim));
            Assert.That(config.DataTimeoutSeconds, Is.EqualTo(90));
            Assert.That(config.DpiAware, Is.False, "новое поведение по умолчанию выключено");
            Assert.That(config.ResetUiBeforeEachTest, Is.False);
            Assert.That(config.RestartAfterModuleView, Is.True);
            Assert.That(config.UnknownKeys, Is.Empty);
        }

        [Test]
        public void Consumer_json_of_version_0_1_reads_unchanged()
        {
            // типичный состав pilot-ui-tests.json (значения-заглушки)
            File.WriteAllText(Path.Combine(_dir, PilotUiConfig.FileName), @"{
  // комментарий
  ""client"": ""PilotIce"",
  ""serverUrl"": ""http://host:5545/db"",
  ""login"": ""u"", ""password"": ""p"",
  ""keepClientOpen"": true, ""attachToRunning"": true,
  ""defaultTimeoutSeconds"": 45, ""clientStartTimeoutSeconds"": 180,
}");
            PilotUiConfig config = PilotUiConfig.Load(_dir);
            Assert.That(config.Client, Is.EqualTo(PilotClientKind.PilotIce));
            Assert.That(config.DatabaseName, Is.EqualTo("db"));
            Assert.That(config.DefaultTimeoutSeconds, Is.EqualTo(45));
            Assert.That(config.ResolvedApiLogin, Is.EqualTo("u"));
            Assert.That(config.UnknownKeys, Is.Empty);
            Assert.That(config.Validate(), Is.Empty);
        }

        [Test]
        public void Api_account_has_no_default()
        {
            File.WriteAllText(Path.Combine(_dir, PilotUiConfig.FileName), @"{ ""serverUrl"": ""http://host/db"" }");
            PilotUiConfig config = PilotUiConfig.Load(_dir);
            Assert.That(config.ResolvedApiLogin, Is.Null);
            Assert.That(config.ResolvedApiPassword, Is.Null);
        }

        [Test]
        public void Local_file_overrides_main_and_unknown_keys_are_reported()
        {
            File.WriteAllText(Path.Combine(_dir, PilotUiConfig.FileName), @"{ ""serverUrl"": ""http://host/db"", ""apiLogin"": ""x"", ""dataTimeoutSecnds"": 5,
  ""sandbox"": { ""rootType"": ""support"", ""rootTitleAttribute"": ""support_name"", ""cleanup"": ""Keep"" } }");
            File.WriteAllText(Path.Combine(_dir, PilotUiConfig.LocalFileName), @"{ ""ApiLogin"": ""api-user"", ""apiPassword"": ""secret"" }");
            PilotUiConfig config = PilotUiConfig.Load(_dir);
            Assert.That(config.ResolvedApiLogin, Is.EqualTo("api-user"));
            Assert.That(config.ResolvedApiPassword, Is.EqualTo("secret"));
            Assert.That(config.UnknownKeys, Is.EquivalentTo(new[] { "dataTimeoutSecnds" }));
            Assert.That(config.Validate(), Has.Some.Contains("dataTimeoutSecnds"));
            Assert.That(config.Sandbox.Cleanup, Is.EqualTo(SandboxCleanup.Keep));
            Assert.That(config.Describe(), Does.Not.Contain("secret"));
        }

        [Test]
        public void Environment_wins()
        {
            File.WriteAllText(Path.Combine(_dir, PilotUiConfig.FileName), @"{ ""dpiAware"": false, ""dataTimeoutSeconds"": 10 }");
            Environment.SetEnvironmentVariable("PILOT_UI_DPI_AWARE", "1");
            Environment.SetEnvironmentVariable("PILOT_UI_DATA_TIMEOUT", "120");
            Environment.SetEnvironmentVariable("PILOT_UI_UNSENT_CHANGES", "discard");
            PilotUiConfig config = PilotUiConfig.Load(_dir);
            Assert.That(config.DpiAware, Is.True);
            Assert.That(config.DataTimeoutSeconds, Is.EqualTo(120));
            Assert.That(config.UnsentChanges, Is.EqualTo(UnsentChangesPolicy.Discard));
        }

        [Test]
        public void Bad_environment_value_is_error_not_silence()
        {
            Environment.SetEnvironmentVariable("PILOT_UI_DATA_TIMEOUT", "minute");
            Assert.Throws<FormatException>(() => PilotUiConfig.Load(_dir));
        }
    }
}
