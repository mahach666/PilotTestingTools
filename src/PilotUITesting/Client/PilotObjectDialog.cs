using System;
using System.IO;
using System.Linq;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using PilotUITesting.Uia;

namespace PilotUITesting.Client
{
    /// <summary>
    /// Стандартная карточка объекта Pilot («Создание нового элемента» / редактирование):
    /// поля атрибутов имеют AutomationId «AUID&lt;имя атрибута&gt;», кнопки btnCreate / btnSave / btnCancel,
    /// панель файлов - «Выбрать файл» (tbiOpenFile) со стандартным диалогом открытия файла Windows.
    /// </summary>
    public sealed partial class PilotObjectDialog
    {
        public const string CreateWindowId = "AUIDCreateObject";
        public const string CreateWindowTitle = "Создание нового элемента";
        public const string AttributePrefix = "AUID";
        public const string CreateButtonId = "btnCreate";
        public const string SaveButtonId = "btnSave";
        public const string CancelButtonId = "btnCancel";
        public const string OpenFileButtonId = "tbiOpenFile";
        public const string FileDialogClass = "#32770";
        public const string FileDialogNameEditId = "1148";
        public const string FileDialogOpenButtonId = "1";
        public const string FileDialogCancelButtonId = "2";

        public const string EditWindowTitle = "Редактирование элемента";

        private readonly PilotApp _app;
        private readonly Window _window;
        private readonly TimeSpan _timeout;
        private readonly string _windowId;
        private readonly string _title;

        public Window Window => _window;

        public string Title => _title;

        private PilotObjectDialog(PilotApp app, Window window, TimeSpan timeout)
        {
            _app = app;
            _window = window;
            _timeout = timeout;
            _windowId = window.SafeAutomationId();
            _title = window.SafeName();
        }

        /// <summary>Дождаться карточки создания объекта.</summary>
        public static PilotObjectDialog WaitCreate(PilotApp app, TimeSpan timeout)
        {
            Window window = Wait.For(
                () => app.TopLevelWindows().FirstOrDefault(w => w.SafeAutomationId() == CreateWindowId || w.SafeName() == CreateWindowTitle)?.AsWindow(),
                timeout, "карточка «Создание нового элемента»");
            return new PilotObjectDialog(app, window, timeout);
        }

        /// <summary>Дождаться любой карточки объекта Pilot (создание или редактирование): окно с полем атрибута.</summary>
        public static PilotObjectDialog WaitAny(PilotApp app, TimeSpan timeout, string attributeName = "name")
        {
            Window window = Wait.For(
                () => app.TopLevelWindows()
                    .Where(w => w.SafeAutomationId() != PilotApp.MainWindowId)
                    .FirstOrDefault(w => w.SafeAutomationId() == CreateWindowId || w.TryById(AttributePrefix + attributeName) != null)?.AsWindow(),
                timeout, "карточка объекта Pilot");
            return new PilotObjectDialog(app, window, timeout);
        }

        /// <summary>
        /// Дождаться карточки редактирования объекта: у неё тот же AutomationId «AUIDCreateObject», что и у карточки создания,
        /// отличаются заголовок «Редактирование элемента» и кнопка btnSave вместо btnCreate.
        /// </summary>
        public static PilotObjectDialog WaitEdit(PilotApp app, TimeSpan timeout, string attributeName = "name")
        {
            Window window = Wait.For(
                () => app.TopLevelWindows()
                    .Where(w => w.SafeAutomationId() != PilotApp.MainWindowId && w.SafeName() != CreateWindowTitle)
                    .FirstOrDefault(w => w.SafeName() == EditWindowTitle || w.TryById(SaveButtonId) != null || w.TryById(AttributePrefix + attributeName) != null)?.AsWindow(),
                timeout, "карточка редактирования объекта Pilot");
            return new PilotObjectDialog(app, window, timeout);
        }

        /// <summary>Тип создаваемого объекта, как показан в заголовке карточки (первый текст под шапкой).</summary>
        public string ObjectTypeTitle => _window.SafeChildren().Where(c => c.SafeControlType() == ControlType.Text).Select(t => t.SafeName()).FirstOrDefault(s => !string.IsNullOrWhiteSpace(s)) ?? "";

        public AutomationElement Field(string attributeName)
        {
            return _window.ById(AttributePrefix + attributeName, _timeout);
        }

