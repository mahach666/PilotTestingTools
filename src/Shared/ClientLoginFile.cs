#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace PilotTestingTools.Shared
{
    /// <summary>
    /// Сохранённый вход клиента Pilot в «%APPDATA%\ASCON\&lt;продукт&gt;\Settings\Application Settings.xml».
    /// Клиент читает пароль только из SafePassword (ConnectionParamsBase.Password - SecureString, открытый &lt;Password&gt; не используется),
    /// без него настройки неполны (ConnectionParamsExtensions.IsValid) и клиент показывает «Необходимо настроить соединение».
    /// Пароль шифрует метод самого клиента ProtectedDataEx.Protect (область CurrentUser), сборка берётся из папки клиента.
    /// Общий исходник библиотеки PilotUITesting (net48) и pilottest (net8).
    /// </summary>
    internal static class ClientLoginFile
    {
        public static string SettingsPath(string productFolder)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ASCON", productFolder, "Settings", "Application Settings.xml");
        }

        /// <summary>Записать вход. Клиент должен быть закрыт: при выходе он перезаписывает файл настроек.</summary>
        public static void Save(string clientExePath, string productFolder, string server, string user, string password)
        {
            Require(server, nameof(server));
            Require(user, nameof(user));
            Require(password, nameof(password));
            string processName = Path.GetFileNameWithoutExtension(clientExePath);
            int session = Process.GetCurrentProcess().SessionId;
            Process? running = Process.GetProcessesByName(processName).FirstOrDefault(p => p.SessionId == session);
            if (running != null)
            {
                throw new InvalidOperationException($"Клиент {processName} запущен (PID {running.Id}): при выходе он перезапишет настройки. Закройте клиент и повторите.");
            }
            string file = SettingsPath(productFolder);
            if (!File.Exists(file))
            {
                throw new FileNotFoundException("Нет файла настроек клиента Pilot: запустите клиент хотя бы раз.", file);
            }
            string clientDir = Path.GetDirectoryName(Path.GetFullPath(clientExePath)) ?? throw new ArgumentException("Не определить папку клиента: " + clientExePath);
            string safePassword = Convert.ToBase64String(Protect(clientDir, password));

            XmlDocument doc = new XmlDocument { PreserveWhitespace = true };
            doc.Load(file);
            Apply(doc, server, user, safePassword, file);
            string temp = file + ".tmp";
            doc.Save(temp);
            File.Replace(temp, file, null);
        }

        /// <summary>Записать параметры подключения в загруженный файл настроек.</summary>
        internal static void Apply(XmlDocument doc, string server, string user, string safePassword, string source)
        {
            XmlNodeList found = doc.SelectNodes("/SerializableObjectInfo/Properties/SerializablePropertyInfo[PropertyName='ConnectionSettings']/Value")
                ?? throw new InvalidDataException($"{source}: не разобрать XML настроек.");
            if (found.Count != 1)
            {
                throw new InvalidDataException($"{source}: ожидалось одно свойство ConnectionSettings, найдено {found.Count}.");
            }
            XmlNode connection = found[0]!;
            Child(connection, "UserName", source).InnerText = user;
            Child(connection, "Server", source).InnerText = server;
            Child(connection, "SafePassword", source).InnerText = safePassword;
            Child(connection, "NeedSavePassword", source).InnerText = "true";
            // открытый пароль клиент не читает - не оставлять его в файле
            ((XmlElement)Child(connection, "Password", source)).IsEmpty = true;
        }

        /// <summary>Пароль, зашифрованный методом клиента (то же, что ConnectionParamsBase.SafePassword).</summary>
        internal static byte[] Protect(string clientDir, string password)
        {
            string commonPath = Path.Combine(clientDir, "Ascon.Pilot.Common.dll");
            if (!File.Exists(commonPath))
            {
                throw new FileNotFoundException("Не найдена сборка клиента Pilot для шифрования пароля.", commonPath);
            }
            ResolveEventHandler resolve = (sender, e) =>
            {
                string candidate = Path.Combine(clientDir, new AssemblyName(e.Name).Name + ".dll");
                return File.Exists(candidate) ? Assembly.LoadFrom(candidate) : null;
            };
            AppDomain.CurrentDomain.AssemblyResolve += resolve;
            try
            {
                Assembly common = Assembly.LoadFrom(commonPath);
                Type protector = common.GetType("Ascon.Pilot.Common.DataProtection.ProtectedDataEx", true)!;
                Type secure = common.GetType("Ascon.Pilot.Common.DataProtection.SecureStringEx", true)!;
                MethodInfo protect = protector.GetMethod("Protect") ?? throw new MissingMethodException(protector.FullName, "Protect");
                MethodInfo convert = secure.GetMethod("ConvertToSecureString") ?? throw new MissingMethodException(secure.FullName, "ConvertToSecureString");
                object scope = Enum.Parse(protect.GetParameters()[1].ParameterType, "CurrentUser");
                object secureString = convert.Invoke(null, new object[] { password }) ?? throw new InvalidOperationException("ConvertToSecureString вернул null.");
                return protect.Invoke(null, new[] { secureString, scope }) as byte[] ?? throw new InvalidOperationException("ProtectedDataEx.Protect вернул не byte[].");
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                throw new InvalidOperationException("Шифрование пароля методом клиента Pilot не удалось: " + ex.InnerException.Message, ex.InnerException);
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= resolve;
            }
        }

        private static XmlNode Child(XmlNode parent, string name, string source)
        {
            return parent.SelectSingleNode(name) ?? throw new InvalidDataException($"{source}: в ConnectionSettings нет элемента {name}.");
        }

        private static void Require(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Не задано: " + name, name);
            }
        }
    }
}
