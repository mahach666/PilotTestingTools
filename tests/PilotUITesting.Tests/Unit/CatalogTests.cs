using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using PilotUITesting.Client;

namespace PilotUITesting.Tests.Unit
{
    /// <summary>Каталог PilotIds: шаблоны составных id (примеры - реальные id из дампов и разведки), форма констант, совпадение с прежними константами.</summary>
    [TestFixture]
    public class CatalogTests
    {
        [TestCase("task_warningSign", "taskwarningSign")]
        [TestCase("support_document", "supportdocument")]
        [TestCase("Процесс подписания", "Процессподписания")]
        public void XamlName_keeps_only_letters_and_digits(string source, string expected)
        {
            Assert.That(PilotIds.XamlName(source), Is.EqualTo(expected));
        }

        [Test]
        public void Composite_ids_match_observed_client()
        {
            Assert.That(PilotIds.Explorer.CreateItem("support_document"), Is.EqualTo("miCreatesupportdocument"));
            Assert.That(PilotIds.Explorer.AttachToItem("task_warningSign"), Is.EqualTo("miAttachTotaskwarningSign"));
            Assert.That(PilotIds.Explorer.AttachToItem("task_simple"), Is.EqualTo("miAttachTotasksimple"));
            Assert.That(PilotIds.ToolbarStripe.MenuOf("miCreate"), Is.EqualTo("miCreateContextMenu"));
            Assert.That(PilotIds.ToolbarStripe.MenuOf("Create"), Is.EqualTo("CreateContextMenu"));
            Assert.That(PilotIds.Card.Field("content"), Is.EqualTo("AUIDcontent"));
            Assert.That(PilotIds.Card.Caption("name"), Is.EqualTo("attributeCaptionContainer:name"));
            Assert.That(PilotIds.Card.Caption(""), Is.EqualTo(PilotIds.Card.CaptionPrefix));
            Assert.That(PilotIds.Card.OrgUnitInput("Исполнитель"), Is.EqualTo("AUIDNewOrgUnitAutocomplete;Stage:Исполнитель"));
            Assert.That(PilotIds.WorkflowEditor.StageItem("stage_signing", 0), Is.EqualTo("AUIDStageItem:stage_signing;Index:0"));
            Assert.That(PilotIds.Viewer.Toolbar.RemarksMenuButton("doc_issue"), Is.EqualTo("tbiRemarksMenuButton_doc_issue"));
            Assert.That(PilotIds.Viewer.Annotations.AnnotationStateTransition("status_issue", "assigned"), Is.EqualTo("mistatus_issue_assigned"));
            Assert.That(PilotIds.Tasks.CreateSubTaskToolbarItem("task_simple"), Is.EqualTo("tbiCreateSubTasktask_simple"), "в клиенте без очистки имени");
        }

