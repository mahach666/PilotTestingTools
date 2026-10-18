namespace PilotUITesting.Client
{
    public static partial class PilotIds
    {
        /// <summary>Полоса вкладок AvalonDock (theme.xaml; DocumentPaneTabPanel.cs:21-24). Порядок вкладок в UIA не визуальный, SelectionItem нет; активная вкладка - по HelpText Header «IsActive:True» [Р] разведка. У вкладок модулей заголовка в UIA нет [Р] разведка.</summary>
        public static class Tabs
        {
            /// <summary>Name пуст (Custom) [Р] разведка; Ascon.Pilot.Pilot.MainView.xaml:153.</summary>
            public const string Container = "tabHostView";

            /// <summary>(Custom) [Р] разведка; DocumentPaneTabPanel.cs:23. Name = заголовок вкладки.</summary>
            public const string Item = "AUIDTabItemLayoutDocument";

            /// <summary>(Custom) [Р] разведка; theme.xaml:861. HelpText «IsActive:{bool}; Content:{id содержимого}».</summary>
            public const string Header = "AUIDTabLayoutDocument";

            /// <summary>Name пуст (Image) [Р] разведка; theme.xaml:663.</summary>
            public const string Icon = "tabIcon";

            /// <summary>(Text) [Р] разведка; theme.xaml:664.</summary>
            public const string Title = "DocumentTitleTextBlock";

            /// <summary>Name пуст (Text) [Р] разведка; theme.xaml:672.</summary>
            public const string Version = "DocumentVersionTextBlock";

            /// <summary>Name пуст (Button) [Р] разведка; theme.xaml:877; показ: вкладку можно закрыть (CanClose). У вкладки модуля недоступна - закрывать Ctrl+W (разведка).</summary>
            [ShownWhen("вкладку можно закрыть (CanClose)")]
            public const string Close = "DocumentCloseButton";

            /// <summary>Name пуст (Button) [Р] разведка; theme.xaml:208. HelpText «Новая вкладка».</summary>
            public const string Add = "AddTabButton";

            /// <summary>Name пуст (Button) [Р] разведка; theme.xaml:211. Toggle через UIA ставит IsChecked, но список не открывает (разведка).</summary>
            public const string TabList = "MenuDropDownButton";

            /// <summary>«Закрыть другие вкладки» (MenuItem) [К] theme.xaml:768; показ: открыто меню заголовка вкладки.</summary>
            [ShownWhen("открыто меню заголовка вкладки")]
            public const string CloseAllButThis = "AUIDAvalonTabCloseAllButThisMenuItem";
        }

        /// <summary>Корень содержимого вкладки: id = Name = ContentViewModel.ToString() (TabView.xaml:38). Содержимое вкладки модуля (ITabServiceProvider.OpenTabPage) - прямой потомок mainPilotView с ClassName представления, без id [Р] разведка.</summary>
        public static class TabContent
        {
            /// <summary>(Custom) [Р] разведка; NewTabViewModel.cs:79.</summary>
            public const string NewTab = "AUIDNewTab";

            /// <summary>(Custom) [Р] разведка; DocumentExplorerViewModel.cs:1211.</summary>
            public const string DocumentsExplorer = "AUIDDocumentsExplorer";

            /// <summary>(Custom) [Р] разведка; NextTasksTabViewModel.cs:109. Внутри ещё один элемент с тем же id (NextTaskViewer.xaml:2).</summary>
            public const string Tasks = "AUIDNextTasksTab";

            /// <summary>(Custom) [Д] дамп прогона; ReportExplorerViewModel.cs:117.</summary>
            public const string Reports = "AUIDReportExplorer";

            /// <summary>(Custom) [К] ChatsViewerViewModel.cs:118.</summary>
            public const string Chats = "AUIDChatsTab";

            /// <summary>(Custom) [К] WorkingDaysTabViewModel.cs:190.</summary>
            public const string WorkingDaysCalendar = "AUIDWorkingDaysCalendarTab";

            /// <summary>(Custom) [К] XpsMergerTabViewModel.cs:124.</summary>
            public const string XpsMerger = "AUIDXpsMergerTab";
        }

        /// <summary>Страница «Новая вкладка» (NewTabDashboardView.xaml). У плиток НЕТ id: искать Button внутри Tiles по Name (PilotTitles.NewTab) [Р] разведка. Плитка «Задания» каждый раз открывает новую вкладку.</summary>
        public static class NewTabPage
        {
            /// <summary>(Custom) [Р] разведка; NewTabViewModel.cs:79.</summary>
            public const string Container = "AUIDNewTab";

            /// <summary>Name пуст (Pane) [Р] разведка; Ascon.Pilot.Client.NewTabDashboardView.xaml:124.</summary>
            public const string Tiles = "AUIDCommandButtons";

            /// <summary>(ListItem) [Р] разведка; Ascon.Pilot.Client.NewTabDashboardView.xaml:106; показ: есть недавние документы, ширина страницы >= 700. У всех строк один id; Name = заголовок.</summary>
            [ShownWhen("есть недавние документы, ширина страницы >= 700")]
            public const string RecentDocument = "AUIDDashboardDocumentItem";

            /// <summary>(Text) [Р] разведка; listviewobjectnodetemplate.xaml:326; показ: есть недавние документы, ширина страницы >= 700.</summary>
            [ShownWhen("есть недавние документы, ширина страницы >= 700")]
            public const string RecentDocumentTitle = "AUIDRecentDocumentNodeTitle";

            /// <summary>(ListItem) [Р] разведка; Ascon.Pilot.Client.NewTabDashboardView.xaml:118; показ: есть недавние задания, ширина страницы >= 700.</summary>
            [ShownWhen("есть недавние задания, ширина страницы >= 700")]
            public const string RecentTask = "AUIDDashboardTaskItem";

            /// <summary>(Text) [Р] разведка; listviewobjectnodetemplate.xaml:342; показ: есть недавние задания, ширина страницы >= 700.</summary>
            [ShownWhen("есть недавние задания, ширина страницы >= 700")]
            public const string RecentTaskTitle = "AUIDRecentTaskNodeTitle";
        }
    }
}
