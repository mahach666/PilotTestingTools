using System;
using System.Collections.Generic;
using System.Linq;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.WindowsAPI;
using PilotUITesting.Uia;

namespace PilotUITesting.Client
{
    /// <summary>
    /// Содержимое обычной вкладки клиента (корень - потомок TabView с AutomationId = тип содержимого).
    /// Поиск идёт только внутри корня активной вкладки: в главном окне остаются устаревшие копии содержимого других вкладок.
    /// </summary>
    public class PilotTabPage : PilotPage
    {
        public PilotTabPage(PilotTab tab)
            : base(AppOf(tab), "вкладка «" + tab.LastKnownTitle + "»")
        {
            Tab = tab;
        }

        public PilotTab Tab { get; }

        /// <summary>Корень содержимого; вкладка активируется, если не активна.</summary>
        protected override AutomationElement ResolveRoot()
        {
            if (!Tab.IsActive)
            {
                Tab.Activate();
            }
            return Tab.TryContent();
        }

        public void Close()
        {
            Tab.Close();
        }

        private static PilotApp AppOf(PilotTab tab)
        {
            return (tab ?? throw new ArgumentNullException(nameof(tab))).App;
        }
    }

    /// <summary>
    /// Представление модуля, которое клиент показывает прямым потомком главного окна (отдельный HWND, класс - класс представления,
    /// например MyModuleView). Так устроены вкладки модулей, открытые через ITabServiceProvider: заголовка вкладки у них в
    /// UI Automation нет, закрываются они Ctrl+W; видны в дереве, только пока вкладка активна (проверено на реестре замечаний, Pilot-ICE 26.11).
    /// После открытия такого представления дерево UI Automation главного окна может перестать обновляться - см. <see cref="PilotApp.ModuleViewOpened"/>.
    /// </summary>
    public class PilotModuleView : PilotPage
    {
        private readonly string _runtimeId;

        private PilotModuleView(PilotApp app, string viewClassName, string runtimeId)
            : base(app, "представление модуля " + viewClassName)
        {
            ViewClassName = viewClassName;
            _runtimeId = runtimeId;
        }

        /// <summary>Для наследников (page object конкретного модуля).</summary>
        protected PilotModuleView(PilotModuleView opened)
            : this(opened.App, opened.ViewClassName, opened._runtimeId)
        {
        }

        public string ViewClassName { get; }

        /// <summary>
        /// Выполнить open и дождаться НОВОГО (по RuntimeId) потомка главного окна с ClassName == viewClassName.
        /// Отмечает <see cref="PilotApp.ModuleViewOpened"/>. timeout null - Config.DataTimeout (модуль может долго строить представление).
        /// </summary>
        public static PilotModuleView OpenedBy(PilotApp app, string viewClassName, Action open, TimeSpan? timeout = null)
        {
            if (string.IsNullOrEmpty(viewClassName))
            {
                throw new ArgumentException("Нужен класс представления модуля", nameof(viewClassName));
            }
            HashSet<string> before = new HashSet<string>(Views(app, viewClassName).Select(PilotTab.RuntimeKey));
            open();
            app.MarkModuleViewOpened();
            AutomationElement view = Uia.Wait.ForStrict(() => Views(app, viewClassName).FirstOrDefault(v => !before.Contains(PilotTab.RuntimeKey(v))),
                timeout ?? app.Config.DataTimeout, "новое представление модуля " + viewClassName + " (потомок главного окна)", null);
            PilotLog.Action("открыто представление модуля " + viewClassName);
            return new PilotModuleView(app, viewClassName, PilotTab.RuntimeKey(view));
        }

        /// <summary>Представление видно (вкладка модуля активна).</summary>
        public bool IsVisible => ResolveRoot() != null;

        protected override AutomationElement ResolveRoot()
        {
            return Views(App, ViewClassName).FirstOrDefault(v => PilotTab.RuntimeKey(v) == _runtimeId);
        }

        /// <summary>
        /// Закрыть вкладку модуля: фокус в представление -> Ctrl+W (закрывается активная вкладка) -> представление исчезло.
        /// Предусловие - представление видно, иначе Ctrl+W закрыл бы чужую вкладку.
        /// </summary>
        public void Close(TimeSpan? timeout = null)
        {
            AutomationElement root = ResolveRoot() ?? throw new InvalidOperationException($"{Description}: вкладка модуля закрыта или не активна - Ctrl+W закрыл бы другую вкладку, закрытие отменено.");
            UiAct.Keys(root, "закрыть вкладку модуля " + ViewClassName, VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_W);
            Uia.Wait.Strict(() => ResolveRoot() == null, timeout ?? Uia.Wait.DefaultTimeout, "закрытие вкладки модуля " + ViewClassName, null);
        }

        /// <summary>Все представления модуля с классом viewClassName, видимые сейчас (прямые потомки главного окна; у активной вкладки модуля).</summary>
        public static IReadOnlyList<AutomationElement> FindAll(PilotApp app, string viewClassName)
        {
            if (string.IsNullOrEmpty(viewClassName))
            {
                throw new ArgumentException("Нужен класс представления модуля", nameof(viewClassName));
            }
            return Views(app, viewClassName).ToList();
        }

        private static IEnumerable<AutomationElement> Views(PilotApp app, string viewClassName)
        {
            Window main = app.MainWindowStrict();
            UiQueryResult faults = new UiQueryResult();
            AutomationElement[] children = UiQuery.Children(main, faults);
            if (!faults.Complete)
            {
                throw new ClientBusyException("потомки главного окна", Uia.Wait.DefaultTimeout, TimeSpan.Zero, faults.BusyFaults, faults.LastFault);
            }
            return children.Where(c => c.SafeClassName() == viewClassName).ToList();
        }
    }
}
