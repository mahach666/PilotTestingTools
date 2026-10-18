using System;
using System.Collections.Generic;
using System.Linq;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.WindowsAPI;
using PilotUITesting.Data;
using PilotUITesting.Uia;

namespace PilotUITesting.Client
{
    /// <summary>
    /// Проводник «Проекты и документы» (AUIDDocumentsExplorer): панель команд, список объектов, дерево, строка навигации, контекстное меню,
    /// создание объекта, карточка, удаление. Поиск - только внутри этой вкладки.
    /// Переход к объекту - <see cref="GoTo(ObjectRef)"/> (piloturi через exe клиента, проверено на Pilot-ICE 26.11: строка выделяется и получает фокус).
    /// </summary>
    public sealed class PilotExplorer : PilotTabPage
    {
        private PilotExplorer(PilotTab tab)
            : base(tab)
        {
        }

        /// <summary>
        /// Открыть новую вкладку «Проекты и документы» плиткой «Новой вкладки». Новая вкладка открывается «в последнем месте», а не в «Начале»
        /// (при необходимости - <see cref="GoHome"/>); у вкладок, восстановленных клиентом при запуске, нет кнопок панелей от модулей -
        /// поэтому тестам нужна своя вкладка.
        /// </summary>
        public static PilotExplorer Open(PilotApp app)
        {
            PilotTab tab = app.TabBar.OpenTile(PilotTitles.NewTab.ProjectsAndDocuments, PilotIds.TabContent.DocumentsExplorer);
            PilotLog.Action("открыта вкладка проводника «" + tab.LastKnownTitle + "»");
            return new PilotExplorer(tab);
        }

        /// <summary>Работать с уже открытой вкладкой проводника.</summary>
        public static PilotExplorer Attach(PilotTab tab)
        {
            if (tab.ContentId != PilotIds.TabContent.DocumentsExplorer)
            {
                throw new ArgumentException($"{tab} - не проводник (содержимое {tab.ContentId}).", nameof(tab));
            }
            return new PilotExplorer(tab);
        }

        /// <summary>Панель команд проводника (objectListViewerToolbar).</summary>
        public PilotToolbar Toolbar => ToolbarById(PilotIds.Explorer.Toolbar.Container);

        /// <summary>Список объектов (ascListView.ListView); заголовок строки - Name.</summary>
        public PilotList List => ListById(PilotIds.Explorer.List.View);

        /// <summary>Дерево навигации слева (ascTreeView).</summary>
        public PilotList Tree => ListById(PilotIds.Explorer.Tree.List);

        /// <summary>Уровни строки навигации (navigationItemButton: HelpText = заголовок уровня).</summary>
        public IReadOnlyList<string> Location()
        {
            AutomationElement bar = Find(PilotIds.Explorer.Navigation.Bar);
            return bar.FindAll(By.Id(PilotIds.Explorer.Navigation.Item)).Select(b => b.SafeHelpText()).Where(t => t.Length > 0).ToList();
        }

        /// <summary>В «Начало»: btnHome, если доступна; постусловие - btnHome недоступна (признак «Начала»).</summary>
        public PilotExplorer GoHome()
        {
            AutomationElement home = Find(PilotIds.Explorer.Navigation.Home);
            // сбой чтения доступности не подменяется ответом «недоступна»: иначе «уже в Начале» без перехода
            if (UiQuery.Read(() => home.Properties.IsEnabled.Value, Uia.Wait.DefaultTimeout, "доступность btnHome"))
            {
                UiAct.Press(home, "«В начало» проводника");
            }
            Uia.Wait.Strict(() => !Find(PilotIds.Explorer.Navigation.Home).Properties.IsEnabled.Value, Uia.Wait.DefaultTimeout, "проводник в «Начале» (btnHome недоступна)", null);
            return this;
        }

        /// <summary>
        /// Перейти к объекту по id (вкладка сначала делается активной, затем <see cref="PilotApp.ShowObject"/>). Постусловие: в списке выделена
        /// ровно одна строка - target.Title - и она в фокусе клавиатуры (сразу работают Apps, Delete, Enter).
        /// </summary>
        public PilotExplorer GoTo(ObjectRef target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }
            return GoTo(target.Id, target.Title);
        }

