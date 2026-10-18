using System;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;
using PilotTestingTools.Shared;

namespace PilotUITesting.Tests.Unit
{
    /// <summary>Журнал client.log и файл сохранённого входа клиента (общий код библиотеки и pilottest).</summary>
    [TestFixture]
    public class ClientFilesTests
    {
        // Записи - как в client.log Pilot-BIM 26.x
        private const string Blocking = "2026-07-31 13:04:30,495 [RemoteStorageThread_fast] ERROR [RemoteStorageThread] - Возникло иключение, блокирующее попытки переподключения к серверу";
        private const string Denied = "Ascon.Pilot.DataClasses.PilotSecurityAccessDeniedException: The user name or password is incorrect";
        private const string Stack = "   в Ascon.Pilot.Transport.TransportClient.<ProcessResponseAsync>d__24.MoveNext()";
        private const string Info = "2026-07-31 13:04:31,000 [1] INFO  [Context] - context build finish";

        private string _dir;

        [SetUp]
        public void SetUp()
        {
            _dir = Path.Combine(Path.GetTempPath(), "PilotUITesting.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dir);
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(_dir, true);
        }

        [Test]
        public void Exception_after_blocking_record_is_failure()
        {
            ClientLogFile log = ClientLogFile.Since("client.log", DateTime.MinValue);
            log.Feed(Info);
            log.Feed(Blocking);
            log.Feed(Denied);
            log.Feed(Stack);

            Assert.That(log.Failure, Is.EqualTo(Denied));
            Assert.That(ClientLogFile.Explain(log.Failure), Does.StartWith("сервер отклонил сохранённый вход"));
        }

        [Test]
        public void Errors_without_blocking_record_are_not_failure()
        {
            ClientLogFile log = ClientLogFile.Since("client.log", DateTime.MinValue);
            log.Feed("2026-10-18 20:26:26,137 [FileLoadOperation] ERROR [IFileLoadOperation] - Ascon.Pilot.DataClasses.PilotServerException: Could not find a part of the path");
            log.Feed(Denied);

            Assert.That(log.Failure, Is.Null);
        }

        [Test]
        public void Records_before_process_start_are_ignored()
        {
            ClientLogFile log = ClientLogFile.Since("client.log", new DateTime(2026, 7, 31, 13, 4, 31));
            log.Feed(Blocking);
            log.Feed(Denied);
            log.Feed(Info);

            Assert.That(log.Failure, Is.Null);
            Assert.That(log.Tail, Is.EqualTo(new[] { Info }));
        }

        [Test]
        public void FromNow_reads_only_appended_complete_lines()
        {
            string path = Path.Combine(_dir, "client.log");
            File.WriteAllText(path, "\uFEFF" + Blocking + "\r\n" + Denied + "\r\n", new UTF8Encoding(false));
            ClientLogFile log = ClientLogFile.FromNow(path);

            Assert.That(log.Poll(), Is.Null, "старая запись до запуска");

            File.AppendAllText(path, Blocking + "\r\nAscon.Pilot.Server.Api.VersionMatchException: Server version", Encoding.UTF8);
            Assert.That(log.Poll(), Is.Null, "строка исключения ещё не дописана");

            File.AppendAllText(path, " and client do not match\r\n" + Stack + "\r\n", Encoding.UTF8);
            Assert.That(log.Poll(), Is.EqualTo("Ascon.Pilot.Server.Api.VersionMatchException: Server version and client do not match"));
            Assert.That(ClientLogFile.Explain(log.Failure), Does.StartWith("версии клиента и сервера Pilot не совпадают"));
        }

        [Test]
        public void Rotated_log_is_read_from_start()
        {
            string path = Path.Combine(_dir, "client.log");
            File.WriteAllText(path, Info + "\r\n" + Info + "\r\n" + Info + "\r\n");
            ClientLogFile log = ClientLogFile.FromNow(path);

            File.WriteAllText(path, Blocking + "\r\n" + Denied + "\r\n");

            Assert.That(log.Poll(), Is.EqualTo(Denied));
        }

        [Test]
        public void Missing_log_is_not_failure()
        {
            ClientLogFile log = ClientLogFile.FromNow(Path.Combine(_dir, "нет", "client.log"));

            Assert.That(log.Poll(), Is.Null);
            Assert.That(log.ReadError, Is.Null);
        }

        [TestCase(@"C:\Program Files\ASCON\Pilot-ICE Enterprise\Ascon.Pilot.PilotEnterprise.exe", "Pilot-ICE Enterprise")]
        [TestCase(@"D:\Pilot\ascon.pilot.pilotbim.exe", "Pilot-BIM")]
        [TestCase(@"D:\Pilot\Other.exe", null)]
        public void Product_folder_by_exe(string exe, string expected)
        {
            Assert.That(ClientLogFile.ProductFolder(exe), Is.EqualTo(expected));
        }

        private const string Settings = @"<?xml version=""1.0""?>
<SerializableObjectInfo xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <Properties>
    <SerializablePropertyInfo>
      <PropertyName>ConnectionSettings</PropertyName>
      <Value xsi:type=""ConnectionParams"">
        <UserName>old</UserName>
        <Password>открытый</Password>
        <SafePassword>AAAA</SafePassword>
        <Server>http://old:5545/db</Server>
        <Proxy>
          <SafePassword>BBBB</SafePassword>
        </Proxy>
        <NeedSavePassword>false</NeedSavePassword>
      </Value>
    </SerializablePropertyInfo>
  </Properties>
</SerializableObjectInfo>";

        [Test]
        public void Apply_writes_connection_settings_only()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(Settings);

            ClientLoginFile.Apply(doc, "http://host:5545/database", "bot", "c2FmZQ==", "test");

            XmlNode value = doc.SelectSingleNode("//SerializablePropertyInfo/Value");
            Assert.That(value.SelectSingleNode("UserName").InnerText, Is.EqualTo("bot"));
            Assert.That(value.SelectSingleNode("Server").InnerText, Is.EqualTo("http://host:5545/database"));
            Assert.That(value.SelectSingleNode("SafePassword").InnerText, Is.EqualTo("c2FmZQ=="));
            Assert.That(value.SelectSingleNode("NeedSavePassword").InnerText, Is.EqualTo("true"));
            Assert.That(((XmlElement)value.SelectSingleNode("Password")).IsEmpty, Is.True, "открытый пароль удалён");
            Assert.That(value.SelectSingleNode("Proxy/SafePassword").InnerText, Is.EqualTo("BBBB"), "пароль прокси не тронут");
        }

