using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Patterns;
using FlaUI.Core.WindowsAPI;
using PilotUITesting.Uia;
using Wait = PilotUITesting.Uia.Wait;

namespace PilotUITesting.Client
{
    /// <summary>Как выбрать подсказку автодополнения оргединицы.</summary>
    public enum OrgUnitPick
    {
        /// <summary>Клик мышью по подсказке (проверено на Pilot-ICE; выбор через SelectionItem клиент не принимает).</summary>
        MouseClick,

        /// <summary>Стрелка вниз и Enter (проверено на Pilot-BIM); подсказка должна быть единственной.</summary>
        DownEnter,
    }

    /// <summary>
    /// Карточка атрибутов Pilot в любой области: окно «Создание нового элемента» (AUIDCreateObject), карточка ObjectCardView проводника и
    /// диалогов модулей, карточки задания (ascTaskCard), процесса (workflowCard) и задания этапа (executorsCard).
    /// Поле атрибута - AUID&lt;имя атрибута&gt;, подпись - attributeCaptionContainer:&lt;имя атрибута&gt; (имя, а не заголовок).
    /// Ввод - только через UI Automation с проверкой прочитанного значения; «набрать в то, что в фокусе» карточка не делает.
    /// </summary>
    public sealed class PilotCard
    {
        private readonly PilotApp _app;
        private readonly Func<AutomationElement> _scope;
        private readonly string _description;

        internal PilotCard(PilotApp app, Func<AutomationElement> scope, string description)
        {
            _app = app;
            _scope = scope;
            _description = description;
        }

        private AutomationElement Scope => _scope() ?? throw new ElementNotFoundException(_description, "карточка", TimeSpan.Zero, TimeSpan.Zero, null, null);

        /// <summary>Имена атрибутов карточки (по контейнерам подписей).</summary>
        public IReadOnlyList<string> AttributeNames()
        {
            return Scope.FindAll(By.IdPrefix(PilotIds.Card.CaptionPrefix))
                .Select(e => e.SafeAutomationId().Substring(PilotIds.Card.CaptionPrefix.Length)).ToList();
        }

        /// <summary>Обязательные атрибуты (в подписи есть знак obligatoryTxb). Это признак обязательности, а не «поле пустое»: знак виден и после заполнения.</summary>
        public IReadOnlyList<string> ObligatoryAttributes()
        {
            return Scope.FindAll(By.IdPrefix(PilotIds.Card.CaptionPrefix))
                .Where(c => UiQuery.TryFind(c, By.Id(PilotIds.Card.ObligatoryMark), Wait.DefaultTimeout, 3) is AutomationElement mark && !mark.SafeIsOffscreen())
                .Select(e => e.SafeAutomationId().Substring(PilotIds.Card.CaptionPrefix.Length)).ToList();
        }

        /// <summary>Заголовок атрибута, как его видит пользователь (textTxb в контейнере подписи).</summary>
        public string Caption(string attribute)
        {
            AutomationElement container = Scope.Find(By.Id(PilotIds.Card.Caption(attribute)), null, _description);
            return UiQuery.TryFind(container, By.Id(PilotIds.Card.CaptionTitle), Wait.DefaultTimeout, 3)?.SafeName() ?? "";
        }

        public bool Has(string attribute)
        {
            return UiQuery.TryFind(Scope, By.Id(PilotIds.Card.Field(attribute)), Wait.DefaultTimeout) != null;
        }

        /// <summary>Поле атрибута AUID&lt;имя&gt;.</summary>
        public AutomationElement Field(string attribute, TimeSpan? timeout = null)
        {
            return UiQuery.WaitFind(_scope, By.Id(PilotIds.Card.Field(attribute)), timeout ?? Wait.DefaultTimeout, _description);
        }

