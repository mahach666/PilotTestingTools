using System;
using System.Collections.Generic;
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
    /// <summary>Как открыть контекстное меню.</summary>
    public enum MenuOpenMethod
    {
        /// <summary>Клавиша «Меню» (VK_APPS) - элемент должен иметь фокус клавиатуры; работает без DPI-осведомлённости.</summary>
        AppsKey,

        /// <summary>Правый клик мышью - требует DPI-осведомлённости процесса тестов (dpiAware).</summary>
        RightClick,
    }

    /// <summary>Пункт меню.</summary>
    public sealed class PilotMenuItem
    {
        internal PilotMenuItem(string id, string caption, bool isEnabled, bool hasSubmenu)
            : this(id, caption, isEnabled, hasSubmenu, null)
        {
        }

        internal PilotMenuItem(string id, string caption, bool isEnabled, bool hasSubmenu, bool? isChecked)
        {
            Id = id;
            Caption = caption;
            IsEnabled = isEnabled;
            HasSubmenu = hasSubmenu;
            IsChecked = isChecked;
        }

        /// <summary>Отметка пункта: TogglePattern (отмечаемый пункт WPF), иначе ItemStatus «IsChecked:…» (пункты меню панелей); null - пункт не отмечаемый.</summary>
        public bool? IsChecked { get; }

        /// <summary>AutomationId = имя пункта из построителя меню (у пунктов модулей - name из IMenuBuilder.AddItem).</summary>
        public string Id { get; }

        /// <summary>Подпись без знака ускорителя: Name, иначе текст дочернего TextBlock.</summary>
        public string Caption { get; }

        public bool IsEnabled { get; }

        public bool HasSubmenu { get; }

        public override string ToString()
        {
            return $"{Id} «{Caption}»{(IsEnabled ? "" : " недоступен")}{(HasSubmenu ? " ›" : "")}";
        }
    }

    /// <summary>
    /// Меню: контекстное (AUIDListViewContextMenu, AUIDTaskListContextMenu, AUIDAnnotationsListContextMenu …), меню кнопки панели
    /// («&lt;id&gt;ContextMenu»), главное меню. Открытое меню - во всплывающем окне класса Popup (дочернее окно главного).
    /// Пункт ищется по AutomationId; подменю строятся лениво - пункты ждутся после раскрытия.
    /// </summary>
    public sealed class PilotMenu
    {
        private readonly PilotApp _app;
        private readonly Func<AutomationElement> _root;
        private readonly string _description;
        private readonly IntPtr _popup;

        internal PilotMenu(PilotApp app, Func<AutomationElement> root, string description, IntPtr popup = default(IntPtr))
        {
            _app = app;
            _root = root;
            _description = description;
            _popup = popup;
            Id = description;
        }

        /// <summary>AutomationId меню (или описание, если у меню нет id).</summary>
        public string Id { get; }

        public AutomationElement Root => _root() ?? throw new ElementNotFoundException(_description, "меню", TimeSpan.Zero, TimeSpan.Zero, null, null);

        /// <summary>
        /// Меню открыто: всплывающее окно меню видно. У главного меню (строка меню всегда на экране) и его подменю своего всплывающего
        /// окна нет - открыто, если пункт подменю раскрыт (у строки меню - раскрыт один из пунктов).
        /// </summary>
        public bool IsOpen
        {
            get
            {
                if (_popup != IntPtr.Zero)
                {
                    return Native.IsShown(_popup);
                }
                AutomationElement root = _root();
                if (root == null)
                {
                    return false;
                }
                IExpandCollapsePattern own = UiQuery.Read(() => root.Patterns.ExpandCollapse.PatternOrDefault, Uia.Wait.DefaultTimeout, "ExpandCollapse " + _description);
                if (own != null)
                {
                    return IsExpanded(own, _description);
                }
                return ItemElements(root).Any(item =>
                {
                    IExpandCollapsePattern pattern = UiQuery.Read(() => item.Patterns.ExpandCollapse.PatternOrDefault, Uia.Wait.DefaultTimeout, "ExpandCollapse пункта " + _description);
                    return pattern != null && IsExpanded(pattern, _description);
                });
            }
        }

        /// <summary>Дождаться открытого меню с AutomationId (во всплывающих окнах процесса).</summary>
        public static PilotMenu Wait(PilotApp app, string menuId, TimeSpan? timeout = null)
        {
            (AutomationElement popup, AutomationElement menu) = Uia.Wait.ForStrict(() => FindOpen(app, By.Id(menuId)), timeout ?? Uia.Wait.DefaultTimeout,
                "меню " + menuId, () => DescribePopups(app));
            return Create(app, popup, menu, menuId);
        }

        /// <summary>Открытое меню с AutomationId без ожидания; null - не открыто.</summary>
        internal static PilotMenu TryOpen(PilotApp app, string menuId)
        {
            Tuple<AutomationElement, AutomationElement> found = FindOpen(app, By.Id(menuId));
            return found == null ? null : Create(app, found.Item1, found.Item2, menuId);
        }

        /// <summary>
        /// Открыть контекстное меню элемента (строки списка, ячейки таблицы): Apps с фокусом на элементе или правый клик.
        /// menuId null - меню без AutomationId (WPF ContextMenu модулей): берётся новое всплывающее окно с меню.
        /// </summary>
        public static PilotMenu OpenFor(PilotApp app, AutomationElement target, string menuId = null, MenuOpenMethod method = MenuOpenMethod.AppsKey)
        {
            HashSet<IntPtr> before = new HashSet<IntPtr>(app.Windows.Popups().Select(Handle));
            if (method == MenuOpenMethod.AppsKey)
            {
                UiAct.Keys(target, "контекстное меню " + target.Describe(), VirtualKeyShort.APPS);
            }
            else
            {
                UiAct.Click(target, target.Describe(), MouseButton.Right, false);
            }
            if (menuId != null)
            {
                return Wait(app, menuId);
            }
            (AutomationElement popup, AutomationElement menu) = Uia.Wait.ForStrict(() =>
            {
                foreach (AutomationElement candidate in app.Windows.Popups().Where(p => !before.Contains(Handle(p))))
                {
                    AutomationElement menuElement = UiQuery.TryFind(candidate, By.Type(ControlType.Menu), Uia.Wait.DefaultTimeout, 3);
                    if (menuElement != null)
                    {
                        return Tuple.Create(candidate, menuElement);
                    }
                }
                return null;
            }, Uia.Wait.DefaultTimeout, "новое контекстное меню", () => DescribePopups(app));
            return Create(app, popup, menu, "контекстное меню без id");
        }

        /// <summary>Пункты первого уровня.</summary>
        public IReadOnlyList<PilotMenuItem> Items()
        {
            return ItemElements(Root).Select(Describe).ToList();
        }

        public bool Has(string itemId)
        {
            return ItemElements(Root).Any(i => UiQuery.Read(() => i.Properties.AutomationId.ValueOrDefault ?? "", Uia.Wait.DefaultTimeout, "AutomationId пункта меню") == itemId);
        }

        /// <summary>
        /// Нажать пункт по пути AutomationId: все, кроме последнего, - подменю (раскрыть и дождаться ленивых пунктов);
        /// последний - Invoke. Пункт есть, но недоступен - исключение. После нажатия ждётся закрытие меню.
        /// </summary>
        public void Click(params string[] path)
        {
            ClickBy(path.Select(id => By.Id(id)).ToArray(), string.Join(" › ", path));
        }

        /// <summary>Нажать пункт по пути подписей (для меню модулей без AutomationId у пунктов).</summary>
        public void ClickText(params string[] captions)
        {
            ClickBy(captions.Select(c => By.Where(f => f.ControlType == ControlType.MenuItem && Caption(f.Element) == c, "пункт «" + c + "»")).ToArray(), string.Join(" › ", captions));
        }

        /// <summary>Раскрыть подменю и вернуть его как меню.</summary>
        public PilotMenu Submenu(string itemId)
        {
            AutomationElement item = WaitItem(Root, By.Id(itemId));
            Expand(item, itemId);
            return new PilotMenu(_app, () => item, _description + " › " + itemId, _popup);
        }

        /// <summary>Закрыть меню клавишей Esc (до трёх нажатий) и дождаться закрытия.</summary>
        public void Close()
        {
            for (int i = 0; i < 3 && IsOpen; i++)
            {
                _app.SendKeys("Esc: закрыть " + _description, VirtualKeyShort.ESCAPE);
                Uia.Wait.TryUntil(() => !IsOpen, TimeSpan.FromSeconds(2));
            }
            if (IsOpen)
            {
                throw new InvalidOperationException($"Меню {_description} не закрылось по Esc.");
            }
        }

        public override string ToString()
        {
            return _description;
        }

        private void ClickBy(By[] path, string text)
        {
            if (path.Length == 0)
            {
                throw new ArgumentException("Пустой путь пункта меню", nameof(path));
            }
            AutomationElement scope = Root;
            for (int i = 0; i < path.Length; i++)
            {
                AutomationElement item = WaitItem(scope, path[i]);
                if (!item.SafeIsEnabled())
                {
                    throw new InvalidOperationException($"Пункт {path[i]} меню {_description} есть, но недоступен. Пункты: {string.Join("; ", ItemElements(scope).Select(Describe))}");
                }
                if (i < path.Length - 1)
                {
                    Expand(item, path[i].ToString());
                    scope = item;
                    continue;
                }
                PilotLog.Action("меню " + _description + " › " + text);
                UiAct.Press(item, "пункт " + path[i] + " меню " + _description);
            }
            if (_popup != IntPtr.Zero)
            {
                Uia.Wait.Strict(() => !IsOpen, Uia.Wait.DefaultTimeout, "закрытие меню " + _description + " после нажатия", null);
            }
        }

        /// <summary>Пункт ищется под элементом-областью (пункты подменю в UIA - дети раскрытого пункта), затем во всплывающих окнах.</summary>
        private AutomationElement WaitItem(AutomationElement scope, By selector)
        {
            return Uia.Wait.ForStrict(() =>
                UiQuery.TryFind(scope, selector, Uia.Wait.DefaultTimeout, 3)
                ?? _app.Windows.Popups().Select(p => UiQuery.TryFind(p, selector, Uia.Wait.DefaultTimeout, 6)).FirstOrDefault(e => e != null),
                Uia.Wait.DefaultTimeout, "пункт " + selector + " меню " + _description, () => DescribeItems(scope));
        }

        /// <summary>
        /// Пункты для сообщения об ошибке. Меню открыто, а пунктов в дереве нет - так выглядит устаревшее дерево UI Automation после
        /// представления модуля: меню кнопки перестраивается при каждом открытии, и новые пункты в UIA не появляются (проверено на Pilot-ICE 26.11).
        /// </summary>
        private string DescribeItems(AutomationElement scope)
        {
            List<AutomationElement> items = ItemElements(scope).ToList();
            if (items.Count > 0)
            {
                return "пункты: " + string.Join("; ", items.Select(Describe));
            }
            string text = "меню открыто, но пунктов в дереве UI Automation нет";
            if (_app.ModuleViewOpened)
            {
                text += " - в этом сеансе клиента открывалось представление модуля, после этого повторно открытые меню не видны в UI Automation; " +
                        "откройте меню в свежем сеансе клиента (перезапуск: restartAfterModuleView между тестами) или один раз за сеанс";
            }
            return text;
        }

        private static void Expand(AutomationElement item, string what)
        {
            IExpandCollapsePattern expand = UiQuery.Read(() => item.Patterns.ExpandCollapse.PatternOrDefault, Uia.Wait.DefaultTimeout, "ExpandCollapse " + what);
            if (expand == null)
            {
                throw new InvalidOperationException($"Пункт {what} не раскрывается (нет ExpandCollapsePattern): это не подменю.");
            }
            UiAct.RunOnce(item, "подменю " + what, expand.Expand, "Expand");
        }

        /// <summary>Пункты меню - прямые потомки-MenuItem; клиент занят - повтор чтения, затем исключение (а не «пунктов нет»).</summary>
        private static IEnumerable<AutomationElement> ItemElements(AutomationElement scope)
        {
            AutomationElement[] children = UiQuery.Read(() => scope.FindAllChildren(), Uia.Wait.DefaultTimeout, "пункты меню");
            return children.Where(c => UiQuery.Read(() => c.Properties.ControlType.Value, Uia.Wait.DefaultTimeout, "тип пункта меню") == ControlType.MenuItem).ToList();
        }

        private static bool IsExpanded(IExpandCollapsePattern pattern, string what)
        {
            ExpandCollapseState state = UiQuery.Read(() => pattern.ExpandCollapseState.Value, Uia.Wait.DefaultTimeout, "раскрытие " + what);
            return state == ExpandCollapseState.Expanded || state == ExpandCollapseState.PartiallyExpanded;
        }

        private static PilotMenuItem Describe(AutomationElement item)
        {
            bool submenu = UiQuery.Read(() => item.Patterns.ExpandCollapse.IsSupported, Uia.Wait.DefaultTimeout, "ExpandCollapse");
            ToggleState? toggle = UiQuery.Read(() => item.Patterns.Toggle.PatternOrDefault?.ToggleState.Value, Uia.Wait.DefaultTimeout, "Toggle пункта меню");
            bool? isChecked = toggle == null ? (bool?)null : toggle == ToggleState.On;
            if (isChecked == null)
            {
                string status = UiQuery.Read(() => item.Properties.ItemStatus.ValueOrDefault ?? "", Uia.Wait.DefaultTimeout, "ItemStatus пункта меню");
                if (status.Contains("IsChecked:"))
                {
                    isChecked = PilotStatus.Parse(status).GetBool("IsChecked");
                }
            }
            return new PilotMenuItem(item.SafeAutomationId(), Caption(item), item.SafeIsEnabled(), submenu, isChecked);
        }

        internal static string Caption(AutomationElement item)
        {
            string name = item.SafeName();
            if (name.Length > 0)
            {
                return name;
            }
            // подпись из Command.Caption: Name пустой, текст со знаком ускорителя - в дочернем TextBlock
            string text = item.SafeChildren().Where(c => c.SafeControlType() == ControlType.Text).Select(c => c.SafeName()).FirstOrDefault(t => t.Length > 0) ?? "";
            return PilotStatus.StripAccessKey(text);
        }

        private static Tuple<AutomationElement, AutomationElement> FindOpen(PilotApp app, By menu)
        {
            foreach (AutomationElement popup in app.Windows.Popups())
            {
                AutomationElement found = UiQuery.TryFind(popup, menu, Uia.Wait.DefaultTimeout, 3);
                if (found != null)
                {
                    return Tuple.Create(popup, found);
                }
            }
            return null;
        }

        private static PilotMenu Create(PilotApp app, AutomationElement popup, AutomationElement menu, string description)
        {
            IntPtr handle = Handle(popup);
            PilotLog.Action("открыто меню " + description);
            return new PilotMenu(app, () => Native.IsShown(handle) ? menu : null, description, handle);
        }

        private static IntPtr Handle(AutomationElement window)
        {
            return UiQuery.Read(() => window.Properties.NativeWindowHandle.ValueOrDefault, Uia.Wait.DefaultTimeout, "HWND всплывающего окна");
        }

        private static string DescribePopups(PilotApp app)
        {
            IReadOnlyList<AutomationElement> popups = app.Windows.Popups();
            return popups.Count == 0 ? "всплывающих окон нет" : "всплывающие окна: " + string.Join("; ", popups.Select(p => UiQuery.TryFind(p, By.Type(ControlType.Menu), Uia.Wait.DefaultTimeout, 3).Describe()));
        }
    }
}