        public bool HasField(string attributeName)
        {
            return _window.TryById(AttributePrefix + attributeName) != null;
        }

        public string FieldValue(string attributeName)
        {
            AutomationElement field = Field(attributeName);
            return field.Patterns.Value.IsSupported ? field.Patterns.Value.Pattern.Value.Value : string.Join(" ", field.Texts());
        }

        /// <summary>
        /// Заполнить атрибут: текстовое поле - через ValuePattern; поле со списком значений (ComboBox) - через вложенное поле ввода,
        /// а если ввода нет - раскрыть список и выбрать пункт с таким текстом.
        /// </summary>
        public PilotObjectDialog SetField(string attributeName, string value)
        {
            AutomationElement field = Field(attributeName);
            if (field.Patterns.Value.IsSupported && field.SafeControlType() != ControlType.ComboBox)
            {
                field.SetText(value);
                return this;
            }
            AutomationElement input = field.FindDescendantBfs(e => e.SafeControlType() == ControlType.Edit, 4);
            if (input != null)
            {
                input.SetText(value);
                return this;
            }
            if (field.Patterns.ExpandCollapse.IsSupported)
            {
                Wait.Retry(() => field.Patterns.ExpandCollapse.Pattern.Expand());
                AutomationElement item = Wait.For(
                    () => field.FindDescendantBfs(e => e.SafeControlType() == ControlType.ListItem && e.SafeName() == value, 6)
                          ?? _app.TopLevelWindows().Select(w => w.FindDescendantBfs(e => e.SafeControlType() == ControlType.ListItem && e.SafeName() == value, 8)).FirstOrDefault(e => e != null),
                    _timeout, $"значение '{value}' в списке поля '{attributeName}'");
                item.SelectItem();
                return this;
            }
            field.SetText(value);
            return this;
        }

        public const string ReferenceBookWindowId = "Справочник";
        public const string ReferenceBookSearchId = "searchEdit";
        public const string ReferenceBookTreeId = "ascTreeView";
        public const string ReferenceBookOkId = "btnOk";
        public const string ReferenceBookItemCheckBoxId = "checkBox";
        public const string ReferenceBookItemTitleId = "titleTextBlock";
        public const string FieldPickerButtonId = "PART_Right_Button";

        /// <summary>Окно выбора из справочника (дочернее окно карточки с AutomationId «Справочник»).</summary>
        public AutomationElement FindReferenceBookDialog()
        {
            return _window.FindDescendantBfs(e => e.SafeAutomationId() == ReferenceBookWindowId && e.SafeControlType() == ControlType.Window, 2)
                ?? _app.TopLevelWindows().FirstOrDefault(w => w.SafeAutomationId() == ReferenceBookWindowId);
        }

        /// <summary>
        /// Заполнить атрибут-ссылку на справочник: нажать «…» у поля, найти элемент по тексту через поиск, выбрать и нажать OK.
        /// </summary>
        public PilotObjectDialog PickReference(string attributeName, string itemText)
        {
            Field(attributeName).ById(FieldPickerButtonId, _timeout).Press();
            AutomationElement book = Wait.For(FindReferenceBookDialog, _timeout, "окно «Справочник»");
            AutomationElement search = book.ById(ReferenceBookSearchId, _timeout);
            search.SetText(itemText);
            AutomationElement tree = book.ById(ReferenceBookTreeId, _timeout);
            AutomationElement row = Wait.For(
                () => tree.Rows().FirstOrDefault(r => r.SafeName() != "Результаты поиска" && (r.SafeName() + " " + string.Join(" ", r.Texts())).IndexOf(itemText, StringComparison.OrdinalIgnoreCase) >= 0),
                _timeout, $"элемент справочника '{itemText}'");
            ConfirmReferenceRow(book, row);
            return this;
        }

        public const string ReferenceBookExpanderId = "expander";

