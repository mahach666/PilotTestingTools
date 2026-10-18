using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using PilotUITesting.Uia;
using Wait = PilotUITesting.Uia.Wait;

namespace PilotUITesting.Client
{
    /// <summary>
    /// Вкладка клиента (AUIDTabItemLayoutDocument). Паттерна SelectionItem у вкладок нет: активность и тип содержимого читаются из
    /// HelpText заголовка «IsActive:True; Content:AUIDDocumentsExplorer». Вкладки представлений модулей-HwndHost
    /// в UI Automation заголовка не имеют - для них <see cref="PilotModuleView"/>.
    /// </summary>
    public sealed class PilotTab
    {
        private readonly PilotApp _app;
        private readonly string _runtimeId;
        private string _lastKnownTitle;

        internal PilotTab(PilotApp app, AutomationElement header)
        {
            _app = app;
            Header = header;
            _runtimeId = RuntimeKey(header);
            _lastKnownTitle = ReadTitle(header);
        }

        internal PilotApp App => _app;

        /// <summary>Элемент заголовка вкладки (AUIDTabItemLayoutDocument).</summary>
        public AutomationElement Header { get; }

        /// <summary>Заголовок вкладки (DocumentTitleTextBlock); у проводника меняется вместе с выбранным объектом.</summary>
        public string Title
        {
            get
            {
                string title = ReadTitle(Header);
                if (title.Length > 0)
                {
                    _lastKnownTitle = title;
                }
                return title;
            }
        }

        /// <summary>Последний прочитанный заголовок (нужен после перезапуска клиента, когда элемента уже нет).</summary>
        public string LastKnownTitle => _lastKnownTitle;

        /// <summary>Тип содержимого: AUIDNewTab, AUIDDocumentsExplorer, AUIDNextTasksTab … (HelpText «Content:…»).</summary>
        public string ContentId => HeaderStatus().ContentId;

        /// <summary>Вкладка активна (HelpText «IsActive:True»).</summary>
        public bool IsActive => HeaderStatus().IsActive;

        /// <summary>Вкладка открыта: её заголовок есть среди вкладок (по RuntimeId). Клиент занят - <see cref="ClientBusyException"/>, а не false.</summary>
        public bool IsOpen => _app.TabHeaders().Any(h => RuntimeKey(h) == _runtimeId);

        /// <summary>Сделать вкладку активной кликом по заголовку (другие способы клиент не принимает); постусловие - IsActive.</summary>
        public PilotTab Activate()
        {
            if (IsActive)
            {
                return this;
            }
            UiAct.Click(Header, "заголовок вкладки «" + _lastKnownTitle + "»", MouseButton.Left, false);
            Uia.Wait.Strict(() => IsActive, Uia.Wait.DefaultTimeout, "вкладка «" + _lastKnownTitle + "» стала активной", null);
            return this;
        }

        /// <summary>Корень содержимого (активирует вкладку): потомок TabView с AutomationId == ContentId, видимый на экране.</summary>
        public AutomationElement Content
        {
            get
            {
                Activate();
                return Uia.Wait.ForStrict(TryContent, Uia.Wait.DefaultTimeout, "содержимое вкладки «" + _lastKnownTitle + "»", null);
            }
        }

        /// <summary>Корень содержимого без активации и ожидания; null - вкладка не активна или содержимого нет.</summary>
        internal AutomationElement TryContent()
        {
            (bool active, string contentId) = HeaderStatus();
            if (!active || string.IsNullOrEmpty(contentId))
            {
                return null;
            }
            AutomationElement tabView = _app.TabViewStrict();
            if (tabView == null)
            {
                return null;
            }
            return UiQuery.FindAllComplete(tabView, By.Id(contentId), Uia.Wait.DefaultTimeout, 1)
                .FirstOrDefault(c => !UiQuery.Read(() => c.Properties.IsOffscreen.ValueOrDefault, Uia.Wait.DefaultTimeout, "IsOffscreen содержимого вкладки"));
        }