        /// <summary>Значение поля: ValuePattern поля или вложенного поля ввода, иначе тексты.</summary>
        public string Value(string attribute)
        {
            AutomationElement field = Field(attribute);
            AutomationElement input = ValueTarget(field);
            return input != null ? input.SafeValue() : string.Join(" ", field.Texts());
        }

        /// <summary>Записать текст в поле (ValuePattern поля или вложенного Edit) и проверить прочитанное значение.</summary>
        public PilotCard Set(string attribute, string value)
        {
            AutomationElement field = Field(attribute);
            AutomationElement input = ValueTarget(field)
                ?? throw new InvalidOperationException($"Поле {attribute} ({_description}) не принимает текст через UI Automation: нет ValuePattern ни у поля, ни у вложенного поля ввода ({field.Describe()}). Для списков - Choose, для справочников - PickReference.");
            PilotPage.SetValue(input, value, $"атрибут {attribute} ({_description})");
            return this;
        }

        /// <summary>Выбрать значение списка (ComboBox): раскрыть и выбрать пункт с точной подписью; если у поля есть Value - проверить его.</summary>
        public PilotCard Choose(string attribute, string itemCaption)
        {
            AutomationElement field = Field(attribute);
            AutomationElement combo = field.SafeControlType() == ControlType.ComboBox ? field : UiQuery.TryFind(field, By.Type(ControlType.ComboBox), Wait.DefaultTimeout, 4) ?? field;
            UiChoice.Choose(_app, combo, itemCaption, $"список {attribute} ({_description})");
            return this;
        }

        /// <summary>Дата: dateTimeAttrContainer:&lt;имя&gt; -> PART_TextBox -> «dd.MM.yyyy» (ru-RU) с проверкой, затем Tab для фиксации значения.</summary>
        public PilotCard SetDate(string attribute, DateTime value)
        {
            AutomationElement container = UiQuery.WaitFind(_scope, By.Id(PilotIds.Card.DateContainer(attribute)), Wait.DefaultTimeout, _description);
            AutomationElement text = container.Find(By.Id(PilotIds.Card.DateTextBox), null, "дата " + attribute);
            string formatted = value.ToString("dd.MM.yyyy", CultureInfo.GetCultureInfo("ru-RU"));
            PilotPage.SetValue(text, formatted, $"дата {attribute} ({_description})");
            UiAct.Keys(text, "Tab: зафиксировать дату " + attribute, VirtualKeyShort.TAB);
            return this;
        }

        /// <summary>Открыть справочник атрибута кнопкой «…» поля (PART_Right_Button).</summary>
        public PilotReferenceBook OpenReferenceBook(string attribute)
        {
            AutomationElement button = Field(attribute).Find(By.Id(PilotIds.Card.FieldRightButton), null, "поле " + attribute);
            PilotDialog dialog = _app.Windows.OpenedBy(() => UiAct.Press(button, "«…» справочника " + attribute), By.Type(ControlType.Window));
            return new PilotReferenceBook(dialog);
        }

        /// <summary>Выбрать элемент справочника по точному заголовку через поиск.</summary>
        public PilotCard PickReference(string attribute, string itemTitle)
        {
            OpenReferenceBook(attribute).Search(itemTitle).Check(itemTitle).Submit();
            return this;
        }

        /// <summary>Выбрать элемент справочника, раскрывая дерево по пути заголовков (поиск сервера отстаёт для только что созданных объектов).</summary>
        public PilotCard PickReferenceByPath(string attribute, params string[] path)
        {
            if (path == null || path.Length == 0)
            {
                throw new ArgumentException("Путь по дереву справочника пуст", nameof(path));
            }
            OpenReferenceBook(attribute).ExpandPath(path.Take(path.Length - 1).ToArray()).Check(path.Last()).Submit();
            return this;
        }