        /// <summary>
        /// Выбрать элемент справочника, раскрывая дерево по пути заголовков строк (сравнение по подстроке, последний сегмент - выбираемый элемент).
        /// Нужен, когда строка поиска объект не находит: поисковый индекс сервера отстаёт для объектов, созданных минуты назад.
        /// </summary>
        public PilotObjectDialog PickReferenceByPath(string attributeName, params string[] path)
        {
            if (path == null || path.Length == 0)
            {
                throw new ArgumentException("Путь по дереву справочника пуст", nameof(path));
            }
            Field(attributeName).ById(FieldPickerButtonId, _timeout).Press();
            AutomationElement book = Wait.For(FindReferenceBookDialog, _timeout, "окно «Справочник»");
            AutomationElement tree = book.ById(ReferenceBookTreeId, _timeout);
            AutomationElement row = null;
            for (int i = 0; i < path.Length; i++)
            {
                string part = path[i];
                row = Wait.For(() => tree.FindRowScrolling(r => r.SafeName().IndexOf(part, StringComparison.OrdinalIgnoreCase) >= 0), _timeout, $"строка справочника '{part}'");
                if (i == path.Length - 1)
                {
                    break;
                }
                string next = path[i + 1];
                Func<bool> nextVisible = () => tree.FindRowScrolling(r => r.SafeName().IndexOf(next, StringComparison.OrdinalIgnoreCase) >= 0) != null;
                if (nextVisible())
                {
                    continue;
                }
                // узел раскрывается кнопкой expander; InvokePattern дерево Pilot не всегда обрабатывает - клик мышью, затем двойной клик по строке
                AutomationElement expander = Wait.For(() => row.TryById(ReferenceBookExpanderId), _timeout, $"кнопка раскрытия узла '{part}'");
                Wait.Retry(() => expander.Click());
                if (!Wait.TryUntil(nextVisible, TimeSpan.FromSeconds(10)))
                {
                    AutomationElement title = row.TryById(ReferenceBookItemTitleId) ?? row;
                    Wait.Retry(() => title.DoubleClick());
                    Wait.TryUntil(nextVisible, TimeSpan.FromSeconds(10));
                }
            }
            ConfirmReferenceRow(book, row);
            return this;
        }

        /// <summary>Отметить строку справочника и нажать OK.</summary>
        private void ConfirmReferenceRow(AutomationElement book, AutomationElement row)
        {
            // элемент справочника отмечается флажком в строке (checkBox). Флажок нажимаем мышью: программный Toggle
            // через UI Automation не создаёт ввода, и WPF не пересчитывает доступность кнопки OK (CanExecute)
            // справочники без флажков (одиночный выбор) - строка выделяется кликом по её заголовку
            AutomationElement checkBox = row.TryById(ReferenceBookItemCheckBoxId);
            AutomationElement titleText = row.TryById(ReferenceBookItemTitleId) ?? row;
            if (checkBox != null && UiQuery.Read(() => checkBox.Patterns.Toggle.PatternOrDefault?.ToggleState.Value, _timeout, "флажок строки справочника") == ToggleState.On)
            {
                // строка уже отмечена (справочник элементов открывается с текущим значением атрибута): клик снял бы отметку и очистил атрибут.
                // Выбор не изменился - OK недоступна, окно закрывается «Отменой»
                AutomationElement okNow = book.ById(ReferenceBookOkId, _timeout);
                bool okEnabled = UiQuery.Read(() => okNow.Properties.IsEnabled.Value, _timeout, "доступность OK справочника");
                PilotLog.Action("строка справочника уже отмечена - " + (okEnabled ? "OK" : "«Отмена»: выбор не изменился"));
                (okEnabled ? okNow : book.ById(PilotIds.Card.ReferenceBook.Cancel, _timeout)).Press();
                Wait.Until(() => FindReferenceBookDialog() == null, _timeout, "закрытие окна «Справочник»");
                return;
            }
            Wait.Retry(() => (checkBox ?? titleText).Click());
            AutomationElement ok = book.ById(ReferenceBookOkId, _timeout);
            if (!Wait.TryUntil(() => ok.SafeIsEnabled(), TimeSpan.FromSeconds(5)))
            {
                Wait.Retry(() => titleText.Click());
                if (row.Patterns.SelectionItem.IsSupported)
                {
                    Wait.Retry(() => row.Patterns.SelectionItem.Pattern.Select());
                }
            }
            Wait.Until(() => ok.SafeIsEnabled(), _timeout, "кнопка OK справочника доступна");
            ok.Press();
            Wait.Until(() => FindReferenceBookDialog() == null, _timeout, "закрытие окна «Справочник»");
        }

