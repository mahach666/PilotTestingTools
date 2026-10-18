using System;
using System.Collections.Generic;
using System.Linq;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using PilotUITesting.Uia;

namespace PilotUITesting.Client
{
    /// <summary>
    /// Окно «Управление общими настройками» (главное меню СЕРВИС -> Настройки -> Управление общими настройками;
    /// доступно администратору). Дочернее окно главного окна клиента с идентификатором
    /// <see cref="WindowId"/>: таблица конфигураций по группам (заголовок группы - название настройки,
    /// строка - оргединица и значение) и кнопка «Добавить конфигурацию», раскрывающая меню настроек модулей
    /// (<c>ISettingsFeature</c>) с пунктами <c>add_{ключ настройки}</c>. Выбор пункта открывает окно
    /// <see cref="PilotNewSettingsItemWindow"/> с редактором настройки.
    /// Разведано на клиенте 26.11 (06.10.2026).
    /// </summary>
    public sealed class PilotCommonSettingsWindow : PilotDialog
    {
        /// <summary>Идентификатор окна (дочернее окно главного окна клиента).</summary>
        public const string WindowId = "Client.CommonSettings.Window";

        /// <summary>Кнопка «Добавить конфигурацию».</summary>
        public const string AddButtonId = "tbiAddSetting";

        /// <summary>Меню настроек под кнопкой «Добавить конфигурацию».</summary>
        public const string AddMenuId = "tbiAddSettingContextMenu";

        /// <summary>Таблица конфигураций.</summary>
        public const string GridId = "settingsDataGrid";

        /// <summary>Кнопка закрытия окна в заголовке.</summary>
        public const string CloseButtonId = "btnClose";

        /// <summary>Контекстное меню строки таблицы.</summary>
        public const string RowMenuId = "AUIDSettingsItemMenu";

        /// <summary>Пункт «Редактировать» контекстного меню строки (доступен при одной выделенной строке).</summary>
        public const string EditItemId = "miEdit";

        /// <summary>Пункт «Удалить» контекстного меню строки (с подтверждением).</summary>
        public const string RemoveItemId = "miRemove";

        private PilotCommonSettingsWindow(PilotApp app, AutomationElement window)
            : base(app, window)
        {
        }

        /// <summary>Идентификатор пункта меню «Добавить конфигурацию» для настройки с данным ключом.</summary>
        public static string AddItemId(string featureKey)
        {
            if (string.IsNullOrEmpty(featureKey))
            {
                throw new ArgumentException("Ключ настройки обязателен.", nameof(featureKey));
            }

            return "add_" + featureKey;
        }

        /// <summary>Открывает окно через главное меню и ждёт его появления.</summary>
        public static PilotCommonSettingsWindow Open(PilotApp app, TimeSpan? timeout = null)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            app.MainMenu.Click(PilotIds.MainMenu.Service, PilotIds.MainMenu.Options, PilotIds.MainMenu.CommonSettings);
            return Wait(app, timeout);
        }