        [Test]
        public void Legacy_constants_agree_with_catalog()
        {
            Assert.That(PilotApp.MainWindowId, Is.EqualTo(PilotIds.MainWindow.Container));
            Assert.That(PilotApp.TabHostId, Is.EqualTo(PilotIds.MainWindow.TabHost));
            Assert.That(PilotApp.AddTabButtonId, Is.EqualTo(PilotIds.Tabs.Add));
            Assert.That(PilotApp.TabItemId, Is.EqualTo(PilotIds.Tabs.Item));
            Assert.That(PilotApp.TabTitleId, Is.EqualTo(PilotIds.Tabs.Title));
            Assert.That(PilotApp.TabCloseButtonId, Is.EqualTo(PilotIds.Tabs.Close));
            Assert.That(PilotApp.UserProfileButtonId, Is.EqualTo(PilotIds.MainWindow.UserProfile));
            Assert.That(PilotObjectDialog.CreateWindowId, Is.EqualTo(PilotIds.Card.Container));
            Assert.That(PilotObjectDialog.CreateButtonId, Is.EqualTo(PilotIds.Card.Create));
            Assert.That(PilotObjectDialog.SaveButtonId, Is.EqualTo(PilotIds.Card.Save));
            Assert.That(PilotObjectDialog.CancelButtonId, Is.EqualTo(PilotIds.Card.Cancel));
            Assert.That(PilotObjectDialog.OpenFileButtonId, Is.EqualTo(PilotIds.Card.OpenFile));
            Assert.That(PilotObjectDialog.FileDialogClass, Is.EqualTo(PilotIds.WindowsFileDialog.WindowClass));
            Assert.That(PilotObjectDialog.FileDialogNameEditId, Is.EqualTo(PilotIds.WindowsFileDialog.FileName));
            Assert.That(PilotObjectDialog.FileDialogOpenButtonId, Is.EqualTo(PilotIds.WindowsFileDialog.Ok));
            Assert.That(PilotObjectDialog.FileDialogCancelButtonId, Is.EqualTo(PilotIds.WindowsFileDialog.Cancel));
            Assert.That(PilotObjectDialog.ReferenceBookSearchId, Is.EqualTo(PilotIds.Card.ReferenceBook.Search));
            Assert.That(PilotObjectDialog.ReferenceBookTreeId, Is.EqualTo(PilotIds.Card.ReferenceBook.Tree));
            Assert.That(PilotObjectDialog.ReferenceBookOkId, Is.EqualTo(PilotIds.Card.ReferenceBook.Ok));
            Assert.That(PilotObjectDialog.ReferenceBookItemCheckBoxId, Is.EqualTo(PilotIds.Card.ReferenceBook.CheckBox));
            Assert.That(PilotObjectDialog.ReferenceBookItemTitleId, Is.EqualTo(PilotIds.Card.ReferenceBook.ItemTitle));
            Assert.That(PilotObjectDialog.FieldPickerButtonId, Is.EqualTo(PilotIds.Card.FieldRightButton));
            Assert.That(PilotObjectDialog.ReferenceBookExpanderId, Is.EqualTo(PilotIds.Card.ReferenceBook.Expander));
        }

        [Test]
        public void Every_constant_is_a_clean_non_empty_id_unique_in_its_class()
        {
            List<string> problems = new List<string>();
            foreach (Type type in Sections(typeof(PilotIds)).Concat(Sections(typeof(PilotTitles))))
            {
                List<FieldInfo> constants = type.GetFields(BindingFlags.Public | BindingFlags.Static).Where(f => f.IsLiteral && f.FieldType == typeof(string)).ToList();
                foreach (FieldInfo field in constants)
                {
                    string value = (string)field.GetRawConstantValue();
                    if (string.IsNullOrEmpty(value) || value.Trim() != value)
                    {
                        problems.Add($"{type.FullName}.{field.Name} = «{value}»");
                    }
                }
                if (type.DeclaringType == null || !type.FullName.StartsWith(typeof(PilotIds).FullName, StringComparison.Ordinal))
                {
                    continue;
                }
                foreach (IGrouping<string, FieldInfo> duplicate in constants.GroupBy(f => (string)f.GetRawConstantValue()).Where(g => g.Count() > 1))
                {
                    problems.Add($"{type.FullName}: значение «{duplicate.Key}» у {string.Join(", ", duplicate.Select(f => f.Name))}");
                }
            }
            Assert.That(problems, Is.Empty, string.Join(Environment.NewLine, problems));
        }

        [Test]
        public void Command_sections_have_container()
        {
            List<string> without = new List<string>();
            foreach (Type type in Sections(typeof(PilotIds)))
            {
                string[] values = type.GetFields(BindingFlags.Public | BindingFlags.Static).Where(f => f.IsLiteral && f.FieldType == typeof(string)).Select(f => (string)f.GetRawConstantValue()).ToArray();
                bool commands = values.Any(v => v.StartsWith("mi", StringComparison.Ordinal) || v.StartsWith("mni", StringComparison.Ordinal) || v.StartsWith("tbi", StringComparison.Ordinal));
                if (commands && type.GetField("Container") == null)
                {
                    without.Add(type.FullName);
                }
            }
            Assert.That(without, Is.Empty, "у панели или меню должна быть константа Container: " + string.Join(", ", without));
        }

        [Test]
        public void Catalog_is_not_empty()
        {
            int count = Sections(typeof(PilotIds)).Sum(t => t.GetFields(BindingFlags.Public | BindingFlags.Static).Count(f => f.IsLiteral));
            Assert.That(count, Is.GreaterThan(500));
        }

        private static IEnumerable<Type> Sections(Type root)
        {
            yield return root;
            foreach (Type nested in root.GetNestedTypes(BindingFlags.Public))
            {
                foreach (Type type in Sections(nested))
                {
                    yield return type;
                }
            }
        }
    }
}