        /// <summary>Закрыть кнопкой DocumentCloseButton; не закрылась - исключение.</summary>
        public void Close()
        {
            string title = _lastKnownTitle;
            AutomationElement close = Header.Find(By.Id(PilotIds.Tabs.Close), null, "вкладка «" + title + "»");
            UiAct.Press(close, "закрыть вкладку «" + title + "»");
            Uia.Wait.Strict(() => !IsOpen, Uia.Wait.DefaultTimeout, "закрытие вкладки «" + title + "»", null);
        }

        public override string ToString()
        {
            return $"вкладка «{_lastKnownTitle}»";
        }

        internal bool Same(AutomationElement header)
        {
            return RuntimeKey(header) == _runtimeId;
        }

        private (bool IsActive, string ContentId) HeaderStatus()
        {
            AutomationElement layout = UiQuery.TryFind(Header, By.Id(PilotIds.Tabs.Header), Uia.Wait.DefaultTimeout, 3)
                ?? throw new InvalidOperationException($"У вкладки «{_lastKnownTitle}» нет элемента {PilotIds.Tabs.Header} (вкладка закрыта или дерево UI Automation устарело).");
            string help = UiQuery.Read(() => layout.Properties.HelpText.ValueOrDefault ?? "", Uia.Wait.DefaultTimeout, "HelpText заголовка вкладки");
            return PilotStatus.TabHeader(help);
        }

        private static string ReadTitle(AutomationElement header)
        {
            AutomationElement title = header.FindDescendantBfs(e => e.SafeAutomationId() == PilotApp.TabTitleId, 4);
            return title?.SafeName() ?? header.SafeName();
        }

        internal static string RuntimeKey(AutomationElement element)
        {
            int[] id = UiQuery.Read(() => element.Properties.RuntimeId.ValueOrDefault, Uia.Wait.DefaultTimeout, "RuntimeId");
            return id == null ? "" : string.Join(".", id);
        }
    }

    /// <summary>Вкладки клиента: список, активная, открыть плитку «Новой вкладки», дождаться новой вкладки после действия.</summary>
    public sealed class PilotTabBar
    {
        private readonly PilotApp _app;

        internal PilotTabBar(PilotApp app)
        {
            _app = app;
        }

        /// <summary>Вкладки, видимые в UI Automation (вкладок представлений модулей-HwndHost среди них нет). Порядок - не визуальный.</summary>
        public IReadOnlyList<PilotTab> All()
        {
            return _app.TabHeaders().Select(h => new PilotTab(_app, h)).ToList();
        }

        /// <summary>Активная вкладка; null - активна вкладка модуля-HwndHost или вкладок нет.</summary>
        public PilotTab Active => All().FirstOrDefault(t => t.IsActive);

        /// <summary>Вкладка по условию; больше одной - <see cref="AmbiguousElementException"/>.</summary>
        public PilotTab Find(Func<PilotTab, bool> match)
        {
            List<PilotTab> found = All().Where(match).ToList();
            if (found.Count > 1)
            {
                throw new AmbiguousElementException("вкладка по условию", found.Select(t => t.LastKnownTitle).ToList());
            }
            return found.FirstOrDefault();
        }

        public PilotTab Wait(Func<PilotTab, bool> match, TimeSpan? timeout = null, [CallerArgumentExpression("match")] string description = null)
        {
            return Uia.Wait.ForStrict(() => Find(match), timeout ?? Uia.Wait.DefaultTimeout, "вкладка " + description, DescribeTabs);
        }

        /// <summary>Выполнить действие и вернуть вкладку, которой до этого не было (RuntimeId). Для представлений модулей - <see cref="PilotModuleView.OpenedBy"/>.</summary>
        public PilotTab OpenedBy(Action open, Func<PilotTab, bool> match = null, TimeSpan? timeout = null)
        {
            HashSet<string> before = Snapshot();
            open();
            return Uia.Wait.ForStrict(() =>
            {
                List<PilotTab> fresh = FreshTabs(before).Where(t => match == null || match(t)).ToList();
                if (fresh.Count > 1)
                {
                    throw new AmbiguousElementException("новая вкладка", fresh.Select(t => t.LastKnownTitle).ToList());
                }
                return fresh.FirstOrDefault();
            }, timeout ?? Uia.Wait.DefaultTimeout, "новая вкладка", DescribeTabs);
        }

