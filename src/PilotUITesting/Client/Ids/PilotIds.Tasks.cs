namespace PilotUITesting.Client
{
    public static partial class PilotIds
    {
        /// <summary>Вкладка «Задания» (NextTaskViewer.xaml). Вкладки «Задания», открытые плиткой после вкладки модуля, в UIA пустые (разведка).</summary>
        public static class Tasks
        {
            /// <summary>(Custom) [Р] разведка; Ascon.Pilot.Client.TasksNext.NextTaskViewer.xaml:2.</summary>
            public const string Container = "AUIDNextTasksTab";

            /// <summary>Name пуст (Button) [Р] разведка; Ascon.Pilot.Client.TasksNext.NextTaskViewer.xaml:36. История фильтра.</summary>
            public const string Back = "btnBack";

            /// <summary>Name пуст (Button) [Р] разведка; Ascon.Pilot.Client.TasksNext.NextTaskViewer.xaml:37.</summary>
            public const string Next = "btnNext";

            /// <summary>Name пуст (Button) [Р] разведка; Ascon.Pilot.Client.TasksNext.NextTaskViewer.xaml:38. Переключатель (Toggle).</summary>
            public const string History = "btnSearchHistory";

            /// <summary>(Edit) [Р] разведка; Ascon.Pilot.Client.TasksNext.NextTaskViewer.xaml:77. Value = строка фильтра; Ctrl+F.</summary>
            public const string Search = "searchEdit";

            /// <summary>Name пуст (List) [Р] разведка; taskfilterstemplates.xaml:72. ListItem Name = подпись фильтра, ItemStatus «Common»; внутри Text с id = подпись фильтра.</summary>
            public const string Filters = "filtersListView";

            /// <summary>Name пуст (Button) [Р] разведка; taskfilterstemplates.xaml:82.</summary>
            public const string FilterSettings = "AUIDTasksFilterSettingsButton";

            /// <summary>Name пуст (Custom) [Р] разведка; Ascon.Pilot.Client.TasksNext.NextTaskViewer.xaml:108.</summary>
            public const string ListHost = "tasksListView";

            /// <summary>Name пуст (List) [Р] разведка; Ascon.Pilot.Client.TasksNext.NextTasksListView.xaml:74. ItemStatus «ProgressBarHasBeenHidden:…»; строки - Tasks.Row.</summary>
            public const string List = "AUIDTaskTreeView";

            /// <summary>(Custom) [Р] разведка; Ascon.Pilot.Client.TasksNext.NextTaskViewer.xaml:91; показ: загружены не все задания (CanLoadMoreTasks). Кнопка «Загрузить еще» без id - PilotTitles.Buttons.LoadMore.</summary>
            [ShownWhen("загружены не все задания (CanLoadMoreTasks)")]
            public const string InfoPanel = "infoPanel";

            /// <summary>(Text) [Р] разведка; Ascon.Pilot.Client.TasksNext.NextTaskViewer.xaml:97; показ: загружены не все задания (CanLoadMoreTasks).</summary>
            [ShownWhen("загружены не все задания (CanLoadMoreTasks)")]
            public const string LoadMoreText = "AUIDLoadMoreTasksText";

            /// <summary>Name пуст (Thumb) [Р] разведка; Ascon.Pilot.Client.TasksNext.NextTaskViewer.xaml:110.</summary>
            public const string DetailsSplitter = "taskDetailSplitter";

            /// <summary>«Создать задание/процесс -> тип» (меню кнопок miCreateTask/miCreateWorkflow и пунктов меню): "miCreate" + XamlName(имя типа). [К] NextTaskViewerToolbarFiller.cs:225,241; NextTaskViewerContextMenuDirector.cs:61,74.</summary>
            public static string CreateItem(string typeName) => "miCreate" + XamlName(typeName);

            /// <summary>Тип подзадания в меню списка: "miCreateSubTask" + XamlName(имя типа). [К] NextTaskViewerContextMenuDirector.cs:98.</summary>
            public static string CreateSubTaskMenuItem(string typeName) => "miCreateSubTask" + XamlName(typeName);

            /// <summary>Тип подзадания в меню кнопки tbiCreateSubTask: "tbiCreateSubTask" + имя типа БЕЗ очистки. [К] NextTaskViewerToolbarFiller.cs:266.</summary>
            public static string CreateSubTaskToolbarItem(string typeName) => "tbiCreateSubTask" + typeName;

            /// <summary>Тип подпроцесса в меню списка: "miCreateSubWorkflow" + XamlName(имя типа). [К] NextTaskViewerContextMenuDirector.cs:112.</summary>
            public static string CreateSubWorkflowMenuItem(string typeName) => "miCreateSubWorkflow" + XamlName(typeName);

            /// <summary>Тип подпроцесса в меню кнопки tbiCreateSubWorkflowMenu: "tbiCreateSubWorkflow" + XamlName(имя типа). [К] NextTaskViewerToolbarFiller.cs:282.</summary>
            public static string CreateSubWorkflowToolbarItem(string typeName) => "tbiCreateSubWorkflow" + XamlName(typeName);

            /// <summary>«Редактировать процесс» в меню списка: "miEditWorkflow" + XamlName(имя типа процесса). [К] NextTaskViewerContextMenuDirector.cs:183.</summary>
            public static string EditWorkflowMenuItem(string workflowTypeName) => "miEditWorkflow" + XamlName(workflowTypeName);

            /// <summary>«Редактировать процесс» на панели: "miEditWorkflowToolbarButton" + XamlName(имя типа процесса). [К] NextTaskViewerToolbarFiller.cs:208.</summary>
            public static string EditWorkflowToolbarButton(string workflowTypeName) => "miEditWorkflowToolbarButton" + XamlName(workflowTypeName);

            /// <summary>Переход состояния задания (кнопка панели и пункт меню): "{имя атрибута}_{имя состояния}" БЕЗ преобразований. [К] NextTaskViewerContextMenuDirector.cs:135; NextTaskViewerToolbarFiller.cs:60.</summary>
            public static string StateTransition(string attributeName, string stateName) => attributeName + "_" + stateName;

            /// <summary>Панель вкладки «Задания» (NextTaskViewerToolbarFiller.cs:33-43, порядок: создать -> переходы -> редактировать процесс -> отображение -> сортировка -> группировка -> шаблоны). Кнопки модулей (IToolbar&lt;TasksViewContext2&gt;) - в конце; у вкладки, восстановленной при старте, их нет (разведка).</summary>
            public static class Toolbar
            {
                /// <summary>Name пуст (Custom) [Р] разведка; Ascon.Pilot.Client.TasksNext.NextTaskViewer.xaml:28.</summary>
                public const string Container = "taskViewerToolbar";

                /// <summary>«Новое задание» (Button) [Р] разведка; NextTaskViewerToolbarFiller.cs:219; показ: есть корневые типы заданий. Пункты - Tasks.CreateItem(тип), шаблоны - Explorer.TemplateToolbarItem.</summary>
                [ShownWhen("есть корневые типы заданий")]
                public const string CreateTask = "miCreateTask";

                /// <summary>«Создать процесс» (Button) [Р] разведка; NextTaskViewerToolbarFiller.cs:235; показ: есть корневые типы процессов.</summary>
                [ShownWhen("есть корневые типы процессов")]
                public const string CreateWorkflow = "miCreateWorkflow";

                /// <summary>«Создать подзадание» (Button) [К] NextTaskViewerToolbarFiller.cs:262; показ: выбрано 1 задание (не этап) и есть типы подзаданий. Пункты - Tasks.CreateSubTaskToolbarItem.</summary>
                [ShownWhen("выбрано 1 задание (не этап), есть типы подзаданий")]
                public const string CreateSubTask = "tbiCreateSubTask";

                /// <summary>«Создать подпроцесс» (Button) [К] NextTaskViewerToolbarFiller.cs:277; показ: выбрано 1 задание (не этап) и есть типы подпроцессов (:249-252, 273-276). Пункты - Tasks.CreateSubWorkflowToolbarItem.</summary>
                [ShownWhen("выбрано 1 задание (не этап), есть типы подпроцессов")]
                public const string CreateSubWorkflowMenu = "tbiCreateSubWorkflowMenu";

                /// <summary>«Настройки отображения» (Button) [Р] разведка; NextTaskViewerToolbarFiller.cs:75.</summary>
                public const string TasksDisplayOptions = "tbiTasksDisplayOptions";

                /// <summary>(MenuItem) [К] NextTaskViewerToolbarFiller.cs:77; показ: открыто подменю «Настройки отображения». Заголовок группы (недоступен).</summary>
                [ShownWhen("открыто подменю «Настройки отображения»")]
                public const string GroupPersonalTemplatesProcesses = "tbiGroupPersonalTemplatesProcesses";

                /// <summary>«Отображать завершенные задания» (MenuItem) [К] NextTaskViewerToolbarFiller.cs:78; показ: открыто подменю «Настройки отображения».</summary>
                [ShownWhen("открыто подменю «Настройки отображения»")]
                public const string ShowCompleted = "tbiShowCompleted";

                /// <summary>«Отображать отозванные задания» (MenuItem) [К] NextTaskViewerToolbarFiller.cs:80; показ: открыто подменю «Настройки отображения».</summary>
                [ShownWhen("открыто подменю «Настройки отображения»")]
                public const string ShowRevoked = "tbiShowRevoked";

                /// <summary>«Показывать группировки свёрнутыми по умолчанию» (MenuItem) [К] NextTaskViewerToolbarFiller.cs:82; показ: открыто подменю «Настройки отображения».</summary>
                [ShownWhen("открыто подменю «Настройки отображения»")]
                public const string ShowTaskGroupingsCollapsed = "tbiShowTaskGroupingsCollapsed";

                /// <summary>«Показывать процессы свёрнутыми по умолчанию» (MenuItem) [К] NextTaskViewerToolbarFiller.cs:85; показ: открыто подменю «Настройки отображения». Опечатка клиента в id («Worflows»).</summary>
                [ShownWhen("открыто подменю «Настройки отображения»")]
                public const string ShowWorkflowsCollapsed = "tbiShowWorflowsCollapsed";

                /// <summary>«Показывать подзадания свёрнутыми по умолчанию» (MenuItem) [К] NextTaskViewerToolbarFiller.cs:88; показ: открыто подменю «Настройки отображения».</summary>
                [ShownWhen("открыто подменю «Настройки отображения»")]
                public const string ShowSubtasksCollapsed = "tbiShowSubtasksCollapsed";

                /// <summary>«Cортировка» (Button) [Р] разведка; NextTaskViewerToolbarFiller.cs:104. Недоступна при группировке.</summary>
                public const string Sort = "tbiSort";

                /// <summary>«Группировка» (Button) [Р] разведка; NextTaskViewerToolbarFiller.cs:148.</summary>
                public const string Grouping = "tbiGrouping";

                /// <summary>«По процессам и этапам» (MenuItem) [К] NextTaskViewerToolbarFiller.cs:150; показ: открыто подменю «Группировка».</summary>
                [ShownWhen("открыто подменю «Группировка»")]
                public const string EnableGroupingByWorkflowsAndStages = "tbiEnableGroupingByWorkflowsAndStages";

                /// <summary>«Настроить...» (MenuItem) [К] NextTaskViewerToolbarFiller.cs:185; показ: открыто подменю «Группировка».</summary>
                [ShownWhen("открыто подменю «Группировка»")]
                public const string ConfigureGrouping = "tbiConfigureGrouping";

                /// <summary>«Настройка шаблонов», Name пуст (Button) [Р] разведка; NextTaskViewerToolbarFiller.cs:303.</summary>
                public const string TemplatesManager = "tbiTemplatesManager";

                /// <summary>(MenuItem) [К] TemplateFiller.cs:70; показ: открыто меню кнопки создания, есть шаблоны. Заголовок группы «Шаблоны».</summary>
                [ShownWhen("открыто меню кнопки создания, есть шаблоны")]
                public const string Templates = "tbiTemplates";
            }

            /// <summary>Контекстное меню списка заданий (NextTaskViewerContextMenuDirector.cs:44-48). Пункты модулей (IMenu&lt;TasksViewContext2&gt;) - после «Копировать ссылку», перед «Редактировать процесс» (Tasks.EditWorkflowMenuItem). Переходы состояния - Tasks.StateTransition(атрибут, состояние).</summary>
            public static class Menu
            {
                /// <summary>(Menu) [К] NextTaskViewerContextMenuDirector.cs:27.</summary>
                public const string Container = "AUIDTaskListContextMenu";

                /// <summary>«Новое задание» (MenuItem) [К] NextTaskViewerContextMenuDirector.cs:57; показ: есть корневые типы заданий.</summary>
                [ShownWhen("есть корневые типы заданий")]
                public const string CreateTaskMenuItem = "miCreateTaskMenuItem";

                /// <summary>«Создать процесс» (MenuItem) [К] NextTaskViewerContextMenuDirector.cs:70; показ: есть корневые типы процессов.</summary>
                [ShownWhen("есть корневые типы процессов")]
                public const string CreateWorkflowMenuItem = "miCreateWorkflowMenuItem";

                /// <summary>«Создать подзадание» (MenuItem) [К] NextTaskViewerContextMenuDirector.cs:94; показ: выбрано 1 задание (не этап), есть типы подзаданий.</summary>
                [ShownWhen("выбрано 1 задание (не этап), есть типы подзаданий")]
                public const string CreateSubTask = "miCreateSubTask";

                /// <summary>«Создать подпроцесс» (MenuItem) [К] NextTaskViewerContextMenuDirector.cs:108; показ: выбрано 1 задание (не этап) и есть типы подпроцессов (:80-84, 104-108).</summary>
                [ShownWhen("выбрано 1 задание (не этап), есть типы подпроцессов")]
                public const string CreateSubWorkflow = "miCreateSubWorkflow";

                /// <summary>«Копировать ссылку» (MenuItem) [К] NextTaskViewerContextMenuDirector.cs:154; показ: выбрано 1 (не этап). Копирует ССЫЛКУ.</summary>
                [ShownWhen("выбран 1 элемент (не этап)")]
                public const string Copy = "miCopy";

                /// <summary>«Расположение элемента», Name пуст (MenuItem) [К] NextTaskViewerContextMenuDirector.cs:159; показ: выбранный элемент не задание и не процесс.</summary>
                [ShownWhen("выбранный элемент - не задание и не процесс")]
                public const string Navigate = "miNavigate";

                /// <summary>(MenuItem) [К] TemplateFiller.cs:80; показ: открыто подменю «Новое задание/Создать процесс», есть шаблоны. Заголовок группы «Шаблоны»; шаблоны - Explorer.TemplateMenuItem.</summary>
                [ShownWhen("открыто подменю «Новое задание/Создать процесс», есть шаблоны")]
                public const string Templates = "miTemplates";
            }

            /// <summary>Элементы строки задания (ListItem, Name = заголовок; tasknodetemplates.xaml:80-163). Искать внутри строки.</summary>
            public static class Row
            {
                /// <summary>Name пуст (Button) [Р] разведка; tasknodetemplates.xaml:80. Toggle.</summary>
                public const string Expander = "taskNodeExpander";

                /// <summary>Name пуст (Image) [Р] разведка; tasknodetemplates.xaml:81. HelpText = вид задания/процесса.</summary>
                public const string KindIcon = "AUIDImageKind";

                /// <summary>Name пуст (Image) [Р] разведка; tasknodetemplates.xaml:82. HelpText = состояние.</summary>
                public const string StatusIcon = "AUIDImageStatus";

                /// <summary>Name пуст (Image) [Р] разведка; tasknodetemplates.xaml:83; показ: у задания есть вложения.</summary>
                [ShownWhen("у задания есть вложения")]
                public const string HasAttachmentsIcon = "AUIDImageHasAttachments";

                /// <summary>(Text) [Р] разведка; tasknodetemplates.xaml:85.</summary>
                public const string Title = "titleTextBlock";

                /// <summary>(Text) [Р] разведка; tasknodetemplates.xaml:100; показ: строка задания (не процесса).</summary>
                [ShownWhen("строка задания (не процесса)")]
                public const string Workflow = "tbWorkflow";

                /// <summary>(Hyperlink) [Р] разведка; commnadpopuplinkcontrol.xaml:12; показ: строка задания (не процесса). В XAML строки x:Name executorCommandLink (tasknodetemplates.xaml:108), в UIA - commandPopupLink. При пользовательском состоянии в строке есть второй commandPopupLink (ссылка состояния, Name = заголовок состояния).</summary>
                [ShownWhen("строка задания (не процесса)")]
                public const string ExecutorLink = "commandPopupLink";

                /// <summary>(Text) [Р] разведка; tasknodetemplates.xaml:110; показ: строка задания (не процесса). Name = исполнитель.</summary>
                [ShownWhen("строка задания (не процесса)")]
                public const string ExecutorText = "showExecutorPopupLinkText";

                /// <summary>(Text) [Д] дамп прогона; tasknodetemplates.xaml:114; показ: у задания есть срок.</summary>
                [ShownWhen("у задания есть срок")]
                public const string Deadline = "tbDeadline";

                /// <summary>(Text) [Р] разведка; tasknodetemplates.xaml:140.</summary>
                public const string Type = "taskType";

                /// <summary>Name пуст (Image) [К] tasknodetemplates.xaml:163; userobjectstatecontrol.xaml:19; показ: у задания есть пользовательское состояние. Сам UserObjectStateControl (x:Name stateControl) в UIA не виден - Control без AutomationPeer (UserObjectStateControl.cs:9; разведка).</summary>
                [ShownWhen("у задания есть пользовательское состояние")]
                public const string StateIcon = "stateIcon";
            }

            /// <summary>Карточка выбранного задания справа (NextTaskViewer.xaml:111; TaskDetailsView.xaml:25; savecanceleditor.xaml). В разведке не открывалась. ContentControl taskDetailsPresenter и SaveCancelEditor detailsEditor в UIA не видны (нет AutomationPeer): корень - TaskDetailsView без id (ViewClass), кнопки Edit/Save/Cancel искать прямо внутри него.</summary>
            public static class Details
            {
                /// <summary>(Custom) [К] TaskDetailsView.xaml:2; viewtemplates.xaml:49. ClassName, НЕ AutomationId (CatalogAssert должен её пропускать). Корень карточки задания, id пустой; показ: выбрано задание.</summary>
                [ShownWhen("выбрано задание")]
                public const string ViewClass = "TaskDetailsView";

                /// <summary>(Button) [К] savecanceleditor.xaml:52; показ: выбрано задание, не режим правки.</summary>
                [ShownWhen("выбрано задание, не режим правки")]
                public const string Edit = "btnEdit";

                /// <summary>(Button) [К] savecanceleditor.xaml:30; показ: режим правки.</summary>
                [ShownWhen("режим правки")]
                public const string Save = "btnSave";

                /// <summary>(Button) [К] savecanceleditor.xaml:49; показ: режим правки.</summary>
                [ShownWhen("режим правки")]
                public const string Cancel = "btnCancel";

                /// <summary>(Custom) [К] datatemplates.xaml; показ: выбрано задание.</summary>
                [ShownWhen("выбрано задание")]
                public const string Card = "ascTaskCard";

                /// <summary>Name пуст (List) [К] datatemplates.xaml:21 -> attachmentstemplates.xaml:511 (WorkflowAttachmentsManagerTemplate -> AttachmentsListTemplate); показ: выбрано задание. Тот же id, что TaskEditor.Attachments (дамп прогона); ContentControl attachmentsContentControl в UIA не виден.</summary>
                [ShownWhen("выбрано задание")]
                public const string Attachments = "attachmentsItemControl";
            }
        }

        /// <summary>Окно нового задания (NextTaskDialogView.xaml): окно верхнего уровня, Name = PilotTitles.WindowNames.TaskEditor, id = заголовок («Новое задание - …», PilotTitles.Windows.NewTaskPrefix). Поля - Card.Field(attr), исполнитель - Card.OrgUnitInput(заголовок атрибута).</summary>
        public static class TaskEditor
        {
            /// <summary>Name пуст (Pane) [Д] дамп прогона; Ascon.Pilot.Client.TasksNext.Dialogs.NextTaskDialogView.xaml:25.</summary>
            public const string ContentScroll = "taskDialogContentScrollViewer";

            /// <summary>Name пуст (Custom) [Д] дамп прогона; Ascon.Pilot.Client.TasksNext.Dialogs.NextTaskDialogView.xaml:42.</summary>
            public const string Card = "ascTaskCard";

            /// <summary>Name пуст (List) [Д] дамп прогона; attachmentstemplates.xaml:511 (AttachmentsListTemplate через WorkflowAttachmentsManagerTemplate :557-559; NextTaskDialogView.xaml:43).</summary>
            public const string Attachments = "attachmentsItemControl";

            /// <summary>Name пуст (Button) [Д] дамп прогона; attachmentstemplates.xaml:344. Выпадающая кнопка.</summary>
            public const string AddAttachment = "addAttachmentButton";

            /// <summary>[К] attachmentstemplates.xaml:346; показ: раскрыта кнопка addAttachmentButton.</summary>
            [ShownWhen("раскрыта кнопка addAttachmentButton")]
            public const string AddObject = "addObjectButton";

            /// <summary>[К] attachmentstemplates.xaml:347; показ: раскрыта addAttachmentButton, AllowAddFiles.</summary>
            [ShownWhen("раскрыта addAttachmentButton, AllowAddFiles")]
            public const string AddFile = "addFileButton";

            /// <summary>[К] attachmentstemplates.xaml; показ: раскрыта addAttachmentButton, AllowAddFolders.</summary>
            [ShownWhen("раскрыта addAttachmentButton, AllowAddFolders")]
            public const string AddFolder = "addFolderButton";

            /// <summary>«Выдать задание» (Button) [Д] дамп прогона; okcancelbuttonspanel.xaml:9; NextTaskDialogView.xaml:47 (OkButtonCaption).</summary>
            public const string Ok = "btnOk";

            /// <summary>«Отмена» (Button) [Д] дамп прогона; okcancelbuttonspanel.xaml:11.</summary>
            public const string Cancel = "btnCancel";

            /// <summary>(Text) [Д] дамп прогона; Ascon.Pilot.Client.TasksNext.Dialogs.NextTaskDialogView.xaml:49.</summary>
            public const string SaveAsTemplateText = "saveAsTemplateTxb";

            /// <summary>«Сохранить как шаблон» (Hyperlink) [Д] дамп прогона; Ascon.Pilot.Client.TasksNext.Dialogs.NextTaskDialogView.xaml:50.</summary>
            public const string SaveAsTemplate = "saveAsTemplateLnk";

            /// <summary>Name пуст (Custom) [Д] дамп прогона; datetimeviewtemplate.xaml:59. Внутри Card.DateContainer("deadlineDate").</summary>
            public const string DeadlineEditor = "deadlineEditor";

            /// <summary>Name пуст (Edit) [Д] дамп прогона; datetimeviewtemplate.xaml:65. Срок в рабочих днях.</summary>
            public const string DeadlineDays = "deadlineDaysEditor";

            /// <summary>Name пуст (Edit) [Д] дамп прогона; datepicker.xaml:30. Поле даты внутри DeadlineEditor.</summary>
            public const string DeadlineDate = "PART_TextBox";

            /// <summary>(Text) [Д] дамп прогона; datetimeviewtemplate.xaml:64. «Рабочих дней».</summary>
            public const string CalendarDays = "CalendarDaysTbl";

            /// <summary>(Button) [Д] дамп прогона; datetimeviewtemplate.xaml:61. «Время».</summary>
            public const string ShowTime = "showTime";

            /// <summary>Name пуст (Custom) [Д] дамп прогона; datetimeviewtemplate.xaml:62.</summary>
            public const string TimePicker = "timePicker";
        }

        /// <summary>Окно процесса (WorkflowDialogView.xaml; stagetemplates.xaml): окно верхнего уровня, Name = PilotTitles.WindowNames.WorkflowEditor, id = заголовок («Новый процесс - …», PilotTitles.Windows.NewWorkflowPrefix).</summary>
        public static class WorkflowEditor
        {
            /// <summary>Name пуст (Pane) [Д] дамп прогона; Ascon.Pilot.Client.TasksNext.Dialogs.WorkflowDialogView.xaml:40.</summary>
            public const string Stages = "StagesScrollViewer";

            /// <summary>(Text) [Д] дамп прогона; cardcaptiontemplates.xaml:12. Name = тип процесса.</summary>
            public const string TypeName = "typeNameTextBlock";

            /// <summary>Name пуст (Custom) [Д] дамп прогона; Ascon.Pilot.Client.TasksNext.Dialogs.WorkflowDialogView.xaml:48.</summary>
            public const string WorkflowCard = "workflowCard";

            /// <summary>Name пуст (Custom) [Д] дамп прогона; Ascon.Pilot.Client.TasksNext.Dialogs.WorkflowDialogView.xaml:49.</summary>
            public const string StagesPanel = "AUIDStagesPanel";

            /// <summary>Name пуст (List) [Д] дамп прогона; stagetemplates.xaml:218. Список этапов: StageItem(тип, i).</summary>
            public const string StagesList = "executorsItemControl";

            /// <summary>(Text) [Д] дамп прогона; stagetemplates.xaml:170. «Этап N»; внутри StageItem.</summary>
            public const string StageCaption = "stageCaption";

            /// <summary>(Hyperlink) [Д] дамп прогона; stagetemplates.xaml:171. Name «Этап N».</summary>
            public const string ShowExecutors = "AUIDShowExecutorsLink";

            /// <summary>(Text) [Д] дамп прогона; stagetemplates.xaml:175.</summary>
            public const string StageDeadline = "stageDeadlineText";

            /// <summary>Name пуст (Button) [Д] дамп прогона; stagetemplates.xaml:177.</summary>
            public const string SelectAllStage = "selectAllStageBtn";

            /// <summary>Name пуст (Button) [Д] дамп прогона; stagetemplates.xaml:187.</summary>
            public const string DeleteStage = "deleteStageBtn";

            /// <summary>(Text) [Д] дамп прогона; stagetemplates.xaml:17. Внутри ExecutorItem; Name = «имя (должность)».</summary>
            public const string ExecutorTitle = "executorTitle";

            /// <summary>(Text) [Д] дамп прогона; stagetemplates.xaml:19.</summary>
            public const string ExecutorDeadline = "deadlineText";

            /// <summary>Name пуст (Image) [Д] дамп прогона; stagetemplates.xaml:51.</summary>
            public const string TaskKindIcon = "taskKindIconImage";

            /// <summary>(Button) [К] stagetemplates.xaml:56; показ: у этапа выбран исполнитель.</summary>
            [ShownWhen("у этапа выбран исполнитель")]
            public const string OrgUnitRemove = "removeOrgUnitItemButton";

            /// <summary>Name пуст (Button) [Д] дамп прогона; stagetemplates.xaml:58.</summary>
            public const string TaskTypeMenuButton = "taskTypeMenuButton";

            /// <summary>(Menu) [К] stagetemplates.xaml:61; показ: открыто меню taskTypeMenuButton.</summary>
            [ShownWhen("открыто меню taskTypeMenuButton")]
            public const string TaskTypeMenu = "TaskTypeMenu";

            /// <summary>(Text) [Д] дамп прогона; stagetemplates.xaml:228.</summary>
            public const string AddStageText = "addStageTbl";

            /// <summary>«Добавить этап» (Hyperlink) [Д] дамп прогона; stagetemplates.xaml:229.</summary>
            public const string AddStage = "addStageButton";

            /// <summary>Name пуст (Pane) [Д] дамп прогона; Ascon.Pilot.Client.TasksNext.Dialogs.WorkflowDialogView.xaml:55.</summary>
            public const string ExecutorDetails = "workflowExecutorDetailsScrollViewer";

            /// <summary>Name пуст (Custom) [Д] дамп прогона; Ascon.Pilot.Client.TasksNext.Dialogs.WorkflowDialogView.xaml:63. Поля - Card.Field(attr), например AUIDrole.</summary>
            public const string ExecutorsCard = "executorsCard";

            /// <summary>(Text) [К] Ascon.Pilot.Client.TasksNext.Dialogs.WorkflowDialogView.xaml:18.</summary>
            public const string SyncAttachmentsText = "syncAttachmentsText";

            /// <summary>(Hyperlink) [К] Ascon.Pilot.Client.TasksNext.Dialogs.WorkflowDialogView.xaml:19.</summary>
            public const string SyncAttachments = "syncAttachmentsLink";

            /// <summary>[К] Ascon.Pilot.Client.TasksNext.Dialogs.WorkflowDialogView.xaml:21.</summary>
            public const string ClearAttachments = "clearAttachmentsLink";

            /// <summary>«OK» (Button) [Д] дамп прогона; okcancelbuttonspanel.xaml:9; WorkflowDialogView.xaml:89 (OkButtonCaption).</summary>
            public const string Ok = "btnOk";

            /// <summary>«Отмена» (Button) [Д] дамп прогона; okcancelbuttonspanel.xaml:11.</summary>
            public const string Cancel = "btnCancel";

            /// <summary>«Сохранить как шаблон» (Text) [Д] дамп прогона; Ascon.Pilot.Client.TasksNext.Dialogs.WorkflowDialogView.xaml:91.</summary>
            public const string SaveAsTemplateText = "saveAsTemplateTxb";

            /// <summary>«Сохранить как шаблон» (Hyperlink) [Д] дамп прогона; Ascon.Pilot.Client.TasksNext.Dialogs.WorkflowDialogView.xaml:92.</summary>
            public const string SaveAsTemplate = "saveAsTemplateLnk";

            /// <summary>Этап: "AUIDStageItem:{имя типа этапа};Index:{i}". [Д] дамп прогона (AUIDStageItem:stage_signing;Index:0); [К] stagetemplates.xaml:150.</summary>
            public static string StageItem(string stageTypeName, int index) => "AUIDStageItem:" + stageTypeName + ";Index:" + index;

            /// <summary>Префикс id этапа. [Д] дамп прогона; [К] stagetemplates.xaml:150.</summary>
            public static readonly string StageItemPrefix = "AUIDStageItem:";

            /// <summary>Исполнитель этапа: "AUIDExecutorItem:{имя};Index:{i};Stage:{индекс этапа}". [Д] дамп прогона (AUIDExecutorItem:bot2;Index:0;Stage:0); [К] stagetemplates.xaml:121.</summary>
            public static string ExecutorItem(string personName, int index, int stageIndex) => "AUIDExecutorItem:" + personName + ";Index:" + index + ";Stage:" + stageIndex;

            /// <summary>Префикс id исполнителя этапа. [Д] дамп прогона; [К] stagetemplates.xaml:121.</summary>
            public static readonly string ExecutorItemPrefix = "AUIDExecutorItem:";

            /// <summary>Место для нового исполнителя этапа: "AUIDNewExecutorItem:{индекс этапа}". [Д] дамп прогона; [К] stagetemplates.xaml:139.</summary>
            public static string NewExecutorItem(int stageIndex) => "AUIDNewExecutorItem:" + stageIndex;

            /// <summary>Поле ввода исполнителя этапа = Card.OrgUnitInput(индекс этапа). [Д] дамп прогона (Stage:0); [К] stagetemplates.xaml:147.</summary>
            public static string ExecutorInput(int stageIndex) => Card.OrgUnitInput(stageIndex.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
    }
}