        [Test]
        public void Apply_without_connection_settings_throws()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<SerializableObjectInfo><Properties /></SerializableObjectInfo>");

            Assert.That(() => ClientLoginFile.Apply(doc, "s", "u", "p", "test"),
                Throws.TypeOf<InvalidDataException>().With.Message.Contains("ConnectionSettings"));
        }

        [Test]
        public void Apply_without_element_throws()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(Settings.Replace("<NeedSavePassword>false</NeedSavePassword>", ""));

            Assert.That(() => ClientLoginFile.Apply(doc, "s", "u", "p", "test"),
                Throws.TypeOf<InvalidDataException>().With.Message.Contains("NeedSavePassword"));
        }

        [TestCase(@"C:\Program Files\ASCON\Pilot-BIM")]
        [TestCase(@"C:\Program Files\ASCON\Pilot-ICE Enterprise")]
        public void Protect_uses_client_assembly(string clientDir)
        {
            if (!File.Exists(Path.Combine(clientDir, "Ascon.Pilot.Common.dll")))
            {
                Assert.Ignore("Клиент не установлен: " + clientDir);
            }

            byte[] protectedPassword = ClientLoginFile.Protect(clientDir, "probe");

            Assert.That(protectedPassword, Is.Not.Empty);
        }
    }
}