        /// <summary>
        /// «+» -> плитка с подписью caption на странице «Новая вкладка» (кнопка без AutomationId внутри AUIDCommandButtons) -> вкладка,
        /// которой не было до «+», с содержимым expectedContentId (если задан) или любым, кроме «Новой вкладки».
        /// Несколько плиток с одной подписью - исключение с перечнем.
        /// </summary>
        public PilotTab OpenTile(string caption, string expectedContentId = null, TimeSpan? timeout = null)
        {
            TimeSpan limit = timeout ?? Uia.Wait.DefaultTimeout;
            HashSet<string> before = Snapshot();
            AutomationElement add = Uia.Wait.ForStrict(() => _app.TabStrip?.ChildById(PilotApp.AddTabButtonId), limit, "кнопка «+» (новая вкладка)", null);
            UiAct.Press(add, "«+» (новая вкладка)");
            PilotTab newTab = Uia.Wait.ForStrict(() => FreshTabs(before).FirstOrDefault(t => t.ContentId == PilotIds.TabContent.NewTab), limit, "страница «Новая вкладка»", DescribeTabs);
            AutomationElement tiles = UiQuery.WaitFind(newTab.TryContent, By.Id(PilotIds.NewTabPage.Tiles), limit, "страница «Новая вкладка»");
            By tileSelector = By.Type(ControlType.Button).And(By.Name(caption));
            AutomationElement tile = Uia.Wait.ForStrict(() => tiles.FindAll(tileSelector).FirstOrDefault(), limit, $"плитка «{caption}»", null);
            IReadOnlyList<AutomationElement> same = tiles.FindAll(tileSelector);
            if (same.Count > 1)
            {
                throw new AmbiguousElementException($"плитка «{caption}» на странице «Новая вкладка»", same.Select(t => t.Describe()).ToList());
            }
            UiAct.Press(tile, $"плитка «{caption}»");
            PilotTab opened = Uia.Wait.ForStrict(() => FreshTabs(before).FirstOrDefault(t =>
                expectedContentId != null ? t.ContentId == expectedContentId : t.ContentId != PilotIds.TabContent.NewTab), limit,
                $"вкладка из плитки «{caption}»" + (expectedContentId != null ? " с содержимым " + expectedContentId : ""), DescribeTabs);
            if (!opened.Same(newTab.Header) && newTab.IsOpen && newTab.ContentId == PilotIds.TabContent.NewTab)
            {
                PilotLog.Action("плитка открыла отдельную вкладку - закрываю оставшуюся «Новую вкладку»");
                newTab.Close();
            }
            return opened;
        }

        /// <summary>Закрыть вкладки по условию; возвращает число закрытых (каждое закрытие проверяется).</summary>
        public int CloseWhere(Func<PilotTab, bool> match)
        {
            int closed = 0;
            foreach (PilotTab tab in All().Where(match).ToList())
            {
                tab.Close();
                closed++;
            }
            return closed;
        }

        /// <summary>RuntimeId всех вкладок до действия: только по полному перечню (неполный снимок выдал бы старые вкладки за новые).</summary>
        private HashSet<string> Snapshot()
        {
            return Uia.Wait.ForStrict(() => new HashSet<string>(_app.TabHeaders().Select(PilotTab.RuntimeKey)), Uia.Wait.DefaultTimeout, "перечень вкладок", null);
        }

        private IEnumerable<PilotTab> FreshTabs(HashSet<string> before)
        {
            return _app.TabHeaders().Where(h => !before.Contains(PilotTab.RuntimeKey(h))).Select(h => new PilotTab(_app, h)).ToList();
        }

        private string DescribeTabs()
        {
            return "вкладки: " + string.Join("; ", All().Select(t => t.LastKnownTitle));
        }
    }
}
