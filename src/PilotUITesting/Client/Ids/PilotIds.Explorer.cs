namespace PilotUITesting.Client
{
    public static partial class PilotIds
    {
        /// <summary>Вкладка «Проекты и документы» (DocumentExplorerView.xaml, ObjectListViewer.xaml). Состав меню и панелей зависит от выделения - см. [ShownWhen].</summary>
        public static class Explorer
        {
            /// <summary>(Custom) [Р] разведка; DocumentExplorerViewModel.cs:1211.</summary>
            public const string Container = "AUIDDocumentsExplorer";

            /// <summary>Name пуст (Custom) [Р] разведка; Ascon.Pilot.Client.DocumentExplorerView.xaml:40. ObjectListViewer: панель, строка навигации, дерево и список.</summary>
            public const string ListViewer = "ascListView";

            /// <summary>Name пуст (Thumb) [Р] разведка; Ascon.Pilot.Client.Controls.ObjectListViewer.xaml:127.</summary>
            public const string TreeSplitter = "AUIDTreeViewSplitter";

            /// <summary>Name пуст (Custom) [Д] дамп прогона; Ascon.Pilot.Client.DocumentExplorerView.xaml:125; показ: выбран объект без просмотра (карточка справа). Поля - Card.*; справа показывается первое непустое: карточка, SdkDetailsView, просмотр (DocumentExplorerView.xaml:130-138).</summary>
            [ShownWhen("выбран объект без просмотра (карточка справа)")]
            public const string ObjectCard = "ObjectCardView";

            /// <summary>Name пуст (Custom) [Р] разведка; Ascon.Pilot.Client.DocumentExplorerView.xaml:135; показ: выбран документ с просмотром. Внутри Viewer.Container.</summary>
            [ShownWhen("выбран документ с просмотром")]
            public const string Preview = "documentPreview";

            /// <summary>(Tab) [Р] разведка; Ascon.Pilot.Client.DocumentExplorerView.xaml:142; показ: DocumentExplorerView.xaml:58-60 (IsContextSelected -> Hidden). HelpText «IsExpanded:{bool}»; Name - отладочная строка, не использовать; вкладки - по Name (PilotTitles.Explorer).</summary>
            [ShownWhen("выбрана строка списка (иначе правая панель скрыта)")]
            public const string BottomTabs = "filesAndTasksTabControl";

            /// <summary>Name пуст (Thumb) [Р] разведка; Ascon.Pilot.Client.DocumentExplorerView.xaml:141; показ: DocumentExplorerView.xaml:58-60 (IsContextSelected -> Hidden), нижняя панель развёрнута (:141).</summary>
            [ShownWhen("выбрана строка списка, нижняя панель развёрнута")]
            public const string BottomTabsSplitter = "filesAndTasksTabControlSplitter";

            /// <summary>Name пуст (Thumb) [Р] разведка; Ascon.Pilot.Client.DocumentExplorerView.xaml:42.</summary>
            public const string Splitter = "documentExplorerSplitter";

            /// <summary>Name пуст (Button) [Р] разведка; Ascon.Pilot.Client.DocumentExplorerView.xaml:65; показ: DocumentExplorerView.xaml:58-60 (IsContextSelected -> Hidden). HelpText «Скрыть навигацию, Ctrl+Q».</summary>
            [ShownWhen("выбрана строка списка (иначе правая панель скрыта)")]
            public const string ShowHideNavigation = "showHideButton";

            /// <summary>«Создать -> тип» в меню кнопки miCreate и в подменю списка: "miCreate" + XamlName(имя типа). [К] ObjectListToolbarItemsProvider.cs:554, MenuItemsProvider.cs:102.</summary>
            public static string CreateItem(string typeName) => "miCreate" + XamlName(typeName);

            /// <summary>Тип задания/процесса в подменю «Вложить в задание/процесс»: "miAttachTo" + XamlName(имя типа). [Д] дамп прогона (miAttachTotaskwarningSign); [К] MenuItemsProvider.cs:230,242, ObjectListToolbarItemsProvider.cs:205,221.</summary>
            public static string AttachToItem(string taskOrWorkflowTypeName) => "miAttachTo" + XamlName(taskOrWorkflowTypeName);

            /// <summary>Шаблон задания/процесса в контекстном меню: "miTemplate" + XamlName(имя шаблона). Те же id в меню заданий. [К] TemplateFiller.cs:133.</summary>
            public static string TemplateMenuItem(string templateName) => "miTemplate" + XamlName(templateName);

            /// <summary>Шаблон задания/процесса в меню кнопки панели: "tbiTemplate" + XamlName(имя шаблона). [К] TemplateFiller.cs:107.</summary>
            public static string TemplateToolbarItem(string templateName) => "tbiTemplate" + XamlName(templateName);

            /// <summary>Папка шаблонов в контекстном меню: "miTemplateFolder" + XamlName(имя папки). [К] TemplateFiller.cs:123.</summary>
            public static string TemplateFolderMenuItem(string folderName) => "miTemplateFolder" + XamlName(folderName);

            /// <summary>Папка шаблонов в меню кнопки панели: "tbiTemplateFolder" + XamlName(имя папки). [К] TemplateFiller.cs:94.</summary>
            public static string TemplateFolderToolbarItem(string folderName) => "tbiTemplateFolder" + XamlName(folderName);

            /// <summary>Отчёт в подменю «Построить отчёт» списка: "miBuildReportName" + XamlName(имя отчёта). [К] MenuItemsProvider.cs:278.</summary>
            public static string BuildReportMenuItem(string reportName) => "miBuildReportName" + XamlName(reportName);

            /// <summary>Отчёт в меню кнопки tbiBuildReportMenu: "tbiBuildReportName" + XamlName(имя отчёта). [К] ObjectListToolbarItemsProvider.cs:261.</summary>
            public static string BuildReportToolbarItem(string reportName) => "tbiBuildReportName" + XamlName(reportName);

            /// <summary>Фильтр проводника в меню tbiDocumentExplorerFilter: "mi_" + XamlName(подпись фильтра). [К] ObjectListToolbarItemsProvider.cs:132.</summary>
            public static string FilterItem(string filterCaption) => "mi_" + XamlName(filterCaption);

            /// <summary>Состояние в меню tbiAssignStateToVersion: "miAssignStateToVersionStateTitle_" + XamlName(заголовок состояния). [К] ObjectListToolbarItemsProvider.cs:484.</summary>
            public static string AssignStateToVersionItem(string stateTitle) => "miAssignStateToVersionStateTitle_" + XamlName(stateTitle);

            /// <summary>Кнопка-меню «Сортировка» панели: "tbiSort" + текущее направление (ListSortDirection.ToString()). [Р] разведка; [К] ObjectListToolbarItemsProvider.cs:92.</summary>
            public static string SortToolbarButton(bool descending) => "tbiSort" + (descending ? "Descending" : "Ascending");

            /// <summary>Подменю «Сортировка» контекстного меню: "miSort" + текущее направление. [К] MenuItemsProvider.cs:122,158.</summary>
            public static string SortMenuItem(bool descending) => "miSort" + (descending ? "Descending" : "Ascending");

            /// <summary>Версия в строке списка (0 - первая): "AUIDVersionItem:{i};". [К] listviewobjectnodetemplate.xaml:214.</summary>
            public static string VersionItem(int index) => "AUIDVersionItem:" + index + ";";

            /// <summary>Список внутри ObjectListViewer с заданным Name: имя + ".ListView" (ascListView.ListView в проводнике, objectListViewer.ListView в выборе объекта, reportListViewer.ListView в отчётах). [Р] разведка; [К] Ascon.Pilot.Client.Controls.ObjectsList.xaml:18.</summary>
            public static string ListViewOf(string objectListViewerName) => objectListViewerName + ".ListView";

            /// <summary>Строка навигации над списком (ObjectListViewer.xaml:35-90; NavigationControl.xaml). Общего элемента с id нет: кнопки истории и путь - внутри Bar (AUIDNavBar), переключатель и поле пути - рядом с ним, прямые потомки ListViewer [Р] разведка.</summary>
            public static class Navigation
            {
                /// <summary>Name пуст (Custom) [Р] разведка; Ascon.Pilot.Client.Controls.ObjectListViewer.xaml:43.</summary>
                public const string Bar = "AUIDNavBar";

                /// <summary>Name пуст (Button) [Р] разведка; Ascon.Pilot.Client.Controls.NavigationControl.xaml:50. Alt+Left.</summary>
                public const string Back = "btnBack";

                /// <summary>Name пуст (Button) [Р] разведка; Ascon.Pilot.Client.Controls.NavigationControl.xaml:51. Alt+Right.</summary>
                public const string Next = "btnNext";

                /// <summary>Name пуст (Button) [Р] разведка; Ascon.Pilot.Client.Controls.NavigationControl.xaml:52. Toggle.</summary>
                public const string History = "btnSearchHistory";

                /// <summary>Name пуст (Button) [Р] разведка; Ascon.Pilot.Client.Controls.NavigationControl.xaml:91. Недоступна, когда список уже в «Начале»: Invoke даёт ElementNotEnabledException (разведка).</summary>
                public const string Home = "btnHome";

                /// <summary>Name пуст (Button) [Р] разведка; Ascon.Pilot.Client.Controls.NavigationControl.xaml:124. HelpText = заголовок уровня; id у всех уровней один.</summary>
                public const string Item = "navigationItemButton";

                /// <summary>Name пуст (Button) [Р] разведка; Ascon.Pilot.Client.Controls.NavigationControl.xaml:125; показ: NavigationControl.xaml:125 (Children, NavigationPopupVisibilityConverter); в 004:100-118 нет. HelpText = заголовок уровня.</summary>
                [ShownWhen("у уровня пути есть дочерние")]
                public const string ShowChildItems = "showChildItemsButton";

                /// <summary>(Button) [Р] разведка; Ascon.Pilot.Client.Controls.NavigationControl.xaml:98; показ: NavigationControl.xaml:98 (HiddenItemsToVisiblityConverter); в 004:100-118 нет. «...» - скрытые уровни пути.</summary>
                [ShownWhen("путь не помещается (есть скрытые уровни)")]
                public const string ShowHiddenItems = "showHiddenItemsButton";

                /// <summary>[К] Ascon.Pilot.Client.Controls.NavigationControl.xaml:104; показ: открыт список showHiddenItemsButton. Опечатка клиента в id («hiddent»).</summary>
                [ShownWhen("открыт список showHiddenItemsButton")]
                public const string HiddenItem = "hiddentNavigationPopupItem";

                /// <summary>[К] Ascon.Pilot.Client.Controls.NavigationControl.xaml:80; показ: открыт список btnSearchHistory.</summary>
                [ShownWhen("открыт список btnSearchHistory")]
                public const string HistoryItem = "searchHistoryPopupItem";

                /// <summary>(Menu) [К] Ascon.Pilot.Client.Controls.NavigationControl.xaml:110; показ: открыто меню уровня пути.</summary>
                [ShownWhen("открыто меню уровня пути")]
                public const string ItemsMenu = "AUIDNavigationItemsContextMenu";

                /// <summary>(Menu) [К] Ascon.Pilot.Client.Controls.NavigationControl.xaml:101; показ: открыто меню скрытых уровней.</summary>
                [ShownWhen("открыто меню скрытых уровней")]
                public const string HiddenItemsMenu = "AUIDNavigationHiddenItemsContextMenu";

                /// <summary>(Menu) [К] Ascon.Pilot.Client.Controls.NavigationControl.xaml:131; показ: открыто меню пункта списка уровней.</summary>
                [ShownWhen("открыто меню пункта списка уровней")]
                public const string PopupItemsMenu = "AUIDNavigationPopupItemsContextMenu";

                /// <summary>Name пуст (Button) [Р] разведка; Ascon.Pilot.Client.Controls.ObjectListViewer.xaml:64. Toggle (не Invoke); доступность = узел поддерживает путь строкой (разведка).</summary>
                public const string ShowPathAsString = "showPathAsStringButton";

                /// <summary>(Edit) [Р] разведка; Ascon.Pilot.Client.Controls.ObjectListViewer.xaml:50; показ: включён ShowPathAsString. ValuePattern: «\A\B» + Enter -> переход (NavigateByString); после перехода режим выключается сам (разведка).</summary>
                [ShownWhen("включён ShowPathAsString")]
                public const string PathEdit = "directoryEdit";
            }

            /// <summary>Поиск в проводнике (ObjectListViewer.xaml:73-121) и шаблон SearchBox (searchbox.xaml:52-66). Результаты - узел «Результаты поиска» (PilotTitles.Explorer.SearchResults). Общего элемента с id нет.</summary>
            public static class Search
            {
                /// <summary>Name пуст (Button) [Р] разведка; Ascon.Pilot.Client.Controls.ObjectListViewer.xaml:73. Ctrl+F.</summary>
                public const string Show = "showSearchButton";

                /// <summary>[К] Ascon.Pilot.Client.Controls.ObjectListViewer.xaml:103; показ: поле поиска открыто.</summary>
                [ShownWhen("поле поиска открыто")]
                public const string Hide = "hideSearchButton";

                /// <summary>(Edit) [К] Ascon.Pilot.Client.Controls.ObjectListViewer.xaml:121; показ: поле поиска открыто.</summary>
                [ShownWhen("поле поиска открыто")]
                public const string Edit = "searchEdit";

                /// <summary>(Button) [К] searchbox.xaml:53; показ: поле открыто; скрыта при пустом тексте и IsReadOnly (searchbox.xaml:82-90).</summary>
                [ShownWhen("поле поиска открыто, текст не пуст")]
                public const string Clear = "clearButton";

                /// <summary>(Button) [К] searchbox.xaml:56; показ: поле открыто; скрыта без SearchCommand (searchbox.xaml:91-93).</summary>
                [ShownWhen("поле поиска открыто, задана SearchCommand")]
                public const string Go = "searchButton";

                /// <summary>(Text) [К] searchbox.xaml:60; показ: поле поиска открыто.</summary>
                [ShownWhen("поле поиска открыто")]
                public const string Hint = "PART_HintMessage";

                /// <summary>[К] searchbox.xaml:62; показ: есть подсказки.</summary>
                [ShownWhen("есть подсказки")]
                public const string AutocompletePopup = "PART_AutocompletePopup";

                /// <summary>[К] searchbox.xaml:66; показ: есть подсказки.</summary>
                [ShownWhen("есть подсказки")]
                public const string AutocompleteList = "autocompleteList";
            }

            /// <summary>Элементы строки списка объектов (ListItem, Name = заголовок; listviewobjectnodetemplate.xaml:36-237). Искать внутри строки.</summary>
            public static class Row
            {
                /// <summary>Name пуст (Image) [Р] разведка; listviewobjectnodetemplate.xaml:36. HelpText = заголовок типа.</summary>
                public const string Icon = "nodeIcon";

                /// <summary>(Text) [Р] разведка; listviewobjectnodetemplate.xaml:51.</summary>
                public const string Title = "titleTextBlock";

                /// <summary>(Text) [Р] разведка; listviewobjectnodetemplate.xaml:79; показ: listviewobjectnodetemplate.xaml:177-185 (по умолчанию Collapsed). ItemType = системное имя типа (разведка).</summary>
                [ShownWhen("заголовок в одну строку, объект не удалён и не вложение")]
                public const string Type = "itemTypeTextBlock";

                /// <summary>(Text) [Р] разведка; listviewobjectnodetemplate.xaml:105; показ: :105-111, TypeVisibilityConverter.cs.</summary>
                [ShownWhen("заголовок не длиннее 2 строк, нет отметки синхронизации")]
                public const string VersionCreated = "tbVersionCreated";

                /// <summary>«заголовок состояния» (Hyperlink) [Р] разведка; commnadpopuplinkcontrol.xaml:12; показ: есть пользовательское состояние или ссылка на элемент справочника. Тот же id у ссылок на элементы справочника (listviewobjectnodetemplate.xaml:118-120, текст elementBookLinkText, 001:397-398), они идут первыми: ссылку состояния искать по дочернему StateText. Ссылок состояния может быть несколько (001:399,401).</summary>
                [ShownWhen("есть пользовательское состояние или ссылка на элемент справочника")]
                public const string State = "commandPopupLink";

                /// <summary>(Text) [Р] разведка; userobjectstatecontrol.xaml:22; показ: у объекта есть пользовательское состояние.</summary>
                [ShownWhen("у объекта есть пользовательское состояние")]
                public const string StateText = "userObjectPopupLinkText";

                /// <summary>«число детей» или Name пуст (Button) [Р] разведка; listviewobjectnodetemplate.xaml:52; показ: HasChildren или CanIncludeChildObjects (:68-73). Name = DisplayChildrenCount (:55), при пустом - иконка и пустой Name (:60-66). UIA Invoke НЕ переходит внутрь - только клик мышью (разведка).</summary>
                [ShownWhen("у объекта есть дочерние или он может их содержать")]
                public const string FallInto = "fallIntoButton";

                /// <summary>(Button) [Р] разведка; listviewobjectnodetemplate.xaml:139; показ: FileSnapshots.Count &gt; 1 (listviewobjectnodetemplate.xaml:153). Toggle.</summary>
                [ShownWhen("у файла больше одной версии")]
                public const string Versions = "versionButton";

                /// <summary>(Image) [К] listviewobjectnodetemplate.xaml:37; показ: есть подписка на объект.</summary>
                [ShownWhen("есть подписка на объект")]
                public const string Subscribed = "AUIDSubscribed";

                /// <summary>[К] listviewobjectnodetemplate.xaml:39; показ: идёт процесс подписания.</summary>
                [ShownWhen("идёт процесс подписания")]
                public const string SigningIndicator = "AUIDSigningProcessIndicator";

                /// <summary>(RadioButton) [К] listviewobjectnodetemplate.xaml:222; показ: раскрыт список версий.</summary>
                [ShownWhen("раскрыт список версий")]
                public const string VersionRadio = "versionRadioButton";

                /// <summary>(Button) [К] listviewobjectnodetemplate.xaml:237; показ: раскрыт список версий.</summary>
                [ShownWhen("раскрыт список версий")]
                public const string MakeActual = "makeActualButton";
            }

            /// <summary>Дерево навигации слева: treeView (ObjectsTree) -> treeViewControl -> ascTreeView - плоский виртуализированный список (TreeViewControl.xaml:153): невидимых строк в UIA нет. Строка - ListItem, Name = заголовок узла.</summary>
            public static class Tree
            {
                /// <summary>Name пуст (Custom) [Р] разведка; Ascon.Pilot.Client.Controls.ObjectsTree.xaml:2.</summary>
                public const string Container = "treeView";

                /// <summary>Name пуст (Custom) [Р] разведка; Ascon.Pilot.VisualElements.Controls.TreeViewControl.xaml:2.</summary>
                public const string Control = "treeViewControl";

                /// <summary>Name пуст (List) [Р] разведка; Ascon.Pilot.VisualElements.Controls.TreeViewControl.xaml:153.</summary>
                public const string List = "ascTreeView";

                /// <summary>Name пуст (Edit) [К] Ascon.Pilot.VisualElements.Controls.TreeViewControl.xaml:139; показ: задан FilterRule (:143-144, NullToBooleanConverter = value == null -> Collapsed).</summary>
                [ShownWhen("у дерева задан FilterRule (в дереве проводника не показывается)")]
                public const string Filter = "filterEdit";

                /// <summary>Name пуст (Button) [Р] разведка; Ascon.Pilot.VisualElements.Controls.TreeViewControl.xaml:23; показ: HasChildren, иначе Hidden (TreeViewControl.xaml:11,23); у строки «Избранное» нет (001:207-209).</summary>
                [ShownWhen("у узла есть дочерние (HasChildren)")]
                public const string Expander = "expander";

                /// <summary>(CheckBox) [К] Ascon.Pilot.VisualElements.Controls.TreeViewControl.xaml:30; показ: режим выбора флажками.</summary>
                [ShownWhen("режим выбора флажками")]
                public const string CheckBox = "checkBox";

                /// <summary>Name пуст (Image) [Р] разведка; objectnodetemplate.xaml:38. ItemStatus = имя типа.</summary>
                public const string TypeIcon = "typeIcon";

                /// <summary>(Text) [Р] разведка; objectnodetemplate.xaml:52.</summary>
                public const string Title = "titleTextBlock";

                /// <summary>«(скрытый)» (Text) [Д] дамп прогона; objectnodetemplate.xaml:53; показ: узел скрытый.</summary>
                [ShownWhen("узел скрытый")]
                public const string SecretMark = "secretMark";

                /// <summary>(Image) [К] objectnodetemplate.xaml:39; показ: есть подписка на объект.</summary>
                [ShownWhen("есть подписка на объект")]
                public const string Subscribed = "AUIDSubscribed";
            }

            /// <summary>Список объектов проводника (ObjectListViewer.xaml:144-146; ObjectsList.xaml:18).</summary>
            public static class List
            {
                /// <summary>Name пуст (Custom) [Р] разведка; Ascon.Pilot.Client.Controls.ObjectListViewer.xaml:144.</summary>
                public const string Container = "objectsList";

                /// <summary>Name пуст (List) [Р] разведка; Ascon.Pilot.Client.Controls.ObjectsList.xaml:18. HelpText «HasItems:{bool}; AwaitingForSelection:{bool}; AwaitingForSelectionToComplete:{bool};» - признак загрузки (разведка); = Explorer.ListViewOf(Explorer.ListViewer).</summary>
                public const string View = "ascListView.ListView";

                /// <summary>(Custom) [Р] разведка; Ascon.Pilot.Client.Controls.ObjectListViewer.xaml:145; показ: список пуст или загружается. Name = текст, например «Выполняется загрузка данных для отображения.».</summary>
                [ShownWhen("список пуст или загружается")]
                public const string InfoPanel = "infoPanel";

                /// <summary>(Text) [К] Ascon.Pilot.Client.Controls.ObjectListViewer.xaml:146; показ: список пуст или загружается.</summary>
                [ShownWhen("список пуст или загружается")]
                public const string InfoText = "AUIDEmptyListViewText";
            }

            /// <summary>Панель проводника при выделении живых объектов (ObjectListToolbarFiller.cs:56-83). Кнопки модулей (IToolbar&lt;ObjectsViewContext&gt;) - в конце, id = name модуля. У вкладок, восстановленных при старте клиента, кнопок модулей нет (разведка). Меню кнопки - ToolbarStripe.MenuOf(id).</summary>
            public static class Toolbar
            {
                /// <summary>Name пуст (Custom) [Р] разведка; Ascon.Pilot.Client.Controls.ObjectListViewer.xaml:24.</summary>
                public const string Container = "objectListViewerToolbar";

                /// <summary>«Импортировать проект...», Name пуст (Button) [Р] разведка; ObjectListToolbarFiller.cs:56; показ: Enterprise или BIM (ObjectListToolbarFiller.cs:54).</summary>
                [ShownWhen("клиент Enterprise или BIM")]
                public const string Import = "miImport";

                /// <summary>«Создать» (Button) [Р] разведка; ObjectListToolbarItemsProvider.cs:37. Кнопка-меню, пункты - Explorer.CreateItem(тип); недоступна, если текущая папка не объект или не синхронизирована.</summary>
                public const string Create = "miCreate";

                /// <summary>«Показать файлы на диске», Name пуст (Button) [Р] разведка; ObjectListToolbarItemsProvider.cs:68; показ: CanShowMountedFiles(текущая папка). Может быть две кнопки с этим id: для папки и для выделения (:78).</summary>
                [ShownWhen("текущая папка или выделение с файлами на диске")]
                public const string ShowMountedFiles = "miShowMountedFiles";

                /// <summary>(Button) [К] ObjectListToolbarItemsProvider.cs:124; показ: есть фильтры проводника. Пункты - Explorer.FilterItem(подпись).</summary>
                [ShownWhen("есть фильтры проводника")]
                public const string DocumentExplorerFilter = "tbiDocumentExplorerFilter";

                /// <summary>«Вставить», Name пуст (Button) [К] ObjectListToolbarItemsProvider.cs:145; показ: vm.ToolbarCanPaste().</summary>
                [ShownWhen("можно вставить")]
                public const string Paste = "miPaste";

                /// <summary>«Вставить ярлык», Name пуст (Button) [К] ObjectListToolbarItemsProvider.cs:149; показ: vm.ToolbarCanPasteAsShortcut(). В контекстном меню - miPasteAsShortcut.</summary>
                [ShownWhen("можно вставить ярлык")]
                public const string PasteShortcut = "miPasteShortcut";

                /// <summary>«Карточка», Name пуст (Button) [Р] разведка; ObjectListToolbarItemsProvider.cs:158; показ: выбран ровно 1 не-корневой узел.</summary>
                [ShownWhen("выбран ровно 1 не корневой узел")]
                public const string ShowProperties = "miShowProperties";

                /// <summary>«Зарегистрировать», Name пуст (Button) [К] ObjectListToolbarItemsProvider.cs:497; показ: RegisterCommand.CanExecute.</summary>
                [ShownWhen("тип регистрируемый")]
                public const string Register = "miRegister";

                /// <summary>«Перейти к исходному файлу на диске | Перейти к папке исходных файлов на диске» (Button) [К] ObjectListToolbarItemsProvider.cs:178; показ: у выбранного есть связь с исходным файлом/папкой, выбран 1, не ECM.</summary>
                [ShownWhen("есть связь с исходным файлом, выбран 1")]
                public const string ShowSourceFile = "miShowSourceFile";

                /// <summary>«Связать с...» (Button) [Р] разведка; ObjectListToolbarItemsProvider.cs:330; показ: CanShowSourceFile, не ECM; miLinkToSource если связи нет, иначе miUpdateLinkToSource.</summary>
                [ShownWhen("связи с исходным файлом нет")]
                public const string LinkToSource = "miLinkToSource";

                /// <summary>«Изменить связь...» (Button) [К] ObjectListToolbarItemsProvider.cs:330; показ: CanShowSourceFile, не ECM; miLinkToSource если связи нет, иначе miUpdateLinkToSource.</summary>
                [ShownWhen("связь с исходным файлом есть")]
                public const string UpdateLinkToSource = "miUpdateLinkToSource";

                /// <summary>«Исходным файлом на диске... | Связать с исходным файлом на диске...» (MenuItem) [К] ObjectListToolbarItemsProvider.cs:334; показ: открыто подменю «Связать с.../Изменить связь...».</summary>
                [ShownWhen("открыто подменю «Связать с.../Изменить связь...»")]
                public const string AddSourceFile = "miAddSourceFile";

                /// <summary>«Папкой исходных файлов на диске... | Связать с папкой исходных файлов на диске...» (MenuItem) [К] ObjectListToolbarItemsProvider.cs:336; показ: открыто подменю «Связать с.../Изменить связь...».</summary>
                [ShownWhen("открыто подменю «Связать с.../Изменить связь...»")]
                public const string AddSourceDirectory = "miAddSourceDirectory";

                /// <summary>«Вложить в задание» (Button) [Р] разведка; ObjectListToolbarItemsProvider.cs:199; показ: есть выделение и корневые типы заданий. В контекстном меню этот пункт - miAttachToSimpleTask; пункты - Explorer.AttachToItem(тип).</summary>
                [ShownWhen("есть выделение и корневые типы заданий")]
                public const string AttachToTask = "miAttachToTask";

                /// <summary>«Вложить в процесс» (Button) [Р] разведка; ObjectListToolbarItemsProvider.cs:215; показ: есть выделение и корневые типы процессов.</summary>
                [ShownWhen("есть выделение и корневые типы процессов")]
                public const string AttachToWorkflow = "miAttachToWorkflow";

                /// <summary>(MenuItem) [К] TemplateFiller.cs:70; показ: открыто подменю «Вложить в задание/процесс», есть шаблоны. Заголовок группы «Шаблоны» (недоступен); шаблоны - Explorer.TemplateToolbarItem(имя).</summary>
                [ShownWhen("открыто подменю «Вложить в задание/процесс», есть шаблоны")]
                public const string Templates = "tbiTemplates";

                /// <summary>«Построить отчёт» (Button) [К] ObjectListToolbarItemsProvider.cs:254; показ: выбран 1 объект, есть отчёты для типа, не 3D Storage.</summary>
                [ShownWhen("выбран 1 объект, для типа есть отчёты")]
                public const string BuildReportMenu = "tbiBuildReportMenu";

                /// <summary>«Совместный доступ...», Name пуст (Button) [Р] разведка; ObjectListToolbarItemsProvider.cs:273; показ: выбраны только объекты. В контекстном меню - miShowSharingSettings.</summary>
                [ShownWhen("выбраны только объекты")]
                public const string SharingSettings = "miSharingSettings";

                /// <summary>«Уведомлять об изменениях», Name пуст (Button) [Р] разведка; ObjectListToolbarItemsProvider.cs:288; показ: нет подписки.</summary>
                [ShownWhen("нет подписки")]
                public const string SubscribeToChanges = "miSubscribeToChanges";

                /// <summary>«Не уведомлять об изменениях», Name пуст (Button) [К] ObjectListToolbarItemsProvider.cs:291; показ: подписан напрямую (строка 294 - подписан косвенно, кнопка недоступна).</summary>
                [ShownWhen("есть подписка")]
                public const string UnsubscribeToChanges = "miUnsubscribeToChanges";

                /// <summary>«Добавить в избранное», Name пуст (Button) [Р] разведка; ObjectListToolbarItemsProvider.cs:306; показ: есть выделение.</summary>
                [ShownWhen("есть выделение")]
                public const string AddToFavorites = "miAddToFavorites";

                /// <summary>«Пакетная печать выбранных элементов», Name пуст (Button) [Р] разведка; ObjectListToolbarItemsProvider.cs:319; показ: Enterprise/BIM, CanBatchPrinting, есть выделение. Опечатка клиента в id («Elemets»).</summary>
                [ShownWhen("Enterprise/BIM, есть выделение")]
                public const string BatchPrintingSelectedElements = "miBatchPrintingSelectedElemets";

                /// <summary>«Отправить» (Button) [Р] разведка; ObjectListToolbarItemsProvider.cs:348; показ: все выбранные могут иметь файлы.</summary>
                [ShownWhen("все выбранные могут иметь файлы")]
                public const string Send = "miSend";

                /// <summary>«На диск» (MenuItem) [К] ObjectListToolbarItemsProvider.cs:352; показ: открыто подменю «Отправить».</summary>
                [ShownWhen("открыто подменю «Отправить»")]
                public const string ToDisk = "miToDisk";

                /// <summary>«Почтой» (MenuItem) [К] ObjectListToolbarItemsProvider.cs:353; показ: открыто подменю «Отправить».</summary>
                [ShownWhen("открыто подменю «Отправить»")]
                public const string ByEmail = "miByEmail";

                /// <summary>«На диск как изображение...» (MenuItem) [К] ObjectListToolbarItemsProvider.cs:355; показ: открыто подменю «Отправить».</summary>
                [ShownWhen("открыто подменю «Отправить»")]
                public const string ToDiskAsImage = "miToDiskAsImage";

                /// <summary>«Открыть с помощью» (Button) [Р] разведка; OpenFileCommandContainer.cs:43; показ: выбран 1 документ с файлами и расширением (ObjectListToolbarItemsProvider.cs:360-373). Приложения - Explorer.Toolbar.ApplicationItem(имя).</summary>
                [ShownWhen("выбран 1 документ с файлом")]
                public const string OpenWith = "miOpenWith";

                /// <summary>«Выбрать программу» (MenuItem) [К] OpenFileCommandContainer.cs:57; показ: открыто подменю «Открыть с помощью».</summary>
                [ShownWhen("открыто подменю «Открыть с помощью»")]
                public const string ChooseProgram = "miChooseProgram";

                /// <summary>«Сравнить с...» (Button) [Р] разведка; CompareWithContainer.cs:128; показ: выбран 1 документ XPS/PDF (ObjectListToolbarItemsProvider.cs:375-383).</summary>
                [ShownWhen("выбран 1 документ XPS/PDF")]
                public const string CompareWith = "miCompareWith";

                /// <summary>«Выбрать другой документ...», Name пуст (MenuItem) [К] CompareWithContainer.cs:133; показ: открыто подменю «Сравнить с...».</summary>
                [ShownWhen("открыто подменю «Сравнить с...»")]
                public const string ChooseDocumentForCompare = "miChooseDocumentForCompare";

                /// <summary>(MenuItem) [К] CompareWithContainer.cs:139; показ: открыто подменю «Сравнить с...». Заголовок группы «Предыдущие версии».</summary>
                [ShownWhen("открыто подменю «Сравнить с...»")]
                public const string PreviousVersions = "miPreviousVersions";

                /// <summary>«Обновить текстовые метки» (Button) [К] ObjectListToolbarItemsProvider.cs:389; показ: XPS, можно обновить текстовые метки.</summary>
                [ShownWhen("XPS с обновляемыми текстовыми метками")]
                public const string UpdateBarcodeLabels = "miUpdateBarcodeLabels";

                /// <summary>«Экспортировать проект...», Name пуст (Button) [Р] разведка; ObjectListToolbarItemsProvider.cs:401; показ: MenuItemsProvider.CanExport (>= ICE, не смарт-папка, в проекте или папка проектов).</summary>
                [ShownWhen("экспорт разрешён (CanExport)")]
                public const string Export = "miExport";

                /// <summary>«Заморозить», Name пуст (Button) [Р] разведка; ObjectListToolbarItemsProvider.cs:449; показ: CanShowFreeze.</summary>
                [ShownWhen("можно заморозить")]
                public const string Freeze = "miFreeze";

                /// <summary>«Разморозить», Name пуст (Button) [К] ObjectListToolbarItemsProvider.cs:453; показ: CanShowUnfreeze.</summary>
                [ShownWhen("объект заморожен")]
                public const string Unfreeze = "miUnfreeze";

                /// <summary>«Назначить состояние для версии» (Button) [К] ObjectListToolbarItemsProvider.cs:478; показ: выбран 1 XPS/PDF, есть доступные переходы состояния версии. Пункты - Explorer.AssignStateToVersionItem(заголовок).</summary>
                [ShownWhen("выбран 1 XPS/PDF с переходами состояния версии")]
                public const string AssignStateToVersion = "tbiAssignStateToVersion";

                /// <summary>«Копировать ссылку», Name пуст (Button) [Р] разведка; ObjectListToolbarItemsProvider.cs:427; показ: CanShowCopyMenu. Копирует ССЫЛКУ; Ctrl+C.</summary>
                [ShownWhen("можно копировать ссылку")]
                public const string Copy = "miCopy";

                /// <summary>«Вырезать», Name пуст (Button) [Р] разведка; ObjectListToolbarItemsProvider.cs:409; показ: CanShowCutMenu. Ctrl+X.</summary>
                [ShownWhen("можно вырезать")]
                public const string Cut = "miCut";

                /// <summary>«Удалить», Name пуст (Button) [Р] разведка; ObjectListToolbarItemsProvider.cs:436; показ: CanShowDeleteMenu. Del; подтверждение - окно PilotTitles.Windows.DeleteConfirmation.</summary>
                [ShownWhen("можно удалить")]
                public const string Delete = "miDelete";

                /// <summary>(Button) [К] ObjectListToolbarFiller.cs:35; показ: панель пуста (заглушка). Заглушка пустой панели.</summary>
                [ShownWhen("панель пуста")]
                public const string Empty = "empty";

                /// <summary>Приложение в меню «Открыть с помощью» панели: "miApplication" + имя приложения БЕЗ очистки. [К] OpenFileCommandContainer.cs:49.</summary>
                public static string ApplicationItem(string applicationName) => "miApplication" + applicationName;

                /// <summary>Версия в меню кнопки «Сравнить с...»: "miCompareWithHistory" + i (i с 0 по порядку версий XPS/PDF от новой; все версии сразу, без «еще N»). [К] CompareWithContainer.cs:144-147.</summary>
                public static string CompareWithHistory(int index) => "miCompareWithHistory" + index;
            }

            /// <summary>Та же панель objectListViewerToolbar, когда выделены только удалённые объекты (ObjectListToolbarFiller.cs:39-48). В этом состоянии разведка не выполнялась.</summary>
            public static class RecycleBinToolbar
            {
                /// <summary>Name пуст (Custom) [Р] разведка; Ascon.Pilot.Client.Controls.ObjectListViewer.xaml:24.</summary>
                public const string Container = "objectListViewerToolbar";

                /// <summary>«Восстановить в исходное расположение», Name пуст (Button) [К] ObjectListToolbarItemsProvider.cs:418; показ: есть выделение и CanRestoreToOriginalLocation.</summary>
                [ShownWhen("выделение удалённых объектов")]
                public const string RestoreToOriginalLocation = "miRestoreToOriginalLocation";

                /// <summary>«Вырезать», Name пуст (Button) [К] ObjectListToolbarItemsProvider.cs:409; показ: CanShowCutMenu.</summary>
                [ShownWhen("выделение удалённых объектов")]
                public const string Cut = "miCut";

                /// <summary>«Удалить безвозвратно» (Button) [К] ObjectListToolbarItemsProvider.cs:436; показ: CanShowDeleteMenu.</summary>
                [ShownWhen("выделение удалённых объектов")]
                public const string Delete = "miDelete";

                /// <summary>«Совместный доступ...», Name пуст (Button) [К] ObjectListToolbarItemsProvider.cs:273; показ: выделение удалённых объектов.</summary>
                [ShownWhen("выделение удалённых объектов")]
                public const string SharingSettings = "miSharingSettings";

                /// <summary>«Карточка», Name пуст (Button) [К] ObjectListToolbarItemsProvider.cs:158; показ: выделение удалённых объектов.</summary>
                [ShownWhen("выделение удалённых объектов")]
                public const string ShowProperties = "miShowProperties";
            }

            /// <summary>Контекстное меню списка для живых объектов (ObjContextMenu/MenuItemsProvider.cs). Открывается Apps по строке с фокусом или правым кликом; в UIA - Window cls="Popup", дочернее окно mainPilotView (разведка). Подменю - отдельный Popup. Пункты модулей - в конце, id = name модуля.</summary>
            public static class ListMenu
            {
                /// <summary>Name пуст (Menu) [Р] разведка; Ascon.Pilot.Client.Controls.ObjectsList.xaml:27.</summary>
                public const string Container = "AUIDListViewContextMenu";

                /// <summary>«Создать...» (MenuItem) [К] MenuItemsProvider.cs:66; показ: IsSubItemsVisible (ObjectsListViewModel.cs:103: SelectedItem == null) и есть разрешённые типы (MenuItemsProvider.cs:56-59,76-79); в меню по выделенной строке его нет (разведка). Подменю, пункты - Explorer.CreateItem(тип); недоступен, если контекстный узел не объект или не синхронизирован (:64-68).</summary>
                [ShownWhen("меню без выделенной строки (пустое место списка) или в дереве; есть типы для создания")]
                public const string Create = "miCreate";

                /// <summary>«Зарегистрировать», Name пуст (MenuItem) [К] MenuItemsProvider.cs:649; показ: RegisterCommand.CanExecute.</summary>
                [ShownWhen("тип регистрируемый")]
                public const string Register = "miRegister";

                /// <summary>«Показать файлы на диске», Name пуст (MenuItem) [К] MenuItemsProvider.cs:348; показ: CanShowMountedFiles.</summary>
                [ShownWhen("выделение с файлами на диске")]
                public const string ShowMountedFiles = "miShowMountedFiles";

                /// <summary>«Связать с...» (MenuItem) [Р] разведка; MenuItemsProvider.cs:366; показ: CanShowSourceFile и не ECM.</summary>
                [ShownWhen("связи с исходным файлом нет")]
                public const string LinkToSource = "miLinkToSource";

                /// <summary>«Изменить связь...» (MenuItem) [К] MenuItemsProvider.cs:366; показ: CanShowSourceFile и не ECM.</summary>
                [ShownWhen("связь с исходным файлом есть")]
                public const string UpdateLinkToSource = "miUpdateLinkToSource";

                /// <summary>«Исходным файлом на диске... | Связать с исходным файлом на диске...» (MenuItem) [К] MenuItemsProvider.cs:368; показ: открыто подменю «Связать с.../Изменить связь...».</summary>
                [ShownWhen("открыто подменю «Связать с.../Изменить связь...»")]
                public const string AddSourceFile = "miAddSourceFile";

                /// <summary>«Папкой исходных файлов на диске... | Связать с папкой исходных файлов на диске...» (MenuItem) [К] MenuItemsProvider.cs:370; показ: открыто подменю «Связать с.../Изменить связь...».</summary>
                [ShownWhen("открыто подменю «Связать с.../Изменить связь...»")]
                public const string AddSourceDirectory = "miAddSourceDirectory";

                /// <summary>«Перейти к исходному файлу на диске | Перейти к папке исходных файлов на диске» (MenuItem) [К] MenuItemsProvider.cs:378; показ: есть связь с источником и выбран 1.</summary>
                [ShownWhen("есть связь с исходным файлом, выбран 1")]
                public const string ShowSourceFile = "miShowSourceFile";

                /// <summary>«Вложить в задание» (MenuItem) [Р] разведка; MenuItemsProvider.cs:227; показ: не 3D Storage, есть корневые типы заданий. На панели этот пункт - miAttachToTask; пункты - Explorer.AttachToItem(тип), строятся при раскрытии.</summary>
                [ShownWhen("не 3D Storage, есть корневые типы заданий")]
                public const string AttachToSimpleTask = "miAttachToSimpleTask";

                /// <summary>(MenuItem) [К] TemplateFiller.cs:80; показ: открыто подменю «Вложить в задание/процесс», есть шаблоны. Заголовок группы «Шаблоны»; шаблоны - Explorer.TemplateMenuItem(имя).</summary>
                [ShownWhen("открыто подменю «Вложить в задание/процесс», есть шаблоны")]
                public const string Templates = "miTemplates";

                /// <summary>«Вложить в процесс» (MenuItem) [Р] разведка; MenuItemsProvider.cs:239; показ: есть корневые типы процессов.</summary>
                [ShownWhen("есть корневые типы процессов")]
                public const string AttachToWorkflow = "miAttachToWorkflow";

                /// <summary>«Построить отчёт» (MenuItem) [К] MenuItemsProvider.cs:272; показ: выбран 1 объект, для типа есть отчёты.</summary>
                [ShownWhen("выбран 1 объект, для типа есть отчёты")]
                public const string BuildReportMenu = "miBuildReportMenu";

                /// <summary>«Отменить все непринятые изменения», Name пуст (MenuItem) [К] MenuItemsProvider.cs:397; показ: все выбранные - SynchronizationAborted.</summary>
                [ShownWhen("изменения выбранных не приняты сервером")]
                public const string DiscardSent = "miDiscardSent";

                /// <summary>«Совместный доступ...», Name пуст (MenuItem) [Р] разведка; MenuItemsProvider.cs:410. На панели - miSharingSettings.</summary>
                public const string ShowSharingSettings = "miShowSharingSettings";

                /// <summary>«Уведомлять об изменениях», Name пуст (MenuItem) [Р] разведка; MenuItemsProvider.cs:439; показ: не подписан.</summary>
                [ShownWhen("нет подписки")]
                public const string SubscribeToChanges = "miSubscribeToChanges";

                /// <summary>«Не уведомлять об изменениях», Name пуст (MenuItem) [К] MenuItemsProvider.cs:442; показ: подписан напрямую.</summary>
                [ShownWhen("есть подписка")]
                public const string UnsubscribeToChanges = "miUnsubscribeToChanges";

                /// <summary>«Добавить в избранное», Name пуст (MenuItem) [Р] разведка; MenuItemsProvider.cs:295. У элементов «Избранного» вместо него FavoritesMenu.DeleteFromFavorites.</summary>
                public const string AddToFavorites = "miAddToFavorites";

                /// <summary>«Пакетная печать выбранных элементов» (MenuItem) [Р] разведка; MenuItemsProvider.cs:536; показ: Enterprise/BIM и CanBatchPrinting и CanShowDeleteMenu. Опечатка клиента в id («Elemets»).</summary>
                [ShownWhen("Enterprise/BIM, пакетная печать доступна")]
                public const string BatchPrintingSelectedElements = "miBatchPrintingSelectedElemets";

                /// <summary>«Чувствительность сравнения» (MenuItem) [К] ModelComparator.cs:124; показ: InCompare и CompareManager.Settings.IsOverlay (ModelComparator.cs:120-122).</summary>
                [ShownWhen("включён режим сравнения наложением")]
                public const string OverlaySensitivity = "miOverlaySensitivity";

                /// <summary>«Низкая» (MenuItem) [К] ModelComparator.cs:126; показ: открыто подменю «Чувствительность сравнения».</summary>
                [ShownWhen("открыто подменю «Чувствительность сравнения»")]
                public const string ToleranceSoft = "miToleranceSoft";

                /// <summary>«Средняя» (MenuItem) [К] ModelComparator.cs:129; показ: открыто подменю «Чувствительность сравнения».</summary>
                [ShownWhen("открыто подменю «Чувствительность сравнения»")]
                public const string ToleranceMedium = "miToleranceMedium";

                /// <summary>«Высокая» (MenuItem) [К] ModelComparator.cs:132; показ: открыто подменю «Чувствительность сравнения».</summary>
                [ShownWhen("открыто подменю «Чувствительность сравнения»")]
                public const string ToleranceStrict = "miToleranceStrict";

                /// <summary>«Выключить режим сравнения» (MenuItem) [К] ModelComparator.cs:136; показ: режим сравнения активен. Опечатка клиента в id («Comparision»).</summary>
                [ShownWhen("включён режим сравнения")]
                public const string DisableComparisonMode = "miDisableComparisionMode";

                /// <summary>«Сравнить с...» (MenuItem) [Р] разведка; ModelComparator.cs:139; показ: режим сравнения не активен.</summary>
                [ShownWhen("выбран 1 документ XPS/PDF, режим сравнения выключен")]
                public const string CompareWith = "miCompareWith";

                /// <summary>«Режим сравнения», Name пуст (MenuItem) [К] ModelComparator.cs:142; показ: открыто подменю «Сравнить с...».</summary>
                [ShownWhen("открыто подменю «Сравнить с...»")]
                public const string CompareMode = "miCompareMode";

                /// <summary>«Выбрать другой документ...», Name пуст (MenuItem) [К] ModelComparator.cs:145; показ: открыто подменю «Сравнить с...».</summary>
                [ShownWhen("открыто подменю «Сравнить с...»")]
                public const string ChooseDocumentForCompare = "miChooseDocumentForCompare";

                /// <summary>(MenuItem) [К] ModelComparator.cs:151; показ: открыто подменю «Сравнить с...». Заголовок группы «Предыдущие версии».</summary>
                [ShownWhen("открыто подменю «Сравнить с...»")]
                public const string PreviousVersions = "miPreviousVersions";

                /// <summary>(MenuItem) [К] ModelComparator.cs:167; показ: открыто подменю «Сравнить с...», версий больше трёх. «еще N».</summary>
                [ShownWhen("открыто подменю «Сравнить с...», версий больше трёх")]
                public const string More = "miMore";

                /// <summary>Версия в подменю «Сравнить с...»: "miCompareWithHistory" + i (i с 0 по порядку версий от новой; первые три - сразу, остальные - после More). [К] ModelComparator.cs:159,283.</summary>
                public static string CompareWithHistory(int index) => "miCompareWithHistory" + index;

                /// <summary>«Обновить текстовые метки» (MenuItem) [К] MenuItemsProvider.cs:213; показ: XPS и CanUpdateBarcodeLabels.</summary>
                [ShownWhen("XPS с обновляемыми текстовыми метками")]
                public const string UpdateBarcodeLabels = "miUpdateBarcodeLabels";

                /// <summary>«Экспортировать проект...», Name пуст (MenuItem) [Р] разведка; MenuItemsProvider.cs:460; показ: CanExport.</summary>
                [ShownWhen("экспорт разрешён (CanExport)")]
                public const string Export = "miExport";

                /// <summary>«Отправить» (MenuItem) [Р] разведка; MenuItemsProvider.cs:312; показ: все выбранные могут иметь файлы.</summary>
                [ShownWhen("все выбранные могут иметь файлы")]
                public const string Send = "miSend";

                /// <summary>«На диск» (MenuItem) [К] MenuItemsProvider.cs:313; показ: открыто подменю «Отправить».</summary>
                [ShownWhen("открыто подменю «Отправить»")]
                public const string ToDisk = "miToDisk";

                /// <summary>«Почтой» (MenuItem) [К] MenuItemsProvider.cs:314; показ: открыто подменю «Отправить».</summary>
                [ShownWhen("открыто подменю «Отправить»")]
                public const string ByEmail = "miByEmail";

                /// <summary>«На диск как изображение...» (MenuItem) [К] MenuItemsProvider.cs:316; показ: открыто подменю «Отправить».</summary>
                [ShownWhen("открыто подменю «Отправить»")]
                public const string ToDiskAsImage = "miToDiskAsImage";

                /// <summary>«Открыть с помощью» (MenuItem) [Р] разведка; OpenFileCommandContainer.cs:69; показ: выбран 1 документ с файлами (MenuItemsProvider.cs:317-320).</summary>
                [ShownWhen("выбран 1 документ с файлами")]
                public const string OpenWith = "miOpenWith";

                /// <summary>(MenuItem) [К] OpenFileCommandContainer.cs:72; показ: открыто подменю «Открыть с помощью». У ВСЕХ приложений один id - выбирать по Name.</summary>
                [ShownWhen("открыто подменю «Открыть с помощью»")]
                public const string Application = "miApplication";

                /// <summary>«Выбрать программу» (MenuItem) [К] OpenFileCommandContainer.cs:80; показ: открыто подменю «Открыть с помощью».</summary>
                [ShownWhen("открыто подменю «Открыть с помощью»")]
                public const string ChooseProgram = "miChooseProgram";

                /// <summary>«Отправить в Компоновщик документов» (MenuItem) [Р] разведка; MenuItemsProvider.cs:324; показ: ClientFeatures &gt; Ice и не смарт-папка.</summary>
                [ShownWhen("Enterprise/BIM, не смарт-папка")]
                public const string SendToXpsMerger = "miSendToXpsMerger";

                /// <summary>«Заморозить», Name пуст (MenuItem) [Р] разведка; MenuItemsProvider.cs:636; показ: CanShowFreeze.</summary>
                [ShownWhen("можно заморозить")]
                public const string Freeze = "miFreeze";

                /// <summary>«Разморозить», Name пуст (MenuItem) [К] MenuItemsProvider.cs:640; показ: CanShowUnfreeze.</summary>
                [ShownWhen("объект заморожен")]
                public const string Unfreeze = "miUnfreeze";

                /// <summary>«Копировать идентификатор», Name пуст (MenuItem) [Р] разведка; MenuItemsProvider.cs:547; показ: CanShowCopyObjectIdMenu.</summary>
                [ShownWhen("можно копировать идентификатор")]
                public const string CopyId = "miCopyId";

                /// <summary>«Вырезать», Name пуст (MenuItem) [Р] разведка; MenuItemsProvider.cs:556; показ: CanShowCutMenu. Ctrl+X.</summary>
                [ShownWhen("можно вырезать")]
                public const string Cut = "miCut";

                /// <summary>«Копировать ссылку», Name пуст (MenuItem) [Р] разведка; MenuItemsProvider.cs:565; показ: CanShowCopyMenu. Копирует ССЫЛКУ; Ctrl+C.</summary>
                [ShownWhen("можно копировать ссылку")]
                public const string Copy = "miCopy";

                /// <summary>«Копировать как путь», Name пуст (MenuItem) [К] MenuItemsProvider.cs:574; показ: CanShowCopyFilePathMenu.</summary>
                [ShownWhen("можно копировать путь к файлу")]
                public const string CopyFilePath = "miCopyFilePath";

                /// <summary>«Вставить», Name пуст (MenuItem) [К] MenuItemsProvider.cs:583; показ: CanShowPasteMenu. Ctrl+V.</summary>
                [ShownWhen("можно вставить")]
                public const string Paste = "miPaste";

                /// <summary>«Вставить ярлык», Name пуст (MenuItem) [К] MenuItemsProvider.cs:592; показ: CanShowPasteAsShortcutMenu. На панели - miPasteShortcut.</summary>
                [ShownWhen("можно вставить ярлык")]
                public const string PasteAsShortcut = "miPasteAsShortcut";

                /// <summary>«Удалить», Name пуст (MenuItem) [Р] разведка; MenuItemsProvider.cs:603; показ: CanShowDeleteMenu. Del.</summary>
                [ShownWhen("можно удалить")]
                public const string Delete = "miDelete";

                /// <summary>«Расположение элемента», Name пуст (MenuItem) [К] MenuItemsProvider.cs:617; показ: CanNavigate.</summary>
                [ShownWhen("можно перейти к расположению")]
                public const string Navigate = "miNavigate";

                /// <summary>«Карточка», Name пуст (MenuItem) [Р] разведка; MenuItemsProvider.cs:628; показ: выбран ровно 1. F2.</summary>
                [ShownWhen("выбран ровно 1")]
                public const string ShowProperties = "miShowProperties";
            }

            /// <summary>Контекстное меню, когда выделены только удалённые объекты (ObjectsContextMenu.cs:154-162). В этом состоянии разведка не выполнялась.</summary>
            public static class RecycleBinMenu
            {
                /// <summary>Name пуст (Menu) [Р] разведка; Ascon.Pilot.Client.Controls.ObjectsList.xaml:27.</summary>
                public const string Container = "AUIDListViewContextMenu";

                /// <summary>«Отменить все непринятые изменения», Name пуст (MenuItem) [К] MenuItemsProvider.cs:397; показ: все выбранные - SynchronizationAborted.</summary>
                [ShownWhen("изменения выбранных не приняты сервером")]
                public const string DiscardSent = "miDiscardSent";

                /// <summary>«Восстановить в исходное расположение», Name пуст (MenuItem) [К] MenuItemsProvider.cs:404.</summary>
                public const string RestoreToOriginalLocation = "miRestoreToOriginalLocation";

                /// <summary>«Вырезать», Name пуст (MenuItem) [К] MenuItemsProvider.cs:556; показ: CanShowCutMenu.</summary>
                [ShownWhen("можно вырезать")]
                public const string Cut = "miCut";

                /// <summary>«Удалить безвозвратно» (MenuItem) [К] MenuItemsProvider.cs:603; показ: CanShowDeleteMenu (MenuItemsProvider.cs:599).</summary>
                [ShownWhen("можно удалить (CanShowDeleteMenu)")]
                public const string Delete = "miDelete";

                /// <summary>«Совместный доступ...», Name пуст (MenuItem) [К] MenuItemsProvider.cs:410.</summary>
                public const string ShowSharingSettings = "miShowSharingSettings";

                /// <summary>«Карточка», Name пуст (MenuItem) [К] MenuItemsProvider.cs:628; показ: выбран ровно 1 (MenuItemsProvider.cs:624).</summary>
                [ShownWhen("выбран ровно 1")]
                public const string ShowProperties = "miShowProperties";
            }

            /// <summary>Контекстное меню корневого узла «Начало» (ObjectsContextMenu.cs:72-82). В этом состоянии разведка не выполнялась.</summary>
            public static class RootMenu
            {
                /// <summary>Name пуст (Menu) [Р] разведка; Ascon.Pilot.Client.Controls.ObjectsList.xaml:27.</summary>
                public const string Container = "AUIDListViewContextMenu";

                /// <summary>«Создать в корне» (MenuItem) [К] MenuItemsProvider.cs:80. Здесь «Создать в корне»; пункты - Explorer.CreateItem(тип).</summary>
                public const string Create = "miCreate";

                /// <summary>«Отменить все непринятые изменения», Name пуст (MenuItem) [К] MenuItemsProvider.cs:397; показ: все выбранные - SynchronizationAborted.</summary>
                [ShownWhen("изменения выбранных не приняты сервером")]
                public const string DiscardSent = "miDiscardSent";

                /// <summary>«Совместный доступ...», Name пуст (MenuItem) [К] MenuItemsProvider.cs:410.</summary>
                public const string ShowSharingSettings = "miShowSharingSettings";

                /// <summary>«Копировать идентификатор», Name пуст (MenuItem) [К] MenuItemsProvider.cs:547; показ: CanShowCopyObjectIdMenu.</summary>
                [ShownWhen("можно копировать идентификатор")]
                public const string CopyId = "miCopyId";

                /// <summary>«Вставить», Name пуст (MenuItem) [К] MenuItemsProvider.cs:583; показ: CanShowPasteMenu.</summary>
                [ShownWhen("можно вставить")]
                public const string Paste = "miPaste";

                /// <summary>«Вставить ярлык», Name пуст (MenuItem) [К] MenuItemsProvider.cs:592; показ: CanShowPasteAsShortcutMenu.</summary>
                [ShownWhen("можно вставить ярлык")]
                public const string PasteAsShortcut = "miPasteAsShortcut";
            }

            /// <summary>Контекстное меню элементов «Избранного» (ObjectsContextMenu.cs:119-152): пункты как в ListMenu, только вместо miAddToFavorites - пункт ниже.</summary>
            public static class FavoritesMenu
            {
                /// <summary>Name пуст (Menu) [Р] разведка; Ascon.Pilot.Client.Controls.ObjectsList.xaml:27.</summary>
                public const string Container = "AUIDListViewContextMenu";

                /// <summary>«Удалить из избранного», Name пуст (MenuItem) [К] MenuItemsProvider.cs:302; показ: узлы избранного. Префикс mni, а не mi.</summary>
                [ShownWhen("выбран элемент «Избранного»")]
                public const string DeleteFromFavorites = "mniDeleteFromFavorites";
            }

            /// <summary>Контекстное меню дерева навигации (ObjectsTree.cs:79; NavigationTreeViewContextMenu.cs:23-100). Живые объекты - подмножество ListMenu без LinkToSource/UpdateLinkToSource/ShowSourceFile, CompareWith, UpdateBarcodeLabels, Send/OpenWith/SendToXpsMerger (:30-55); узлы «Избранного» - только DeleteFromFavorites (:25-28); удалённые объекты - пунктов RecycleBinMenu нет (:59-65). Здесь - только собственные пункты дерева. Горячие клавиши дерева: Del, F2, Ctrl+X, Ctrl+V.</summary>
            public static class TreeMenu
            {
                /// <summary>(Menu) [К] ObjectsTree.cs:79.</summary>
                public const string Container = "AUIDTreeContextMenu";

                /// <summary>«Развернуть до открытой папки» (MenuItem) [К] NavigationTreeViewContextMenu.cs:99; показ: _showExpandToFolderItem (не для панели навигации). Переключатель.</summary>
                [ShownWhen("меню дерева проводника (не панели навигации)")]
                public const string ExpandToOpenFolder = "miExpandToOpenFolder";

                /// <summary>«Удалить из избранного», Name пуст (MenuItem) [К] NavigationTreeViewContextMenu.cs:27,63; MenuItemsProvider.cs:302; показ: узлы «Избранного»; тогда в меню только этот пункт.</summary>
                [ShownWhen("выбраны узлы «Избранного» (живые или удалённые)")]
                public const string DeleteFromFavorites = "mniDeleteFromFavorites";
            }

            /// <summary>Вкладка «Файлы» нижней панели проводника (DocumentFilesView.xaml): панель documentFilesViewToolbar и таблица файлов.</summary>
            public static class Files
            {
                /// <summary>Name пуст (Custom) [Р] разведка; Ascon.Pilot.Client.DocumentExplorer.DocumentFilesView.DocumentFilesView.xaml:2.</summary>
                public const string Container = "AUIDdocumentFilesView";

                /// <summary>Name пуст (DataGrid) [Р] разведка; Ascon.Pilot.Client.DocumentExplorer.DocumentFilesView.DocumentFilesView.xaml:9.</summary>
                public const string Grid = "documentFilesDataGrid";

                /// <summary>(Custom) [Р] разведка; Ascon.Pilot.Client.DocumentExplorer.DocumentFilesView.DocumentFilesView.xaml:23. HelpText = имя файла.</summary>
                public const string TitleCell = "AUIDTitleCell";

                /// <summary>Name пуст (Image) [Р] разведка; Ascon.Pilot.Client.DocumentExplorer.DocumentFilesView.DocumentFilesView.xaml:32. Внутри TitleCell; HelpText = состояние файла.</summary>
                public const string FileStateIcon = "fileStateIcon";

                /// <summary>(Custom) [Р] разведка; Ascon.Pilot.Client.DocumentExplorer.DocumentFilesView.DocumentFilesView.xaml:47.</summary>
                public const string ModifiedCell = "AUIDModifiedCell";

                /// <summary>(Custom) [Р] разведка; Ascon.Pilot.Client.DocumentExplorer.DocumentFilesView.DocumentFilesView.xaml:55. HelpText = размер в байтах.</summary>
                public const string FileSizeCell = "AUIDFileSizeCell";

                /// <summary>(Custom) [Р] разведка; Ascon.Pilot.Client.DocumentExplorer.DocumentFilesView.DocumentFilesView.xaml:68.</summary>
                public const string PublishedDateCell = "AUIDPublishedDateCell";

                /// <summary>Панель вкладки «Файлы» (DocumentFilesView.xaml:8; DocumentFilesViewModelBase.cs:393-398).</summary>
                public static class Toolbar
                {
                    /// <summary>Name пуст (Custom) [Р] разведка; Ascon.Pilot.Client.DocumentExplorer.DocumentFilesView.DocumentFilesView.xaml:8.</summary>
                    public const string Container = "documentFilesViewToolbar";

                    /// <summary>«Опубликовать», Name пуст (Button) [Р] разведка; DocumentFilesViewModel.cs:63. Split-кнопка.</summary>
                    public const string Publish = "tbiPublish";

                    /// <summary>Name пуст (Button) [Р] разведка; toolbarstripe.xaml:152. Стрелка split-кнопки tbiPublish = ToolbarStripe.SplitArrowOf(Publish).</summary>
                    public const string PublishMoreButton = "tbiPublishMoreButton";

                    /// <summary>«Опубликовать с указанием причины», Name пуст (MenuItem) [К] DocumentFilesViewModel.cs:66; показ: открыто меню tbiPublishMoreButton.</summary>
                    [ShownWhen("открыто меню tbiPublishMoreButton")]
                    public const string PublishWithReason = "tbiPublishWithReason";

                    /// <summary>«Опубликовать добавлением», Name пуст (MenuItem) [К] DocumentFilesViewModel.cs:67; показ: открыто меню tbiPublishMoreButton.</summary>
                    [ShownWhen("открыто меню tbiPublishMoreButton")]
                    public const string AppendPublish = "tbiAppendPublish";

                    /// <summary>«Добавить исходный файл», Name пуст (Button) [Р] разведка; DocumentFilesViewModelBase.cs:393.</summary>
                    public const string AddSourceFile = "tbiAddSourceFile";

                    /// <summary>«Удалить», Name пуст (Button) [Р] разведка; DocumentFilesViewModelBase.cs:394.</summary>
                    public const string RemoveSourceFile = "tbiRemoveSourceFile";

                    /// <summary>«Показать файлы на диске», Name пуст (Button) [Р] разведка; DocumentFilesViewModelBase.cs:395.</summary>
                    public const string ShowOnDisk = "tbiShowOnDisk";

                    /// <summary>«Обновить поля файлов», Name пуст (Button) [Р] разведка; DocumentFilesViewModelBase.cs:398; показ: SupportsUpdateFileAttributes.</summary>
                    [ShownWhen("документ поддерживает обновление полей файлов")]
                    public const string UpdateFileAttributes = "tbiUpdateFileAttributes";
                }
            }

            /// <summary>Контекстное меню таблицы файлов (FilesContextMenu/MenuItemsProvider.cs; DocumentFilesContextMenu.cs:30-95). Пункты модулей - в конце.</summary>
            public static class FilesMenu
            {
                /// <summary>(Menu) [К] Ascon.Pilot.Client.DocumentExplorer.DocumentFilesView.DocumentFilesView.xaml:11.</summary>
                public const string Container = "AUIDDocumentFilesContextMenu";

                /// <summary>«Вставить» (MenuItem) [К] MenuItemsProvider.cs:238.</summary>
                public const string PasteFileCommand = "miPasteFileCommand";

                /// <summary>«Открыть», Name пуст (MenuItem) [К] MenuItemsProvider.cs:211; показ: выбран файл (DocumentFilesContextMenu.cs:35; без выделения SelectedNodes = null, DocumentFilesViewModelBase.cs:268).</summary>
                [ShownWhen("выбран файл")]
                public const string OpenSourceFile = "miOpenSourceFile";

                /// <summary>«Открыть с помощью» (MenuItem) [К] OpenFileCommandContainer.cs:69; показ: выбран файл с расширением (DocumentFilesContextMenu.cs:38; OpenFileCommandContainer.cs:64-68).</summary>
                [ShownWhen("выбран файл с расширением")]
                public const string OpenWith = "miOpenWith";

                /// <summary>(MenuItem) [К] OpenFileCommandContainer.cs:72; показ: открыто подменю «Открыть с помощью». У ВСЕХ приложений один id - выбирать по Name.</summary>
                [ShownWhen("открыто подменю «Открыть с помощью»")]
                public const string Application = "miApplication";

                /// <summary>«Выбрать программу» (MenuItem) [К] OpenFileCommandContainer.cs:80; показ: открыто подменю «Открыть с помощью».</summary>
                [ShownWhen("открыто подменю «Открыть с помощью»")]
                public const string ChooseProgram = "miChooseProgram";

                /// <summary>«Копировать» (MenuItem) [К] MenuItemsProvider.cs:227; показ: выбран файл (DocumentFilesContextMenu.cs:35).</summary>
                [ShownWhen("выбран файл")]
                public const string CopyFileCommand = "miCopyFileCommand";

                /// <summary>«Вырезать» (MenuItem) [К] MenuItemsProvider.cs:249; показ: выбран файл (DocumentFilesContextMenu.cs:35).</summary>
                [ShownWhen("выбран файл")]
                public const string CutFileCommand = "miCutFileCommand";

                /// <summary>«[состояние файла]», Name пуст при нескольких файлах (MenuItem) [К] MenuItemsProvider.cs:31; DocumentFilesContextMenu.cs:45-50; показ: только смонтированный документ, StateVisible().</summary>
                [ShownWhen("документ смонтирован на диск")]
                public const string State = "miState";

                /// <summary>«Разблокировать» (MenuItem) [К] MenuItemsProvider.cs:21; показ: выбран файл, UnlockVisible() (DocumentFilesViewModel.cs:102-105).</summary>
                [ShownWhen("выбран файл; команда видима (UnlockVisible())")]
                public const string Unlock = "miUnlock";

                /// <summary>«Загрузить изменения с сервера» (MenuItem) [К] MenuItemsProvider.cs:40; показ: только смонтированный документ.</summary>
                [ShownWhen("документ смонтирован на диск")]
                public const string GetLatestVersion = "miGetLatestVersion";

                /// <summary>«Отправить на сервер» (MenuItem) [К] MenuItemsProvider.cs:70; показ: только смонтированный документ.</summary>
                [ShownWhen("документ смонтирован на диск")]
                public const string Commit = "miCommit";

                /// <summary>«Опубликовать» (MenuItem) [К] MenuItemsProvider.cs:50; показ: только смонтированный документ.</summary>
                [ShownWhen("документ смонтирован на диск")]
                public const string PublishAndCommit = "miPublishAndCommit";

                /// <summary>«Опубликовать добавлением» (MenuItem) [К] MenuItemsProvider.cs:60; показ: только смонтированный документ.</summary>
                [ShownWhen("документ смонтирован на диск")]
                public const string AppendPublishAndCommit = "miAppendPublishAndCommit";

                /// <summary>«Отменить непринятые изменения» (MenuItem) [К] MenuItemsProvider.cs:80; показ: только смонтированный документ.</summary>
                [ShownWhen("документ смонтирован на диск")]
                public const string Discard = "miDiscard";

                /// <summary>«Отменить изменения» (MenuItem) [К] MenuItemsProvider.cs:90; показ: только смонтированный документ.</summary>
                [ShownWhen("документ смонтирован на диск")]
                public const string Revert = "miRevert";

                /// <summary>«Сделать доступным автономно» (MenuItem) [К] MenuItemsProvider.cs:100; показ: только смонтированный документ.</summary>
                [ShownWhen("документ смонтирован на диск")]
                public const string Download = "miDownload";

                /// <summary>«Совместный доступ...» (MenuItem) [К] MenuItemsProvider.cs:110; показ: выбран файл, ShareSettingsVisible() (DocumentFilesViewModel.cs:132-135).</summary>
                [ShownWhen("выбран файл; команда видима (ShareSettingsVisible())")]
                public const string ShareSettings = "miShareSettings";

                /// <summary>«Версии файла» (MenuItem) [К] MenuItemsProvider.cs:120; показ: выбран файл, ShowHistoryVisible() (DocumentFilesViewModel.cs:137-140).</summary>
                [ShownWhen("выбран файл; команда видима (ShowHistoryVisible())")]
                public const string ShowHistory = "miShowHistory";

                /// <summary>«Уведомлять об изменениях» (MenuItem) [К] MenuItemsProvider.cs:130; показ: выбран файл, SubscribeVisible() (DocumentFilesViewModel.cs:142-145).</summary>
                [ShownWhen("выбран файл; команда подписки видима (SubscribeVisible())")]
                public const string Subscribe = "miSubscribe";

                /// <summary>«Не уведомлять об изменениях» (MenuItem) [К] MenuItemsProvider.cs:141; показ: выбран файл, UnsubscribeVisible() (DocumentFilesViewModel.cs:147-150).</summary>
                [ShownWhen("выбран файл; команда отписки видима (UnsubscribeVisible())")]
                public const string UnSubscribe = "miUnSubscribe";

                /// <summary>«Чат по элементу» (MenuItem) [К] MenuItemsProvider.cs:197; показ: выбран файл, ShowChatDialogVisible() (DocumentFilesViewModel.cs:127-130).</summary>
                [ShownWhen("выбран файл; команда видима (ShowChatDialogVisible())")]
                public const string ShowChatDialog = "miShowChatDialog";

                /// <summary>«Карточка» (MenuItem) [К] MenuItemsProvider.cs:187; показ: выбран файл, ShowPropertiesDialog() (DocumentFilesViewModel.cs:117-120).</summary>
                [ShownWhen("выбран файл; команда видима (ShowPropertiesDialog())")]
                public const string ShowPropertiesDialog = "miShowPropertiesDialog";

                /// <summary>«Копировать ссылку» (MenuItem) [К] MenuItemsProvider.cs:177; показ: выбран файл, CopyLinkVisible() (DocumentFilesViewModel.cs:122-125).</summary>
                [ShownWhen("выбран файл; команда видима (CopyLinkVisible())")]
                public const string CopyLink = "miCopyLink";

                /// <summary>«Заморозить» (MenuItem) [К] MenuItemsProvider.cs:152; показ: выбран файл, FreezeVisible() (DocumentFilesViewModel.cs:107-110). Взаимоисключающий с Unfreeze (else-if, MenuItemsProvider.cs:150-159).</summary>
                [ShownWhen("выбран файл; команда заморозки видима (FreezeVisible())")]
                public const string Freeze = "miFreeze";

                /// <summary>«Разморозить» (MenuItem) [К] MenuItemsProvider.cs:157; показ: выбран файл, FreezeVisible() ложно и UnfreezeVisible() (MenuItemsProvider.cs:150-159).</summary>
                [ShownWhen("выбран файл; заморозка не показана, команда разморозки видима (UnfreezeVisible())")]
                public const string Unfreeze = "miUnfreeze";

                /// <summary>«Заблокировать» (MenuItem) [К] MenuItemsProvider.cs:167; показ: выбран файл, LockVisible() (DocumentFilesViewModel.cs:97-100).</summary>
                [ShownWhen("выбран файл; команда видима (LockVisible())")]
                public const string Lock = "miLock";

                /// <summary>«Удалить», Name пуст (MenuItem) [К] MenuItemsProvider.cs:205; показ: выбран файл (DocumentFilesContextMenu.cs:35,79).</summary>
                [ShownWhen("выбран файл")]
                public const string RemoveSourceFile = "miRemoveSourceFile";
            }

            /// <summary>Вкладка «Связи» нижней панели (DocumentExplorerView.xaml:172-179; LinkedObjectsViewModel.cs:54-67). Пункты меню выделенных связей - Card.Attachments.</summary>
            public static class Links
            {
                /// <summary>(List) [К] Ascon.Pilot.Client.DocumentExplorerView.xaml:172.</summary>
                public const string List = "attachmentsItemControl";

                /// <summary>Панель вкладки «Связи» (LinkedObjectsViewModel.cs:54-67). Кнопки модулей (IToolbar&lt;LinkedObjectsContext&gt;) - в конце.</summary>
                public static class Toolbar
                {
                    /// <summary>(Custom) [К] Ascon.Pilot.Client.DocumentExplorerView.xaml:179.</summary>
                    public const string Container = "objectLinksToolbar";

                    /// <summary>«Добавить» (Button) [К] LinkedObjectsViewModel.cs:54.</summary>
                    public const string Add = "tbiAdd";

                    /// <summary>«Документ», Name пуст (MenuItem) [К] LinkedObjectsViewModel.cs:58; показ: открыто подменю «Добавить».</summary>
                    [ShownWhen("открыто подменю «Добавить»")]
                    public const string AddObject = "tbiAddObject";

                    /// <summary>«Файл», Name пуст (MenuItem) [К] LinkedObjectsViewModel.cs:59; показ: открыто подменю «Добавить».</summary>
                    [ShownWhen("открыто подменю «Добавить»")]
                    public const string AddFile = "tbiAddFile";

                    /// <summary>«Папка», Name пуст (MenuItem) [К] LinkedObjectsViewModel.cs:60; показ: открыто подменю «Добавить».</summary>
                    [ShownWhen("открыто подменю «Добавить»")]
                    public const string AddFolder = "tbiAddFolder";

                    /// <summary>«Удалить из списка», Name пуст (Button) [К] LinkedObjectsViewModel.cs:62.</summary>
                    public const string RemoveAttachments = "tbiRemoveAttachments";
                }

                /// <summary>Контекстное меню списка связей без выделения (LinkedObjectsContextMenuDirector.cs:15-43); с выделением - пункты Card.Attachments.</summary>
                public static class Menu
                {
                    /// <summary>(Menu) [К] AttachmentsContextMenuDirector.cs:23.</summary>
                    public const string Container = "AUIDAttachmentsMenu";

                    /// <summary>«Вставить», Name пуст (MenuItem) [К] LinkedObjectsContextMenuDirector.cs:25; показ: нет выделения, список не только для чтения.</summary>
                    [ShownWhen("нет выделения, список не только для чтения")]
                    public const string InsertObjectAttachments = "miInsertObjectAttachments";
                }
            }

            /// <summary>Вкладка «Задания» нижней панели проводника (DocumentExplorerView.xaml:162-163). Строки - Tasks.Row.</summary>
            public static class LinkedTasks
            {
                /// <summary>Name пуст (Custom) [Д] дамп прогона; Ascon.Pilot.Client.DocumentExplorerView.xaml:163.</summary>
                public const string Host = "linkedTasksTab";

                /// <summary>Name пуст (List) [Д] дамп прогона; Ascon.Pilot.Client.TasksNext.NextTasksListView.xaml:74.</summary>
                public const string List = "AUIDTaskTreeView";

                /// <summary>Панель вкладки «Задания» проводника (LinkedNextTasksListToolbarFiller.cs:14-33): создание, переходы состояния (Tasks.StateTransition), настройки отображения. Кнопки модулей (IToolbar&lt;LinkedTasksContext2&gt;) - в конце.</summary>
                public static class Toolbar
                {
                    /// <summary>Name пуст (Custom) [Д] дамп прогона; Ascon.Pilot.Client.DocumentExplorerView.xaml:162.</summary>
                    public const string Container = "taskViewerToolbar";

                    /// <summary>«Новое задание» (Button) [Д] дамп прогона; NextTaskViewerToolbarFiller.cs:219; показ: есть корневые типы заданий.</summary>
                    [ShownWhen("есть корневые типы заданий")]
                    public const string CreateTask = "miCreateTask";

                    /// <summary>«Создать процесс» (Button) [Д] дамп прогона; NextTaskViewerToolbarFiller.cs:235; показ: есть корневые типы процессов.</summary>
                    [ShownWhen("есть корневые типы процессов")]
                    public const string CreateWorkflow = "miCreateWorkflow";

                    /// <summary>«Настройки отображения заданий» (Button) [Д] дамп прогона; NextTaskViewerToolbarFiller.cs:308.</summary>
                    public const string TasksDisplayOptions = "tbiTasksDisplayOptions";

                    /// <summary>«Отображать отозванные задания» (MenuItem) [К] NextTaskViewerToolbarFiller.cs:310; показ: открыто подменю «Настройки отображения заданий».</summary>
                    [ShownWhen("открыто подменю «Настройки отображения заданий»")]
                    public const string ShowRevoked = "tbiShowRevoked";
                }

                /// <summary>Контекстное меню списка заданий проводника (LinkedNextTasksListViewContextMenuDirector.cs:20): создание (id как в Tasks.Menu), переход к заданию, переходы состояния (Tasks.StateTransition), копирование ссылки; пункты модулей (IMenu&lt;LinkedTasksContext2&gt;) - в конце.</summary>
                public static class Menu
                {
                    /// <summary>(Menu) [К] LinkedNextTasksListViewContextMenuDirector.cs:20.</summary>
                    public const string Container = "AUIDLinkedTaskListContextMenu";

                    /// <summary>«Перейти к заданию», Name пуст (MenuItem) [К] LinkedNextTasksListViewContextMenuDirector.cs:48.</summary>
                    public const string NavigateToTask = "miNavigateToTask";
                }
            }
        }
    }
}
