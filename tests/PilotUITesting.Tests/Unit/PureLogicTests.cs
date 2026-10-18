using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ascon.Pilot.DataClasses;
using NUnit.Framework;
using PilotUITesting.Client;
using PilotUITesting.Data;
using PilotUITesting.NUnit;

namespace PilotUITesting.Tests.Unit
{
    [TestFixture]
    public class MessageBoxAnswerTests
    {
        [Test]
        public void Yes_no_cancel_view_uses_6_7_2()
        {
            string[] ids = { "6", "7", "2" };
            Assert.That(PilotMessageBox.Resolve(MessageBoxAnswer.Yes, ids), Is.EqualTo("6"));
            Assert.That(PilotMessageBox.Resolve(MessageBoxAnswer.No, ids), Is.EqualTo("7"));
            Assert.That(PilotMessageBox.Resolve(MessageBoxAnswer.Cancel, ids), Is.EqualTo("2"));
        }

        [Test]
        public void Delete_confirmation_has_no_7()
        {
            string[] ids = { "6", "2" };
            Assert.That(PilotMessageBox.Resolve(MessageBoxAnswer.Yes, ids), Is.EqualTo("6"));
            Assert.That(PilotMessageBox.Resolve(MessageBoxAnswer.No, ids), Is.Null, "«Нет» нет - не подменяем другой кнопкой");
            Assert.That(PilotMessageBox.Resolve(MessageBoxAnswer.Cancel, ids), Is.EqualTo("2"));
        }

        [Test]
        public void Info_window_ok_is_btnCancel()
        {
            Assert.That(PilotMessageBox.Resolve(MessageBoxAnswer.Ok, new[] { "btnCancel" }), Is.EqualTo("btnCancel"));
        }

        [Test]
        public void Action_confirmation_ok_cancel_means_yes_no()
        {
            string[] ids = { "btnOk", "btnCancel" };
            Assert.That(PilotMessageBox.Resolve(MessageBoxAnswer.Yes, ids), Is.EqualTo("btnOk"));
            Assert.That(PilotMessageBox.Resolve(MessageBoxAnswer.No, ids), Is.EqualTo("btnCancel"));
        }
    }

    [TestFixture]
    public class ServerPureTests
    {
        [Test]
        public void Stub_with_zero_type_does_not_exist()
        {
            Assert.That(PilotServerClient.Exists(new DObject { Id = Guid.NewGuid(), TypeId = 0 }), Is.False);
        }

        [Test]
        public void Database_root_has_zero_type_but_exists()
        {
            Assert.That(PilotServerClient.Exists(new DObject { Id = PilotServerClient.RootId, TypeId = 0 }), Is.True);
        }

        [Test]
        public void Permanently_deleted_does_not_exist()
        {
            DObject obj = new DObject { Id = Guid.NewGuid(), TypeId = 5 };
            Assert.That(PilotServerClient.Exists(obj), Is.True);
            obj.StateInfo.State = ObjectState.DeletedPermanently;
            Assert.That(PilotServerClient.Exists(obj), Is.False);
            Assert.That(PilotServerClient.Exists(null), Is.False);
        }

        [Test]
        public void Dates_are_compared_as_sent_to_server_in_utc()
        {
            DateTime local = new DateTime(2026, 9, 26, 10, 0, 0, DateTimeKind.Local);
            DateTime unspecified = new DateTime(2026, 9, 26, 10, 0, 0, DateTimeKind.Unspecified);
            DateTime utc = new DateTime(2026, 9, 26, 7, 0, 0, DateTimeKind.Utc);
            Assert.That(PilotServerClient.AsSent(local).DateValue, Is.EqualTo(local.ToUniversalTime()));
            Assert.That(PilotServerClient.AsSent(local).DateValue.Value.Kind, Is.EqualTo(DateTimeKind.Utc));
            Assert.That(PilotServerClient.AsSent(unspecified).DateValue, Is.EqualTo(unspecified.ToUniversalTime()));
            Assert.That(PilotServerClient.AsSent(utc).DateValue, Is.EqualTo(utc));
            Assert.That(PilotServerClient.AsSent(DateTime.MaxValue).DateValue, Is.EqualTo(DateTime.MaxValue));
            Assert.That(PilotServerClient.AsSent("текст").StrValue, Is.EqualTo("текст"));
        }

        [Test]
        public void AttrOrNull_distinguishes_missing_from_empty()
        {
            DObject obj = new DObject();
            obj.Attributes["name"] = "";
            Assert.That(PilotServerClient.AttrOrNull(obj, "name"), Is.EqualTo(""));
            Assert.That(PilotServerClient.AttrOrNull(obj, "missing"), Is.Null);
            Assert.That(PilotServerClient.Attr(obj, "missing"), Is.EqualTo(""), "прежний Attr по-прежнему возвращает пустую строку");
        }