        /// <summary>Ждёт уже открывающееся окно.</summary>
        public static PilotCommonSettingsWindow Wait(PilotApp app, TimeSpan? timeout = null)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            AutomationElement window = Uia.Wait.For(() => app.MainWindow?.TryById(WindowId), timeout ?? Uia.Wait.DefaultTimeout, "окно «Управление общими настройками»");
            return new PilotCommonSettingsWindow(app, window);
        }

        /// <summary>Открытое окно либо null.</summary>
        public static PilotCommonSettingsWindow TryFind(PilotApp app)
        {
            AutomationElement window = app?.MainWindow?.TryById(WindowId);
            return window == null ? null : new PilotCommonSettingsWindow(app, window);
        }

        /// <summary>Названия групп таблицы - настройки, у которых есть хотя бы одна конфигурация.</summary>
        public IReadOnlyList<string> Groups()
        {
            return Root.FindDescendantsBfs(element => element.SafeControlType() == ControlType.Group, 6)
                .Select(group => group.SafeName())
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();
        }

        /// <summary>Строки таблицы конфигураций (элементы DataItem; при виртуализации - только построенные).</summary>
        public IReadOnlyList<AutomationElement> Rows()
        {
            return Find(GridId).FindDescendantsBfs(element => element.SafeControlType() == ControlType.DataItem, 6);
        }

        /// <summary>Первая строка, в текстах которой есть подстрока; null - такой нет.</summary>
        public AutomationElement TryFindRow(string textPart)
        {
            if (string.IsNullOrEmpty(textPart))
            {
                throw new ArgumentException("Искомый текст обязателен.", nameof(textPart));
            }

            return Rows().FirstOrDefault(row => row.Texts().Any(text => text.IndexOf(textPart, StringComparison.Ordinal) >= 0));
        }

        /// <summary>
        /// Выделяет строку, открывает её контекстное меню правым кликом (строка таблицы не принимает фокус через
        /// UI Automation, клавиша Apps недоступна), выбирает «Редактировать» и ждёт окно конфигурации.
        /// </summary>
        public PilotNewSettingsItemWindow EditRow(AutomationElement row, TimeSpan? timeout = null)
        {
            if (row == null)
            {
                throw new ArgumentNullException(nameof(row));
            }

            UiActions.Select(row, "строка таблицы конфигураций");
            PilotMenu menu = PilotMenu.OpenFor(App, row, RowMenuId, MenuOpenMethod.RightClick);
            menu.Click(EditItemId);
            return PilotNewSettingsItemWindow.Wait(App, timeout);
        }

        /// <summary>
        /// Удаляет строку: контекстное меню строки -> «Удалить» -> «Да» в подтверждении клиента; ждёт, пока строки
        /// с текстом <paramref name="textPart"/> не станет.
        /// </summary>
        public void RemoveRow(string textPart, TimeSpan? timeout = null)
        {
            AutomationElement row = TryFindRow(textPart) ?? throw new InvalidOperationException("В таблице общих настроек нет строки с текстом «" + textPart + "».");
            UiActions.Select(row, "строка таблицы конфигураций");
            HashSet<IntPtr> before = new HashSet<IntPtr>(App.Windows.All().Select(window => window.Handle));
            PilotMenu menu = PilotMenu.OpenFor(App, row, RowMenuId, MenuOpenMethod.RightClick);
            menu.Click(RemoveItemId);
            PilotDialog confirmation = Uia.Wait.For(() => App.Windows.All().FirstOrDefault(window => !before.Contains(window.Handle)),
                timeout ?? Uia.Wait.DefaultTimeout, "подтверждение удаления строки общих настроек");
            new PilotMessageBox(confirmation).Answer(MessageBoxAnswer.Yes);
            Uia.Wait.Until(() => TryFindRow(textPart) == null, timeout ?? Uia.Wait.DefaultTimeout, "строка «" + textPart + "» удалена");
        }

        /// <summary>Раскрывает меню «Добавить конфигурацию» и возвращает его.</summary>
        public PilotMenu OpenAddMenu()
        {
            Click(AddButtonId);
            return PilotMenu.Wait(App, AddMenuId);
        }

        /// <summary>Выбирает настройку в меню «Добавить конфигурацию» и ждёт окно новой конфигурации.</summary>
        public PilotNewSettingsItemWindow AddConfiguration(string featureKey, TimeSpan? timeout = null)
        {
            PilotMenu menu = OpenAddMenu();
            menu.Click(AddItemId(featureKey));
            return PilotNewSettingsItemWindow.Wait(App, timeout);
        }

        /// <summary>Закрывает окно кнопкой заголовка (<see cref="CloseButtonId"/>) и ждёт закрытия.</summary>
        public void CloseWithButton()
        {
            if (IsOpen)
            {
                ClickAndWaitClosed(CloseButtonId);
            }
        }
    }

    /// <summary>
    /// Окно «Новая конфигурация» общих настроек: заголовок настройки (<see cref="GroupTitleId"/>), выбор
    /// организационной единицы ссылкой «выбрать» (<see cref="PickOrgUnitLinkId"/>, открывает модальное окно
    /// «Организационная структура»), поле описания и редактор настройки модуля (<c>ISettingsFeature.Editor</c>);
    /// кнопка OK недоступна, пока не выбрана оргединица. У окна имя <see cref="WindowName"/>, а идентификатор
    /// совпадает с заголовком. Разведано на клиенте 26.11 (06.10.2026).
    /// </summary>
    public sealed class PilotNewSettingsItemWindow : PilotDialog
    {
        /// <summary>Имя окна (свойство Name элемента).</summary>
        public const string WindowName = "AUIDNewSettingsItem";

        /// <summary>Заголовок окна и одновременно его идентификатор.</summary>
        public const string WindowTitle = "Новая конфигурация";

        /// <summary>Текст с названием настройки.</summary>
        public const string GroupTitleId = "GroupTitleTextBlock";

        /// <summary>Ссылка «выбрать» организационной единицы.</summary>
        public const string PickOrgUnitLinkId = "showExecutorsLink";

        /// <summary>Кнопка OK.</summary>
        public const string OkButtonId = "btnOk";

        /// <summary>Кнопка «Отмена».</summary>
        public const string CancelButtonId = "btnCancel";

        private PilotNewSettingsItemWindow(PilotApp app, AutomationElement window)
            : base(app, window)
        {
        }

        /// <summary>Ждёт окно новой конфигурации среди дочерних окон главного окна.</summary>
        public static PilotNewSettingsItemWindow Wait(PilotApp app, TimeSpan? timeout = null)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            AutomationElement window = Uia.Wait.For(() => Find(app), timeout ?? Uia.Wait.DefaultTimeout, "окно «" + WindowTitle + "»");
            return new PilotNewSettingsItemWindow(app, window);
        }

        /// <summary>Открытое окно либо null.</summary>
        public static PilotNewSettingsItemWindow TryFind(PilotApp app)
        {
            AutomationElement window = app == null ? null : Find(app);
            return window == null ? null : new PilotNewSettingsItemWindow(app, window);
        }

        /// <summary>Название настройки, показанное в заголовке окна.</summary>
        public string FeatureTitle => Find(GroupTitleId).SafeName();

        /// <summary>OK доступен (оргединица выбрана, редактор не запретил).</summary>
        public bool CanSubmit => IsEnabled(By.Id(OkButtonId));

        /// <summary>
        /// Открывает окно «Организационная структура» ссылкой «выбрать». Щелчок - мышью, не Invoke: ссылка WPF
        /// (HyperlinkAutomationPeer) выполняет щелчок синхронно внутри запроса UI Automation, и открытое так
        /// модальное окно блокирует все следующие запросы к клиенту, пока не закроется (видны только оболочки окон).
        /// </summary>
        public PilotOrgUnitPickerWindow OpenOrgUnitPicker(TimeSpan? timeout = null)
        {
            UiActions.Click(Find(PickOrgUnitLinkId, timeout), "ссылка «выбрать» оргединицы");
            return PilotOrgUnitPickerWindow.Wait(App, timeout);
        }

        /// <summary>Выбирает оргединицу по подписи в дереве оргструктуры (например «bot2 (bot)» или «bots»).</summary>
        public void PickOrgUnit(string caption, TimeSpan? timeout = null)
        {
            PilotOrgUnitPickerWindow picker = OpenOrgUnitPicker(timeout);
            picker.Choose(caption, timeout);
        }

        /// <summary>Нажимает OK и ждёт закрытия окна (редактор может отклонить сохранение окном ошибки - тогда окно остаётся).</summary>
        public void Submit()
        {
            ClickAndWaitClosed(OkButtonId);
        }

        /// <summary>Нажимает «Отмена» и ждёт закрытия окна.</summary>
        public void Cancel()
        {
            if (IsOpen)
            {
                ClickAndWaitClosed(CancelButtonId);
            }
        }

        private static AutomationElement Find(PilotApp app)
        {
            AutomationElement main = app.MainWindow;
            if (main == null)
            {
                return null;
            }

            return main.FindDescendantsBfs(element => element.SafeControlType() == ControlType.Window && element.SafeName() == WindowName, 3).FirstOrDefault();
        }
    }

    /// <summary>
    /// Окно «Организационная структура» клиента (DialogService.ShowOrganisationUnitsPickerDialog, OrgStructureView):
    /// фильтр <see cref="FilterId"/>, дерево-список <see cref="ListId"/> с флажком <see cref="CheckBoxId"/> у пункта,
    /// кнопки «Выбрать» (<see cref="ChooseButtonId"/>, доступна после отметки) и «Отмена». Разведано на клиенте 26.11.
    /// </summary>
    public sealed class PilotOrgUnitPickerWindow : PilotDialog
    {
        public const string WindowTitle = "Организационная структура";
        public const string FilterId = "filterEdit";
        public const string ListId = "ascTreeView";
        public const string CheckBoxId = "checkBox";
        public const string ChooseButtonId = "btnAdd";
        public const string CancelButtonId = "btnCancel";

        private PilotOrgUnitPickerWindow(PilotDialog dialog)
            : base(dialog)
        {
        }

        public static PilotOrgUnitPickerWindow Wait(PilotApp app, TimeSpan? timeout = null)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return new PilotOrgUnitPickerWindow(app.Windows.Wait(WindowTitle, timeout));
        }

        /// <summary>Пункт дерева с подписью, равной <paramref name="caption"/>; null - не показан (виртуализация, фильтр).</summary>
        public AutomationElement TryItem(string caption)
        {
            AutomationElement list = TryFind(ListId);
            return list?.FindDescendantsBfs(element => element.SafeControlType() == ControlType.ListItem && element.SafeName() == caption, 2).FirstOrDefault();
        }

        /// <summary>Сузить дерево фильтром, отметить пункт и нажать «Выбрать»; ждёт закрытия окна.</summary>
        public void Choose(string caption, TimeSpan? timeout = null)
        {
            if (string.IsNullOrEmpty(caption))
            {
                throw new ArgumentException("Подпись оргединицы обязательна.", nameof(caption));
            }

            SetValue(FilterId, caption);
            AutomationElement item = Uia.Wait.For(() => TryItem(caption), timeout ?? Uia.Wait.DefaultTimeout, "оргединица «" + caption + "» в дереве");
            AutomationElement checkBox = item.FindDescendantsBfs(element => element.SafeAutomationId() == CheckBoxId, 2).FirstOrDefault()
                ?? throw new InvalidOperationException("У пункта «" + caption + "» нет флажка выбора (пункт не выбирается).");
            UiActions.SetChecked(checkBox, true, CheckInput.MouseClick, "флажок оргединицы «" + caption + "»");
            Uia.Wait.Until(() => IsEnabled(By.Id(ChooseButtonId)), timeout ?? Uia.Wait.DefaultTimeout, "кнопка «Выбрать» доступна");
            ClickAndWaitClosed(ChooseButtonId);
        }

        public void Cancel()
        {
            if (IsOpen)
            {
                ClickAndWaitClosed(CancelButtonId);
            }
        }
    }
}
