using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FlaUI.Core.AutomationElements;
using PilotUITesting.Uia;

namespace PilotUITesting.Client
{
    /// <summary>Кнопка панели и её состояние из ItemStatus.</summary>
    public sealed class PilotToolbarButton
    {
        internal PilotToolbarButton(string id, string caption, bool isEnabled, bool isHidden, bool? isChecked)
        {
            Id = id;
            Caption = caption;
            IsEnabled = isEnabled;
            IsHidden = isHidden;
            IsChecked = isChecked;
        }

        public string Id { get; }

        /// <summary>Подпись: Name, иначе подсказка (HelpText) без знака ускорителя.</summary>
        public string Caption { get; }

        public bool IsEnabled { get; }

        /// <summary>Кнопка не поместилась на панель и ушла в меню «…» (PART_MoreButton).</summary>
        public bool IsHidden { get; }

        /// <summary>Состояние переключателя; null - не переключатель.</summary>
        public bool? IsChecked { get; }

        public override string ToString()
        {
            return $"{Id} «{Caption}»{(IsEnabled ? "" : " недоступна")}{(IsHidden ? " скрыта" : "")}{(IsChecked == null ? "" : IsChecked.Value ? " вкл" : " выкл")}";
        }
    }

    /// <summary>
    /// Панель команд Ascon.Pilot.Theme.Controls.ToolbarStripe - и самого Pilot (objectListViewerToolbar, taskViewerToolbar, AUIDRenderToolbar,
    /// annotationsToolbar), и модулей: AutomationId кнопки = имя, переданное в построитель (IToolbarBuilder.AddButtonItem(name) / Name элемента);
    /// меню кнопки - ContextMenu «&lt;id&gt;ContextMenu» во всплывающем окне; состояние - ItemStatus «IsHidden:…; IsChecked:…;».
    /// Кнопки, не поместившиеся на панель, открываются через «…» (PART_MoreButton -> moreButtonMenu, тот же id пункта).
    /// </summary>
    public sealed class PilotToolbar
    {
        private readonly PilotApp _app;
        private readonly Func<AutomationElement> _root;
        private readonly string _description;

        internal PilotToolbar(PilotApp app, Func<AutomationElement> root, string description)
        {
            _app = app;
            _root = root;
            _description = description;
        }

        public AutomationElement Root => _root() ?? throw new ElementNotFoundException(_description, "панель", TimeSpan.Zero, TimeSpan.Zero, null, null);

        /// <summary>Кнопки панели (прямые потомки с AutomationId и ItemStatus), кроме «…».</summary>
        public IReadOnlyList<PilotToolbarButton> Buttons()
        {
            UiQueryResult faults = new UiQueryResult();
            List<PilotToolbarButton> result = new List<PilotToolbarButton>();
            foreach (AutomationElement child in UiQuery.Children(Root, faults))
            {
                string id = UiQuery.Read(() => child.Properties.AutomationId.ValueOrDefault ?? "", Wait.DefaultTimeout, "AutomationId кнопки " + _description);
                string status = UiQuery.Read(() => child.Properties.ItemStatus.ValueOrDefault ?? "", Wait.DefaultTimeout, $"ItemStatus кнопки {id} ({_description})");
                // «IsHidden:» в ItemStatus задают только шаблоны элементов ToolbarStripe (у PART_MoreButton и прочих частей шаблона его нет)
                if (id.Length == 0 || id == PilotIds.ToolbarStripe.MoreButton || !status.Contains("IsHidden:"))
                {
                    continue;
                }
                result.Add(Describe(child, id, status));
            }
            if (!faults.Complete)
            {
                throw new ClientBusyException("кнопки " + _description, Wait.DefaultTimeout, TimeSpan.Zero, faults.BusyFaults, faults.LastFault);
            }
            return result;
        }

        public bool Has(string id)
        {
            return TryButton(id) != null;
        }

        /// <summary>Доступность кнопки; сбой чтения - исключение (занят - <see cref="ClientBusyException"/>), а не «недоступна».</summary>
        public bool IsEnabled(string id)
        {
            return ReadButton(id, b => b.Properties.IsEnabled.Value, "доступность");
        }