        public PilotExplorer GoTo(Guid objectId, string expectedTitle)
        {
            Tab.Activate();
            App.ShowObject(objectId);
            Uia.Wait.Strict(() =>
                {
                    IReadOnlyList<string> selected = List.SelectedTitles();
                    return selected.Count == 1 && selected[0] == expectedTitle && Focused(expectedTitle);
                }, App.Config.DataTimeout, $"в проводнике выделена и в фокусе строка «{expectedTitle}»",
                () => "выделено: " + string.Join("; ", List.SelectedTitles()));
            // заголовок вкладки проводника меняется вместе с объектом - запоминаем (по нему вкладку находят после перезапуска клиента)
            PilotLog.Action("вкладка проводника: «" + Tab.Title + "»");
            return this;
        }

        /// <summary>
        /// Перейти по пути заголовков: «показать путь строкой» (showPathAsStringButton) -> directoryEdit = «\A\B» -> Enter.
        /// Постусловие - последний уровень навигации = последний заголовок.
        /// </summary>
        public PilotExplorer GoToPath(params string[] titles)
        {
            if (titles == null || titles.Length == 0)
            {
                throw new ArgumentException("Пустой путь", nameof(titles));
            }
            AutomationElement toggle = Find(PilotIds.Explorer.Navigation.ShowPathAsString);
            ToggleState? state = UiQuery.Read(() => toggle.Patterns.Toggle.PatternOrDefault?.ToggleState.Value, Uia.Wait.DefaultTimeout, "состояние showPathAsStringButton");
            if (state == null)
            {
                throw new InvalidOperationException($"{PilotIds.Explorer.Navigation.ShowPathAsString} ({Description}) не поддерживает TogglePattern.");
            }
            if (state != ToggleState.On)
            {
                UiAct.Press(toggle, "«показать путь строкой»");
            }
            AutomationElement edit = Find(PilotIds.Explorer.Navigation.PathEdit);
            string path = "\\" + string.Join("\\", titles);
            SetValue(edit, path, "строка пути проводника");
            UiAct.Keys(edit, "Enter: перейти по пути", VirtualKeyShort.RETURN);
            WaitLocation(titles.Last());
            return this;
        }

        /// <summary>Войти в строку текущего уровня: выделить, фокус, Enter; постусловие - последний уровень навигации = title.</summary>
        public PilotExplorer OpenRow(string title)
        {
            List.Select(title);
            UiAct.Keys(List.Row(title), $"Enter: войти в «{title}»", VirtualKeyShort.RETURN);
            WaitLocation(title);
            return this;
        }

        /// <summary>Назад (btnBack).</summary>
        public PilotExplorer Back()
        {
            Click(PilotIds.Explorer.Navigation.Back);
            return this;
        }

        /// <summary>Выделить строку и передать ей фокус.</summary>
        public PilotExplorer Select(string title)
        {
            List.Select(title);
            return this;
        }

        /// <summary>Выделить ровно эти строки текущего уровня (первая - с фокусом клавиатуры, остальные - AddToSelection).</summary>
        public PilotExplorer SelectMany(params string[] titles)
        {
            List.SelectMany(titles);
            return this;
        }

        /// <summary>Выделить все строки (Ctrl+A на первой строке); постусловие - выделено больше одной строки.</summary>
        public PilotExplorer SelectAll()
        {
            AutomationElement first = PilotList.Rows(List.Root).FirstOrDefault() ?? throw new InvalidOperationException("В списке проводника нет строк.");
            UiAct.Keys(first, "Ctrl+A: выделить всё", VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_A);
            Uia.Wait.Strict(() => List.SelectedTitles().Count > 1, Uia.Wait.DefaultTimeout, "выделено больше одной строки", null);
            return this;
        }

