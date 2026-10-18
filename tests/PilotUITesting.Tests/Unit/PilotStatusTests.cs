using System;
using NUnit.Framework;
using PilotUITesting.Client;

namespace PilotUITesting.Tests.Unit
{
    /// <summary>Разбор HelpText/ItemStatus: примеры сняты с живого клиента Pilot-ICE 26.11 (разведка 2026-09-24).</summary>
    [TestFixture]
    public class PilotStatusTests
    {
        [Test]
        public void Toolbar_button_status_without_toggle()
        {
            (bool hidden, bool? isChecked) = PilotStatus.ToolbarItem("IsHidden:False");
            Assert.That(hidden, Is.False);
            Assert.That(isChecked, Is.Null);
        }

        [Test]
        public void Toolbar_toggle_status_with_trailing_semicolon()
        {
            (bool hidden, bool? isChecked) = PilotStatus.ToolbarItem("IsHidden:False; IsChecked:True;");
            Assert.That(hidden, Is.False);
            Assert.That(isChecked, Is.True);
        }

        [Test]
        public void Hidden_button_in_overflow()
        {
            Assert.That(PilotStatus.ToolbarItem("IsHidden:True").IsHidden, Is.True);
        }

        [Test]
        public void Tab_header_help_text()
        {
            (bool active, string content) = PilotStatus.TabHeader("IsActive:True; Content:AUIDDocumentsExplorer");
            Assert.That(active, Is.True);
            Assert.That(content, Is.EqualTo("AUIDDocumentsExplorer"));
        }

        [Test]
        public void Tab_header_content_may_contain_colons()
        {
            (bool active, string content) = PilotStatus.TabHeader("IsActive:False; Content:РЗ: документ");
            Assert.That(active, Is.False);
            Assert.That(content, Is.EqualTo("РЗ: документ"));
        }

        [Test]
        public void Window_help_text_with_label()
        {
            (bool active, bool visible, string state) = PilotStatus.Window("AUIDPilotWindow;IsActive:True;IsVisible:True;State:Maximized");
            Assert.That(active, Is.True);
            Assert.That(visible, Is.True);
            Assert.That(state, Is.EqualTo("Maximized"));
            Assert.That(PilotStatus.Parse("AUIDPilotWindow;IsActive:True").Labels, Is.EquivalentTo(new[] { "AUIDPilotWindow" }));
        }

        [Test]
        public void Connection_state()
        {
            Assert.That(PilotStatus.Connection("ConnectionState:Online"), Is.EqualTo(ClientConnection.Online));
            Assert.That(PilotStatus.Connection("ConnectionState:Offline"), Is.EqualTo(ClientConnection.Offline));
        }

        [Test]
        public void Unknown_connection_state_is_format_error_with_source_text()
        {
            FormatException ex = Assert.Throws<FormatException>(() => PilotStatus.Connection("ConnectionState:Sleeping"));
            Assert.That(ex.Message, Does.Contain("ConnectionState:Sleeping"));
        }

        [Test]
        public void Loading_tabs_flag()
        {
            Assert.That(PilotStatus.IsLoadingTabs("IsLoadingTabs:False"), Is.False);
            Assert.That(PilotStatus.IsLoadingTabs("IsLoadingTabs:True"), Is.True);
        }

        [Test]
        public void Missing_key_is_format_error()
        {
            Assert.Throws<FormatException>(() => PilotStatus.IsLoadingTabs(""));
            Assert.Throws<FormatException>(() => PilotStatus.ToolbarItem("IsChecked:True"));
        }

        [Test]
        public void Bool_must_be_exact()
        {
            Assert.Throws<FormatException>(() => PilotStatus.ToolbarItem("IsHidden:false"));
        }

        [Test]
        public void Duplicate_key_is_format_error()
        {
            Assert.Throws<FormatException>(() => PilotStatus.Parse("IsHidden:False; IsHidden:True"));
        }

        [TestCase("Совместный _доступ...", "Совместный доступ...")]
        [TestCase("СЕ_РВИС", "СЕРВИС")]
        [TestCase("a__b", "a_b")]
        [TestCase("_Создать", "Создать")]
        [TestCase("И_мпортировать _проект", "Импортировать _проект")]
        [TestCase("", "")]
        public void Access_key_is_stripped_like_wpf(string caption, string expected)
        {
            Assert.That(PilotStatus.StripAccessKey(caption), Is.EqualTo(expected));
        }
    }
}