        public bool IsHidden(string id)
        {
            return PilotStatus.ToolbarItem(ReadButton(id, b => b.Properties.ItemStatus.ValueOrDefault ?? "", "ItemStatus")).IsHidden;
        }

        /// <summary>Состояние переключателя (ItemStatus «IsChecked:…»); null - не переключатель.</summary>
        public bool? IsChecked(string id)
        {
            return PilotStatus.ToolbarItem(ReadButton(id, b => b.Properties.ItemStatus.ValueOrDefault ?? "", "ItemStatus")).IsChecked;
        }

        /// <summary>Нажать кнопку: дождаться, что она есть и доступна (панель модуля бывает недоступна до загрузки данных); скрытую - через «…».</summary>
        public void Click(string id, TimeSpan? timeout = null)
        {
            TimeSpan limit = timeout ?? Wait.DefaultTimeout;
            (AutomationElement button, string status) = WaitEnabled(id, limit);
            if (PilotStatus.ToolbarItem(status).IsHidden)
            {
                PilotLog.Action($"кнопка {id} скрыта при переполнении панели - через «…»");
                PilotMenu more = OpenMoreMenu(limit);
                PilotMenuItem entry = more.Items().FirstOrDefault(i => i.Id == id);
                if (entry != null && entry.HasSubmenu)
                {
                    // кнопка-меню в «…» - заголовок подменю (ToolbarContextMenuItemStyle: Menu -> ItemsSource), нажать её нечем
                    more.Close();
                    throw new InvalidOperationException($"Кнопка {id} ({_description}) - кнопка-меню, скрытая в «…»: её пункты открывает OpenMenu/ClickMenuItem.");
                }
                more.Click(id);
                return;
            }
            UiAct.Press(button, $"кнопка {id} ({_description})");
        }

        /// <summary>Установить переключатель; постусловие - ItemStatus «IsChecked:value».</summary>
        public void SetChecked(string id, bool value)
        {
            bool? current = IsChecked(id);
            if (current == null)
            {
                throw new InvalidOperationException($"Кнопка {id} ({_description}) не переключатель: в ItemStatus нет IsChecked.");
            }
            if (current == value)
            {
                return;
            }
            Click(id);
            Wait.Strict(() => IsChecked(id) == value, Wait.DefaultTimeout, $"переключатель {id} = {value}", () => Button(id).SafeItemStatus());
        }

        /// <summary>Нажать кнопку-меню и дождаться её меню («&lt;id&gt;ContextMenu»).</summary>
        public PilotMenu OpenMenu(string id)
        {
            return OpenMenuOf(id, PilotIds.ToolbarStripe.MenuOf(id));
        }

        /// <summary>Нажать кнопку-меню и пункт по пути id (подменю раскрываются).</summary>
        public void ClickMenuItem(string id, params string[] path)
        {
            OpenMenu(id).Click(path);
        }

        public override string ToString()
        {
            return _description;
        }

        /// <summary>
        /// Меню кнопки: видимая кнопка - нажать и дождаться «&lt;id&gt;ContextMenu»; кнопка, ушедшая при переполнении в «…», - там она
        /// заголовок подменю (ToolbarStripe: ToolbarContextMenuItemStyle), меню кнопки - это подменю.
        /// </summary>
        private PilotMenu OpenMenuOf(string buttonId, string menuId)
        {
            (AutomationElement button, string status) = WaitEnabled(buttonId, Wait.DefaultTimeout);
            if (PilotStatus.ToolbarItem(status).IsHidden)
            {
                PilotLog.Action($"кнопка-меню {buttonId} скрыта при переполнении панели - её меню открывается подменю «…»");
                return OpenMoreMenu(Wait.DefaultTimeout).Submenu(buttonId);
            }
            UiAct.Press(button, $"кнопка {buttonId} ({_description})");
            return PilotMenu.Wait(_app, menuId);
        }