        /// <summary>Контекстное меню выделенной строки (AUIDListViewContextMenu): клавишей «Меню» по строке в фокусе (по умолчанию) или правым кликом.</summary>
        public PilotMenu ContextMenu(MenuOpenMethod method = MenuOpenMethod.AppsKey)
        {
            IReadOnlyList<string> selected = List.SelectedTitles();
            if (selected.Count == 0)
            {
                throw new InvalidOperationException("Контекстное меню проводника: ни одна строка не выделена (сначала GoTo/Select).");
            }
            AutomationElement row = List.Row(selected[selected.Count - 1]);
            return PilotMenu.OpenFor(App, row, PilotIds.Explorer.ListMenu.Container, method);
        }

        /// <summary>
        /// Создать объект типа typeName в текущей папке: «Создать» (miCreate) -> в меню кнопки пункт «miCreate&lt;XamlName(тип)&gt;» (если меню есть) ->
        /// стандартная карточка создания.
        /// </summary>
        public PilotObjectDialog CreateObject(string typeName)
        {
            string item = PilotIds.Explorer.CreateItem(typeName);
            string menuId = PilotIds.ToolbarStripe.MenuOf(PilotIds.Explorer.Toolbar.Create);
            By cardWindow = By.Id(PilotIds.Card.Container);
            HashSet<string> before = App.Windows.Snapshot();
            object outcome;
            if (Toolbar.IsHidden(PilotIds.Explorer.Toolbar.Create))
            {
                // «Создать», ушедшая при переполнении панели в «…», - там заголовок подменю с типами
                outcome = Toolbar.OpenMenu(PilotIds.Explorer.Toolbar.Create);
            }
            else
            {
                Toolbar.Click(PilotIds.Explorer.Toolbar.Create);
                // «Создать» либо сразу открывает карточку (один допустимый тип), либо показывает меню типов miCreateContextMenu
                outcome = Uia.Wait.ForStrict(() => (object)App.Windows.TryNew(before, cardWindow) ?? PilotMenu.TryOpen(App, menuId),
                    App.Config.DataTimeout, "карточка создания или меню типов после «Создать»", null);
            }
            if (outcome is PilotMenu menu)
            {
                if (!menu.Has(item))
                {
                    string items = string.Join("; ", menu.Items());
                    menu.Close();
                    throw new InvalidOperationException($"В меню «Создать» нет пункта {item} (тип {typeName}). Пункты: {items}");
                }
                menu.Click(item);
                outcome = Uia.Wait.ForStrict(() => App.Windows.TryNew(before, cardWindow), App.Config.DataTimeout, "карточка создания объекта " + typeName, null);
            }
            return PilotObjectDialog.Wrap((PilotDialog)outcome);
        }

        /// <summary>Карточка выделенного объекта (miShowProperties) -> окно редактирования.</summary>
        public PilotObjectDialog OpenProperties()
        {
            PilotDialog card = App.Windows.OpenedBy(() => Toolbar.Click(PilotIds.Explorer.Toolbar.ShowProperties), By.Id(PilotIds.Card.Container), App.Config.DataTimeout);
            return PilotObjectDialog.Wrap(card);
        }

        /// <summary>Удалить выделенное: «Удалить» (miDelete) -> «Подтверждение операции удаления» -> «Удалить» (6).</summary>
        public void DeleteSelected()
        {
            PilotDialog confirm = App.Windows.OpenedBy(() => Toolbar.Click(PilotIds.Explorer.Toolbar.Delete), By.Id(PilotTitles.Windows.DeleteConfirmation));
            new PilotMessageBox(confirm).Answer(MessageBoxAnswer.Yes);
        }

        private void WaitLocation(string title)
        {
            Uia.Wait.Strict(() => Location().LastOrDefault() == title, App.Config.DataTimeout, $"проводник перешёл в «{title}»",
                () => "уровни: " + string.Join(" › ", Location()));
        }

        private bool Focused(string title)
        {
            AutomationElement row = List.TryRow(title);
            if (row == null)
            {
                return false;
            }
            return row.SafeHasKeyboardFocus() || UiQuery.Find(row, By.Where(f => f.Element.Properties.HasKeyboardFocus.ValueOrDefault, "фокус"), 6, 1).Matches.Count > 0;
        }
    }
}