        /// <summary>
        /// Добавить оргединицу в поле автодополнения AUIDNewOrgUnitAutocomplete;Stage:&lt;stage&gt; (stage - заголовок атрибута карточки или
        /// индекс этапа процесса): ввести searchText -> в списке подсказок autocompleteList пункт с Name == expectedName (единственный) -> выбрать.
        /// Постусловие: элементов AUIDOrgUnitItem:…;Stage:&lt;stage&gt; стало на один больше.
        /// </summary>
        public PilotCard AddOrgUnit(string stage, string searchText, string expectedName, OrgUnitPick pick = OrgUnitPick.MouseClick)
        {
            int before = OrgUnits(stage).Count;
            AutomationElement input = UiQuery.WaitFind(_scope, By.Id(PilotIds.Card.OrgUnitInput(stage)), Wait.DefaultTimeout, _description);
            UiAct.TypeText(input, searchText, "поиск оргединицы " + stage);
            By suggestion = By.Where(f => (f.ControlType == ControlType.ListItem || f.ControlType == ControlType.DataItem) && f.Name == expectedName, $"подсказка «{expectedName}»");
            AutomationElement list = Wait.ForStrict(() =>
                    UiQuery.TryFind(Scope, By.Id(PilotIds.Card.AutocompleteList), Wait.DefaultTimeout)
                    ?? _app.Windows.Popups().Select(p => UiQuery.TryFind(p, By.Id(PilotIds.Card.AutocompleteList), Wait.DefaultTimeout, 6)).FirstOrDefault(e => e != null),
                Wait.DefaultTimeout, "список подсказок autocompleteList", null);
            AutomationElement row = Wait.ForStrict(() => UiQuery.TryFind(list, suggestion, Wait.DefaultTimeout, 4), Wait.DefaultTimeout, $"подсказка «{expectedName}»",
                () => "подсказки: " + string.Join("; ", PilotList.Rows(list).Select(r => r.SafeName())));
            IReadOnlyList<AutomationElement> rows = PilotList.Rows(list);
            if (rows.Count(r => r.SafeName() == expectedName) > 1)
            {
                throw new AmbiguousElementException($"подсказка «{expectedName}»", rows.Select(r => r.SafeName()).ToList());
            }
            if (pick == OrgUnitPick.MouseClick)
            {
                UiAct.Click(row, $"подсказка «{expectedName}»", MouseButton.Left, false);
            }
            else
            {
                if (rows.Count != 1)
                {
                    throw new AmbiguousElementException($"выбор первой подсказки стрелкой (ожидалась единственная «{expectedName}»)", rows.Select(r => r.SafeName()).ToList());
                }
                UiAct.Keys(input, "первая подсказка", VirtualKeyShort.DOWN);
                UiAct.Keys(input, "выбрать подсказку", VirtualKeyShort.RETURN);
            }
            Wait.Strict(() => OrgUnits(stage).Count == before + 1, Wait.DefaultTimeout, $"оргединица «{expectedName}» добавлена в {stage}", () => "в поле: " + string.Join(", ", OrgUnits(stage)));
            return this;
        }

        /// <summary>Имена выбранных оргединиц поля (из AutomationId «AUIDOrgUnitItem:&lt;имя&gt;;Index:&lt;i&gt;;Stage:&lt;stage&gt;»).</summary>
        public IReadOnlyList<string> OrgUnits(string stage)
        {
            string suffix = ";Stage:" + stage;
            return Scope.FindAll(By.IdPrefix(PilotIds.Card.OrgUnitItemPrefix))
                .Select(e => e.SafeAutomationId())
                .Where(id => id.EndsWith(suffix, StringComparison.Ordinal))
                .Select(id =>
                {
                    string body = id.Substring(PilotIds.Card.OrgUnitItemPrefix.Length);
                    int index = body.LastIndexOf(";Index:", StringComparison.Ordinal);
                    return index >= 0 ? body.Substring(0, index) : body;
                }).ToList();
        }

        /// <summary>«атрибут = значение» по всем полям - для сообщений об ошибках.</summary>
        public string Describe()
        {
            return string.Join("; ", AttributeNames().Select(a => a + " = «" + SafeValue(a) + "»"));
        }