        /// <summary>Приложить файл через «Выбрать файл» и стандартный диалог Windows.</summary>
        public PilotObjectDialog AttachFile(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Файл для вложения не найден", path);
            }
            _window.ById(OpenFileButtonId, _timeout).Press();
            // системный диалог открытия файла (#32770) появляется как дочернее окно карточки, а не как окно верхнего уровня
            AutomationElement openDialog = Wait.For(FindFileDialog, _timeout, "диалог открытия файла Windows");
            // «Имя файла:» - ComboBox (id 1148) с вложенным Edit того же id; значение задаём через ValuePattern поля ввода
            AutomationElement fileName = Wait.For(
                () => openDialog.FindDescendantBfs(e => e.SafeAutomationId() == FileDialogNameEditId && e.SafeControlType() == ControlType.Edit)
                      ?? openDialog.TryById(FileDialogNameEditId),
                _timeout, "поле «Имя файла»");
            string fullPath = Path.GetFullPath(path);
            fileName.SetText(fullPath);
            Wait.Until(() => FieldText(fileName) == fullPath, TimeSpan.FromSeconds(5), "имя файла введено в диалог");
            AutomationElement open = openDialog.FindDescendantBfs(e => e.SafeAutomationId() == FileDialogOpenButtonId && e.SafeControlType() != ControlType.ListItem)
                ?? openDialog.ByName("Открыть", _timeout);
            open.Press();
            Wait.Until(() => FindFileDialog() == null, _timeout, "закрытие диалога открытия файла");
            return this;
        }

        /// <summary>Системный диалог открытия файла: дочернее окно карточки либо окно верхнего уровня процесса.</summary>
        public AutomationElement FindFileDialog()
        {
            return _window.FindDescendantBfs(e => e.SafeClassName() == FileDialogClass, 2)
                ?? _app.TopLevelWindows().FirstOrDefault(w => w.SafeClassName() == FileDialogClass);
        }

        private static string FieldText(AutomationElement field)
        {
            try
            {
                return field.Patterns.Value.IsSupported ? field.Patterns.Value.Pattern.Value.Value : "";
            }
            catch
            {
                return "";
            }
        }

        /// <summary>Кнопка подтверждения карточки: btnCreate / btnSave, иначе кнопка с подписью «Сохранить» или «Создать».</summary>
        public AutomationElement SubmitButton =>
            _window.TryById(CreateButtonId)
            ?? _window.TryById(SaveButtonId)
            ?? _window.TryByControlTypeAndName(ControlType.Button, "Сохранить")
            ?? _window.TryByControlTypeAndName(ControlType.Button, "Создать");

        public bool CanSubmit => SubmitButton.SafeIsEnabled();

        /// <summary>Нажать «Создать» (или «Сохранить» у карточки редактирования) и дождаться закрытия окна.</summary>
        public void Submit()
        {
            AutomationElement button = Wait.For(() => SubmitButton, _timeout, "кнопка «Создать»/«Сохранить»");
            try
            {
                Wait.Until(() => button.SafeIsEnabled(), _timeout, "кнопка подтверждения карточки доступна (заполнены обязательные атрибуты)");
            }
            catch (TimeoutException ex)
            {
                throw new PilotWaitTimeoutException("кнопка подтверждения карточки доступна (заполнены обязательные атрибуты)", _timeout, _timeout,
                    "обязательные атрибуты: " + string.Join(", ", Card.ObligatoryAttributes()) + "; значения: " + Card.Describe(), ex);
            }
            button.Press();
            WaitClosed();
        }

        public void Cancel()
        {
            AutomationElement cancel = Wait.For(() => _window.TryById(CancelButtonId) ?? _window.TryByControlTypeAndName(ControlType.Button, "Отмена"), _timeout, "кнопка «Отмена» карточки");
            cancel.Press();
            WaitClosed();
        }

        /// <summary>Окно карточки закрыто: элемент окна недоступен и среди окон процесса нет окна с теми же AutomationId и заголовком.</summary>
        public bool IsClosed => !_window.SafeIsAvailable() || !_app.TopLevelWindows().Any(w => w.SafeAutomationId() == _windowId && w.SafeName() == _title);

        public void WaitClosed()
        {
            Wait.Until(() => IsClosed, _timeout, "закрытие карточки объекта");
        }
    }
}