        [Test]
        public void Strict_value_conversion_rejects_unknown_types()
        {
            Assert.That(PilotServerClient.ToDValueStrict(5).Value, Is.EqualTo(5L));
            Assert.Throws<ArgumentException>(() => PilotServerClient.ToDValueStrict(new object()));
        }

        [Test]
        public void Sandbox_config_validation()
        {
            Assert.That(new PilotSandboxConfig { RootType = "support", RootTitleAttribute = "support_name" }.Validate(), Is.Empty);
            Assert.That(new PilotSandboxConfig().Validate(), Is.Not.Empty);
            Assert.That(new PilotSandboxConfig { RootType = "a", RootTitleAttribute = "b", DocumentType = "c" }.Validate(), Has.Some.Contains("documentType"));
        }
    }

    [TestFixture]
    [NonParallelizable]
    public class FileAndLogTests
    {
        private string _dir;

        [SetUp]
        public void TempDir()
        {
            _dir = Path.Combine(Path.GetTempPath(), "PilotUITesting.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dir);
        }

        [TearDown]
        public void Cleanup()
        {
            Directory.Delete(_dir, true);
        }

        [Test]
        public void Prune_removes_only_old_artifact_folders()
        {
            string[] artifacts = { "20260101_000000_000_A.x", "20260102_000000_000_B.y", "20260103_000000_000_C.z" };
            string[] foreign = { "20260926.1", "output" };
            foreach (string name in artifacts.Concat(foreign))
            {
                Directory.CreateDirectory(Path.Combine(_dir, name));
            }
            TestArtifacts.Prune(new PilotUiConfig { ArtifactsDir = _dir, ArtifactsKeep = 1 });
            Assert.That(Directory.GetDirectories(_dir).Select(Path.GetFileName), Is.EquivalentTo(new[] { "20260103_000000_000_C.z" }.Concat(foreign)));
        }

        [Test]
        public void Backup_restores_content_and_absence()
        {
            string existing = Path.Combine(_dir, "a.json");
            string absent = Path.Combine(_dir, "b.json");
            File.WriteAllText(existing, "исходное");
            LocalFileBackup backup = LocalFileBackup.Take(existing, absent);
            File.WriteAllText(existing, "изменено");
            File.WriteAllText(absent, "появился");
            backup.Restore();
            Assert.That(File.ReadAllText(existing), Is.EqualTo("исходное"));
            Assert.That(File.Exists(absent), Is.False);
        }

        [Test]
        public void Plugin_log_returns_only_complete_new_lines_with_level()
        {
            string log = Path.Combine(_dir, "Module.log");
            File.WriteAllText(log, "2026-09-24 00:10:00,000|[1]|INFO |[Menu]|Plugin version: 1.155.0.0\n", Encoding.UTF8);
            PluginLogWatch watch = new PluginLogWatch(_dir, new[] { "Module" });
            watch.Mark();
            File.AppendAllText(log, "2026-09-24 00:11:00,000|[7]|ERROR|[Grid]|сбой загрузки\nпродолжение сообщения\n2026-09-24 00:11:01,000|[7]|WARN |[Grid]|недописано", Encoding.UTF8);
            IReadOnlyList<PluginLogLine> lines = watch.NewLines();
            Assert.That(lines, Has.Count.EqualTo(2));
            Assert.That(lines[0].Level, Is.EqualTo("ERROR"));
            Assert.That(lines[0].Text, Does.Contain("сбой загрузки"));
            Assert.That(lines[1].Level, Is.EqualTo(""), "строка продолжения без формата log4net");
            Assert.That(watch.LastPluginVersion("Module"), Is.EqualTo("1.155.0.0"));
        }

        [Test]
        public void Pdf_is_written()
        {
            byte[] pdf = PilotTestFiles.MinimalPdf("UI test");
            Assert.That(Encoding.ASCII.GetString(pdf), Does.StartWith("%PDF-1.4").And.EndWith("%%EOF\n"));
            string path = PilotTestFiles.Pdf("UI test");
            PilotTestFiles.WaitWritten(path, TimeSpan.FromSeconds(1));
            Assert.That(new FileInfo(path).Length, Is.EqualTo(pdf.Length));
        }

        [Test]
        public void Artifact_names_are_safe()
        {
            Assert.That(TestArtifacts.Sanitize("A.B(\"x\"/y)", 100), Does.Not.Contain("\"").And.Not.Contain("/"));
            Assert.That(TestArtifacts.Sanitize(new string('я', 200), 100), Has.Length.EqualTo(100));
        }
    }
}