        public override string ToString()
        {
            return _description;
        }

        private string SafeValue(string attribute)
        {
            AutomationElement field = UiQuery.TryFind(Scope, By.Id(PilotIds.Card.Field(attribute)), Wait.DefaultTimeout);
            if (field == null)
            {
                return "(нет поля)";
            }
            AutomationElement input = ValueTarget(field);
            return input != null ? input.SafeValue() : string.Join(" ", field.Texts());
        }

        /// <summary>Куда писать текст: поле с ValuePattern (не ComboBox), иначе вложенный Edit с ValuePattern; null - некуда.</summary>
        private static AutomationElement ValueTarget(AutomationElement field)
        {
            bool fieldHasValue = UiQuery.Read(() => field.Patterns.Value.IsSupported, Wait.DefaultTimeout, "ValuePattern поля");
            if (fieldHasValue && field.SafeControlType() != ControlType.ComboBox)
            {
                return field;
            }
            AutomationElement edit = UiQuery.TryFind(field, By.Type(ControlType.Edit), Wait.DefaultTimeout, 4);
            if (edit != null && UiQuery.Read(() => edit.Patterns.Value.IsSupported, Wait.DefaultTimeout, "ValuePattern поля ввода"))
            {
                return edit;
            }
            return null;
        }

        /// <summary>
        /// Подпись пункта списка: текст дочернего TextBlock, если он есть, иначе Name. У пунктов с моделью-объектом Name - имя типа .NET,
        /// а подпись только в тексте (проверено: ComboBoxEdit DevExpress в окне модуля, PluginUserState).
        /// </summary>
        private static string ItemCaption(AutomationElement item)
        {
            return item.Texts().FirstOrDefault() ?? item.SafeName();
        }
    }

    /// <summary>Окно справочника (заголовок «Справочник», «Справочник элементов» или заданный модулем): поиск, дерево ascTreeView, флажки, OK/Отмена.</summary>
    public sealed class PilotReferenceBook : PilotDialog
    {
        private bool _changed;

        public PilotReferenceBook(PilotDialog dialog)
            : base(dialog)
        {
        }

        // заголовок строки - Name, читается строго (заголовок по умолчанию PilotList)
        private PilotList Tree => ListById(PilotIds.Card.ReferenceBook.Tree);

        /// <summary>Ввести текст в строку поиска справочника (searchEdit).</summary>
        public PilotReferenceBook Search(string text)
        {
            SetValue(PilotIds.Card.ReferenceBook.Search, text);
            return this;
        }

        /// <summary>Заголовки строк дерева (кроме служебной «Результаты поиска»).</summary>
        public IReadOnlyList<string> Titles()
        {
            return Tree.VisibleTitles().Where(t => t != PilotTitles.Explorer.SearchResults).ToList();
        }

        /// <summary>Раскрыть узлы по пути заголовков (кнопка раскрытия expander - кликом мышью, как в PilotObjectDialog.PickReferenceByPath).</summary>
        public PilotReferenceBook ExpandPath(params string[] path)
        {
            foreach (string part in path)
            {
                AutomationElement row = Tree.Row(part);
                AutomationElement expander = row.Find(By.Id(PilotIds.Card.ReferenceBook.Expander), null, "строка «" + part + "»");
                if (ToggleStateOf(expander, "кнопка раскрытия «" + part + "»") == ToggleState.On)
                {
                    continue;
                }
                UiAct.Click(expander, $"раскрыть «{part}»", MouseButton.Left, false);
                Uia.Wait.Strict(() => ToggleStateOf(expander, "кнопка раскрытия «" + part + "»") == ToggleState.On, Uia.Wait.DefaultTimeout, $"узел «{part}» раскрыт", null);
            }
            return this;
        }

