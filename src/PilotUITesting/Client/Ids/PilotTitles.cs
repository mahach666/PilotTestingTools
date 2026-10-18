namespace PilotUITesting.Client
{
    /// <summary>UIA Name (и HelpText индикатора подключения) русского клиента Pilot 26.11. Строки ru.resx даны без «_». Источники: ruC = Ascon.Pilot.Client, ruCC = Ascon.Pilot.ClientCore, ruCT = Ascon.Pilot.Controls, ruVE = Ascon.Pilot.VisualElements, ruXR = Ascon.Pilot.XpsRender (*.Properties.Resources.ru.resx в decomp\ice\ru). Уровни - как в PilotIds.</summary>
    public static class PilotTitles
    {
        /// <summary>Страница «Новая вкладка»: подписи плиток (Button без id внутри PilotIds.NewTabPage.Tiles, Name = подпись без «_») и групп (Text). Модуль может добавить плитку с той же подписью (например «Задания») - ограничивать поиск группой.</summary>
        public static class NewTab
        {
            /// <summary>[Р] разведка; TabHostViewModel.cs:132; ruC:214. Заголовок вкладки «Новая вкладка» (Name AUIDTabItemLayoutDocument).</summary>
            public const string PageTitle = "Новая вкладка";

            /// <summary>[Р] разведка; ruC:1046. Открывает TabContent.DocumentsExplorer в последнем месте, не в «Начале».</summary>
            public const string ProjectsAndDocuments = "Проекты и документы";

            /// <summary>[Р] разведка; ruC:708. Каждый раз открывает новую вкладку; та же строка - заголовок колонки недавних (Text, 002:37).</summary>
            public const string Tasks = "Задания";

            /// <summary>[Р] разведка; ruC:923.</summary>
            public const string Reports = "Отчеты";

            /// <summary>[Р] разведка; ruC:667.</summary>
            public const string Messages = "Сообщения";

            /// <summary>[Р] разведка; ruC:235. Только Enterprise/BIM.</summary>
            public const string GeometrySearch = "Поиск по геометрии";

            /// <summary>[Р] разведка; ruC:987. Только Enterprise/BIM.</summary>
            public const string XpsMerger = "Компоновщик документов";

            /// <summary>[Р] разведка; ruC:1024. Только Enterprise/BIM.</summary>
            public const string WorkingDaysCalendar = "Календарь рабочих дней";

            /// <summary>[Р] разведка; ruC:1148. Заголовок группы (Text).</summary>
            public const string GroupMainTools = "Основные инструменты";

            /// <summary>[Р] разведка; ruC:1149. Заголовок группы (Text).</summary>
            public const string GroupEnterpriseFeatures = "Возможности Enterprise";

            /// <summary>[Р] разведка; ruCC:41. Заголовок группы плиток модулей (INewTabPage.AddButton).</summary>
            public const string GroupExtensions = "Модули расширения";
        }

        /// <summary>Заголовки окон клиента. У окон DialogService AutomationId = заголовок на момент создания, Name = заголовок или ToString() модели (DialogServiceBase.cs:295); у окна карточки AUIDCreateObject заголовок - это Name.</summary>
        public static class Windows
        {
            /// <summary>[К] ObjectCreateDialogViewModel.cs:73; ObjectDialogView.xaml:2 (Title=DisplayName); ruC:579. Name окна PilotIds.Card.Container при создании; тот же Name у окна прогресса PilotIds.Progress.Container при создании (ObjectCreateDialogViewModel.cs:124) и при проверке перед созданием процесса (дамп прогона; WorkflowDialogViewModel.cs:188,345) - искать с id AUIDCreateObject.</summary>
            public const string CreateCard = "Создание нового элемента";

            /// <summary>[Д] дамп прогона; ruC:1122. Name окна PilotIds.Card.Container при редактировании.</summary>
            public const string EditCard = "Редактирование элемента";

            /// <summary>[К] DialogServiceBase.cs:201-210; ruCT:72. Id = Name окна подтверждения удаления; кнопки PilotIds.MessageBox.Yes («Удалить») / Cancel («Не удалять»).</summary>
            public const string DeleteConfirmation = "Подтверждение операции удаления";

            /// <summary>[К] DialogServiceBase.cs:212-221; ruCT:65. Id = Name окна подтверждения отзыва задания; кнопки Yes («Отозвать») / Cancel («Не отзывать»).</summary>
            public const string RevokeConfirmation = "Подтверждение отзыва";

            /// <summary>[К] DialogService.cs:519-555; ruC:1093. Id окна справочника (если не задан options.Title).</summary>
            public const string ReferenceBook = "Справочник";

            /// <summary>[К] DialogService.cs:557-587; ruC:908. Id окна справочника элементов.</summary>
            public const string ElementBook = "Справочник элементов";

            /// <summary>[К] OrgStructureView.cs:19; ruCC:61. Id = Name окна выбора оргединицы (если не задан options.Title).</summary>
            public const string OrgStructure = "Организационная структура";

            /// <summary>[Д] дамп прогона; CreateNextTaskDialogViewModel.cs:54; ruC:601. Начало id окна нового задания («Новое задание - {заголовок или тип}»); Name окна - WindowNames.TaskEditor.</summary>
            public static readonly string NewTaskPrefix = "Новое задание - ";

            /// <summary>[К] EditNextTaskDialogViewModel.cs:11; ruC:1071. Начало id окна редактирования задания («Редактирование задания - {заголовок}»).</summary>
            public static readonly string EditTaskPrefix = "Редактирование задания - ";

            /// <summary>[Д] дамп прогона; WorkflowDialogViewModel.cs:65,135; ruC:1026. Начало id окна нового процесса; Name окна - WindowNames.WorkflowEditor.</summary>
            public static readonly string NewWorkflowPrefix = "Новый процесс - ";

            /// <summary>[К] WorkflowDialogViewModel.cs:65,161; ruC:374. Начало id окна редактирования процесса.</summary>
            public static readonly string EditWorkflowPrefix = "Редактирование процесса - ";
        }

        /// <summary>Name окон и элементов, равный ToString() модели представления: не зависит от языка клиента. Это Name, а не AutomationId.</summary>
        public static class WindowNames
        {
            /// <summary>[Д] дамп прогона; CreateNextTaskDialogViewModel.cs:247. Окно нового задания (PilotIds.TaskEditor).</summary>
            public const string TaskEditor = "AUIDNextTasksEdit";

            /// <summary>[Д] дамп прогона; WorkflowDialogViewModel.cs:325. Окно процесса (PilotIds.WorkflowEditor).</summary>
            public const string WorkflowEditor = "AUIDWorkflowEditView";

            /// <summary>[К] ObjectsExplorerViewModel.cs:157. Окно выбора документов (PilotIds.Card.ObjectSelector).</summary>
            public const string ObjectSelector = "AUIDProjectsExplorer";

            /// <summary>[К] DocumentTemplateSelectorViewModel.cs:58. Окно выбора шаблона документа (PilotIds.Card.DocumentTemplateSelector).</summary>
            public const string DocumentTemplateSelector = "AUIDDocumentTemplateSelector";

            /// <summary>[К] DocumentTemplateItem.cs:85. Начало Name пункта шаблона: «AUIDDocumentTemplateItem: {шаблон}, {файл}».</summary>
            public static readonly string DocumentTemplateItemPrefix = "AUIDDocumentTemplateItem: ";
        }

        /// <summary>Подписи кнопок (UIA Name без «_»): для кнопок без id (диалоги модулей, окно прогресса, «Загрузить еще») и для проверки подписей кнопок с id.</summary>
        public static class Buttons
        {
            /// <summary>[Р] разведка; ruVE:79. В диалоге модуля кнопка без id; в карточке клиента - PilotIds.Card.Create.</summary>
            public const string Create = "Создать";

            /// <summary>[Р] разведка; ruVE:313. В диалоге модуля и в окне прогресса кнопка без id.</summary>
            public const string Cancel = "Отмена";

            /// <summary>[Д] дамп прогона; ruVE:41. PilotIds.Card.Save.</summary>
            public const string Save = "Сохранить";

            /// <summary>[Д] дамп прогона; okcancelbuttonspanel.xaml:9; ruVE:65. В ShowInfo кнопка «OK» - PilotIds.MessageBox.ClientCancel.</summary>
            public const string Ok = "OK";

            /// <summary>[К] DialogServiceBase.cs:223-232; ruCT:64. AskConfirmation: PilotIds.MessageBox.Yes.</summary>
            public const string Yes = "Да";

            /// <summary>[К] DialogServiceBase.cs:223-232; ruCT:63. AskConfirmation: PilotIds.MessageBox.Cancel.</summary>
            public const string No = "Нет";

            /// <summary>[К] DialogServiceBase.cs:203; ruCT:36. Подтверждение удаления: PilotIds.MessageBox.Yes.</summary>
            public const string Delete = "Удалить";

            /// <summary>[К] DialogServiceBase.cs:203; ruCT:30. Подтверждение удаления: PilotIds.MessageBox.Cancel.</summary>
            public const string NotDelete = "Не удалять";

            /// <summary>[К] DialogServiceBase.cs:212-221; ruCT:39. Подтверждение отзыва: PilotIds.MessageBox.Yes.</summary>
            public const string Revoke = "Отозвать";

            /// <summary>[К] DialogServiceBase.cs:212-221; ruCT:60. Подтверждение отзыва: PilotIds.MessageBox.Cancel.</summary>
            public const string NotRevoke = "Не отзывать";

            /// <summary>[Д] дамп прогона; CreateNextTaskDialogViewModel.cs:99; ruC:34. PilotIds.TaskEditor.Ok.</summary>
            public const string GiveTask = "Выдать задание";

            /// <summary>[К] OrgStructureView.xaml:53; ruVE:219. PilotIds.Card.OrgStructure.Add.</summary>
            public const string Add = "Добавить";

            /// <summary>[К] ClientDialogsService.cs:81; AttachmentsManager.cs:671; ruC:523. PilotIds.Card.ObjectSelector.Ok.</summary>
            public const string Attach = "Вложить";

            /// <summary>[Д] дамп прогона; ruVE:301. Окно ошибки (PilotIds.ErrorWindow.Close); в окне прогресса кнопка без id (ProcessView.xaml:28).</summary>
            public const string Close = "Закрыть";

            /// <summary>[Р] разведка; ruC:367. Вкладка «Задания», кнопка без id внутри PilotIds.Tasks.InfoPanel.</summary>
            public const string LoadMore = "Загрузить еще";

            /// <summary>[К] ObjectDialogView.xaml:58; ruC:871. PilotIds.Card.CreateAsSecret.</summary>
            public const string CreateAsSecret = "Создать скрытым";
        }

        /// <summary>HelpText индикатора PilotIds.MainWindow.ConnectionState: ConnectionStateText через UppercaseConverter (ToUpperInvariant, MainView.xaml:29,62; ConnectionViewController.cs:46,57). Это HelpText, а не Name.</summary>
        public static class Connection
        {
            /// <summary>[Р] разведка; ruC:222. Ресурс «Подключено».</summary>
            public const string Online = "ПОДКЛЮЧЕНО";

            /// <summary>[К] ConnectionViewController.cs:46; ruC:685. Ресурс «Автономная работа».</summary>
            public const string Offline = "АВТОНОМНАЯ РАБОТА";

            /// <summary>[К] ConnectionViewController.cs:57; ruC:263. Ресурс «Нет подключения».</summary>
            public const string NoConnection = "НЕТ ПОДКЛЮЧЕНИЯ";
        }

        /// <summary>Проводник: Name узлов дерева (ListItem, дочерний PilotIds.Explorer.Tree.Title) и вкладок нижней панели (TabItem без id).</summary>
        public static class Explorer
        {
            /// <summary>[Р] разведка; ruC:38. Узел дерева.</summary>
            public const string Favorites = "Избранное";

            /// <summary>[Д] дамп прогона; S20251001_smart_recycle_bin_base.cs:21 (ruVE:141 - клиентский RecycleBinViewModel.cs:24). Узел дерева - смарт-папка корзины, создаётся скриптом обновления базы (скрытая, D00:306; :74); имя зависит от языка БАЗЫ (IsCyrillic).</summary>
            public const string RecycleBin = "Корзина";

            /// <summary>[К] S20251001_smart_recycle_bin_base.cs:23. Смарт-папка корзины, создаётся скриптом обновления базы; имя зависит от языка БАЗЫ (IsCyrillic).</summary>
            public const string AllDeleted = "Все удалённые";

            /// <summary>[К] S20251001_smart_recycle_bin_base.cs:22. Смарт-папка корзины, создаётся скриптом обновления базы; имя зависит от языка БАЗЫ (IsCyrillic).</summary>
            public const string DeletedByMe = "Удалённые мной";

            /// <summary>[К] ruC:729. Узел результатов поиска.</summary>
            public const string SearchResults = "Результаты поиска";

            /// <summary>[Р] разведка; ruVE:256. Вкладка нижней панели (PilotIds.Explorer.Files).</summary>
            public const string FilesTab = "Файлы";

            /// <summary>[Р] разведка; ruC:708. Вкладка нижней панели (PilotIds.Explorer.LinkedTasks).</summary>
            public const string TasksTab = "Задания";

            /// <summary>[Р] разведка; ruC:766. Вкладка нижней панели (PilotIds.Explorer.Links).</summary>
            public const string LinksTab = "Связи";
        }

        /// <summary>Просмотрщик: Name вкладок правой панели (TabItem без id в PilotIds.Viewer.SideTabs).</summary>
        public static class Viewer
        {
            /// <summary>[Р] разведка; ruXR:126. Вкладка списка замечаний (PilotIds.Viewer.Annotations).</summary>
            public const string RemarksTab = "Замечания";
        }
    }
}