        /// <summary>«…» (PART_MoreButton) - часть шаблона панели, а не её элемент: ItemStatus у неё пуст, нажимается без разбора состояния.</summary>
        private PilotMenu OpenMoreMenu(TimeSpan limit)
        {
            (AutomationElement more, _) = WaitEnabled(PilotIds.ToolbarStripe.MoreButton, limit);
            UiAct.Press(more, $"кнопка «…» ({_description})");
            return PilotMenu.Wait(_app, PilotIds.ToolbarStripe.MoreMenu);
        }

        private AutomationElement TryButton(string id)
        {
            AutomationElement root = _root();
            return root == null ? null : UiQuery.TryFind(root, By.Id(id), Wait.DefaultTimeout, 2);
        }

        private AutomationElement Button(string id)
        {
            return TryButton(id) ?? throw new ElementNotFoundException(_description, "кнопка " + id, TimeSpan.Zero, TimeSpan.Zero, Buttons().Select(b => b.Id).ToList(), null);
        }

        private AutomationElement WaitButton(string id, TimeSpan? timeout)
        {
            return UiQuery.WaitFind(_root, By.Id(id), timeout ?? Wait.DefaultTimeout, _description, 2);
        }

        /// <summary>
        /// Дождаться доступной кнопки и прочитать её ItemStatus. Кнопка ищется заново на каждом опросе: модуль пересоздаёт
        /// элементы панели после загрузки данных, и найденный заранее элемент исчезает.
        /// </summary>
        private (AutomationElement Button, string Status) WaitEnabled(string id, TimeSpan timeout)
        {
            WaitButton(id, timeout);
            Tuple<AutomationElement, string> found = Wait.ForStrict(() =>
            {
                AutomationElement button = TryButton(id);
                if (button == null || !button.Properties.IsEnabled.Value)
                {
                    return null;
                }
                return Tuple.Create(button, button.Properties.ItemStatus.ValueOrDefault ?? "");
            }, timeout, $"кнопка {id} доступна ({_description})", () => Observe(id));
            return (found.Item1, found.Item2);
        }

        private string Observe(string id)
        {
            AutomationElement button = TryButton(id);
            return button == null ? "кнопки нет" : $"IsEnabled = {button.SafeIsEnabled()}, ItemStatus «{button.SafeItemStatus()}»";
        }

        /// <summary>Прочитать свойство кнопки: «занят» - повтор чтения, кнопку пересоздали между поиском и чтением - поиск заново.</summary>
        private T ReadButton<T>(string id, Func<AutomationElement, T> read, string what)
        {
            Stopwatch sw = Stopwatch.StartNew();
            while (true)
            {
                AutomationElement button = Button(id);
                try
                {
                    return UiQuery.Read(() => read(button), Wait.DefaultTimeout, $"{what} кнопки {id} ({_description})");
                }
                catch (Exception ex) when (UiFault.Classify(ex) == UiFaultKind.Gone && sw.Elapsed < Wait.DefaultTimeout)
                {
                    PilotLog.Action($"кнопка {id} ({_description}) пересоздана во время чтения ({ex.GetType().Name}) - поиск заново");
                }
            }
        }

        private PilotToolbarButton Describe(AutomationElement button, string id, string status)
        {
            (bool hidden, bool? isChecked) = PilotStatus.ToolbarItem(status);
            string caption = UiQuery.Read(() => button.Properties.Name.ValueOrDefault ?? "", Wait.DefaultTimeout, $"Name кнопки {id} ({_description})");
            if (caption.Length == 0)
            {
                caption = PilotStatus.StripAccessKey(UiQuery.Read(() => button.Properties.HelpText.ValueOrDefault ?? "", Wait.DefaultTimeout, $"HelpText кнопки {id} ({_description})"));
            }
            bool enabled = UiQuery.Read(() => button.Properties.IsEnabled.Value, Wait.DefaultTimeout, $"доступность кнопки {id} ({_description})");
            return new PilotToolbarButton(id, caption, enabled, hidden, isChecked);
        }
    }
}