        /// <summary>
        /// Отметить строку с точным заголовком: флажок строки (или заголовок у справочников без флажков) кликом мыши - программный Toggle
        /// не пересчитывает доступность кнопки OK (PilotObjectDialog.cs). Уже отмеченный флажок не нажимается: справочник элементов
        /// открывается с текущим значением атрибута, и клик снял бы отметку. Постусловие - флажок отмечен (без флажков - строка выделена)
        /// и, если отметка изменилась, OK доступна.
        /// </summary>
        public PilotReferenceBook Check(string title)
        {
            AutomationElement row = Tree.Row(title);
            AutomationElement checkBox = UiQuery.TryFind(row, By.Id(PilotIds.Card.ReferenceBook.CheckBox), Uia.Wait.DefaultTimeout, 3);
            if (checkBox != null)
            {
                string what = $"флажок «{title}» в справочнике";
                if (ToggleStateOf(checkBox, what) == ToggleState.On)
                {
                    PilotLog.Action($"«{title}» в справочнике уже отмечена - флажок не нажимается");
                    return this;
                }
                UiAct.Click(checkBox, $"отметить «{title}» в справочнике", MouseButton.Left, false);
                Uia.Wait.Strict(() => ToggleStateOf(checkBox, what) == ToggleState.On, Uia.Wait.DefaultTimeout, what + " отмечен", null);
            }
            else
            {
                AutomationElement target = UiQuery.TryFind(row, By.Id(PilotIds.Card.ReferenceBook.ItemTitle), Uia.Wait.DefaultTimeout, 3) ?? row;
                UiAct.Click(target, $"выбрать «{title}» в справочнике", MouseButton.Left, false);
                ISelectionItemPattern selection = UiQuery.Read(() => row.Patterns.SelectionItem.PatternOrDefault, Uia.Wait.DefaultTimeout, "SelectionItem строки справочника");
                if (selection != null)
                {
                    Uia.Wait.Strict(() => selection.IsSelected.Value, Uia.Wait.DefaultTimeout, $"строка «{title}» выделена в справочнике", null);
                }
            }
            _changed = true;
            AutomationElement ok = Find(PilotIds.Card.ReferenceBook.Ok);
            Uia.Wait.Strict(() => ok.Properties.IsEnabled.Value, Uia.Wait.DefaultTimeout, "кнопка OK справочника доступна", null);
            return this;
        }

        public void Ok()
        {
            ClickAndWaitClosed(PilotIds.Card.ReferenceBook.Ok);
        }

        /// <summary>
        /// Подтвердить выбор: OK. Если <see cref="Check"/> ничего не изменил (строка уже была отмечена) и OK недоступна - «Отмена»:
        /// значение атрибута уже такое. OK недоступна после изменения - исключение.
        /// </summary>
        public void Submit()
        {
            AutomationElement ok = Find(PilotIds.Card.ReferenceBook.Ok);
            if (UiQuery.Read(() => ok.Properties.IsEnabled.Value, Uia.Wait.DefaultTimeout, "доступность OK справочника"))
            {
                Ok();
                return;
            }
            if (_changed)
            {
                throw new InvalidOperationException($"Кнопка OK справочника «{Description}» недоступна после выбора.");
            }
            PilotLog.Action("выбор в справочнике не изменился (строка уже была отмечена) - «Отмена»");
            ClickAndWaitClosed(PilotIds.Card.ReferenceBook.Cancel);
        }

        public void Cancel()
        {
            ClickAndWaitClosed(PilotIds.MessageBox.ClientCancel);
        }

        /// <summary>Состояние флажка или кнопки раскрытия без подмены сбоя чтения значением; нет TogglePattern - исключение.</summary>
        private static ToggleState ToggleStateOf(AutomationElement element, string what)
        {
            ToggleState? state = UiQuery.Read(() => element.Patterns.Toggle.PatternOrDefault?.ToggleState.Value, Uia.Wait.DefaultTimeout, "состояние: " + what);
            return state ?? throw new InvalidOperationException(what + ": нет TogglePattern.");
        }
    }
}
