namespace PilotUITesting.Client
{
    public static partial class PilotIds
    {
        /// <summary>Окно карточки объекта (создание и редактирование; ObjectDialogView.xaml) и общие элементы карточки атрибутов (ObjectCardView, CardEditorTemplates.xaml). Поле = "AUID" + ИМЯ атрибута, подпись = "attributeCaptionContainer:" + ИМЯ атрибута. Диалоги модулей - дочерние окна mainPilotView без id; ObjectCardView там без id, ClassName = ObjectCardView (разведка).</summary>
        public static class Card
        {
            /// <summary>(Window) [Д] дамп прогона; Ascon.Pilot.Client.ObjectCard.ObjectDialogView.xaml:2. Окно верхнего уровня; Name = PilotTitles.Windows.CreateCard/EditCard.</summary>
            public const string Container = "AUIDCreateObject";

            /// <summary>Name пуст (Custom) [Д] дамп прогона; Ascon.Pilot.Client.ObjectCard.ObjectDialogView.xaml:40.</summary>
            public const string ObjectCard = "ascObjectCard";

            /// <summary>«Создать» (Button) [К] Ascon.Pilot.Client.ObjectCard.ObjectDialogView.xaml:53; показ: окно создания.</summary>
            [ShownWhen("окно создания")]
            public const string Create = "btnCreate";

            /// <summary>«Создать скрытым» (Button) [К] Ascon.Pilot.Client.ObjectCard.ObjectDialogView.xaml:58; показ: окно создания, CanCreateSecretObject.</summary>
            [ShownWhen("окно создания, CanCreateSecretObject")]
            public const string CreateAsSecret = "btnCreateAsSecret";

            /// <summary>«Сохранить» (Button) [Д] дамп прогона; Ascon.Pilot.Client.ObjectCard.ObjectDialogView.xaml:76; показ: окно редактирования.</summary>
            [ShownWhen("окно редактирования")]
            public const string Save = "btnSave";

            /// <summary>«Отмена» (Button) [Д] дамп прогона; Ascon.Pilot.Client.ObjectCard.ObjectDialogView.xaml:54 (создание), :77 (редактирование).</summary>
            public const string Cancel = "btnCancel";

            /// <summary>(Edit) [К] Ascon.Pilot.Client.ObjectCard.ObjectDialogView.xaml:74; показ: редактирование, IsReasonVisible. Подсказка «Укажите причину замены».</summary>
            [ShownWhen("редактирование, IsReasonVisible")]
            public const string ReplaceReason = "reasonInput";

            /// <summary>«Открыть файл после создания» (CheckBox) [К] Ascon.Pilot.Client.ObjectCard.ObjectDialogView.xaml:45; показ: IsOpenFileAfterCreationVisible.</summary>
            [ShownWhen("IsOpenFileAfterCreationVisible")]
            public const string OpenAfterCreate = "OpenAfterCreateChbx";

            /// <summary>Name пуст (Custom) [Д] дамп прогона; Ascon.Pilot.Client.ObjectCard.ObjectDialogView.xaml:96; показ: карточка с просмотром документа (иначе offscreen). Панель окна карточки, не проводника.</summary>
            [ShownWhen("карточка с просмотром документа")]
            public const string Toolbar = "objectListViewerToolbar";

            /// <summary>«Выбрать файл» (Button) [К] ObjectDialogViewModel.cs:254; показ: карточка с просмотром документа.</summary>
            [ShownWhen("карточка с просмотром документа")]
            public const string OpenFile = "tbiOpenFile";

            /// <summary>(Thumb) [К] Ascon.Pilot.Client.ObjectCard.ObjectDialogView.xaml:98; показ: карточка с файлами документа.</summary>
            [ShownWhen("карточка с файлами документа")]
            public const string PreviewSplitter = "objectCardDocPreviewSplitter";

            /// <summary>Name пуст (Custom) [К] Ascon.Pilot.Client.DocumentExplorer.DocumentFilesView.DocumentFilesView.xaml:2; ObjectDialogView.xaml:99; показ: карточка с файлами документа. Тот же id, что Explorer.Files.Container (в проводнике [Р] разведка 001:744); x:Name documentFilesTab в UIA не попадает - явный AutomationId корня его перекрывает.</summary>
            [ShownWhen("карточка с файлами документа")]
            public const string DocumentFilesTab = "AUIDdocumentFilesView";

            /// <summary>Name пуст (Custom) [Д] дамп прогона; Ascon.Pilot.Client.DocumentExplorerView.xaml:125. Карточка атрибутов в проводнике и панелях; в диалоге модуля id пустой.</summary>
            public const string ObjectCardView = "ObjectCardView";

            /// <summary>Name пуст (Pane) [Р] разведка; Ascon.Pilot.ClientCore.ObjectCard.ObjectCardView.xaml:65.</summary>
            public const string ScrollViewer = "PART_ScrollViewer";

            /// <summary>Name пуст (Custom) [Р] разведка; Ascon.Pilot.ClientCore.ObjectCard.ObjectCardView.xaml:30.</summary>
            public const string AttributesGroup = "attributesGroup";

            /// <summary>Name пуст (Custom) [Р] разведка; attributecaptiontemplates.xaml:17. Внутри Caption(attr).</summary>
            public const string CaptionText = "attributeCaptionTxb";

            /// <summary>(Text) [Р] разведка; obligatorytextblock.xaml:23. Заголовок атрибута; внутри CaptionText.</summary>
            public const string CaptionTitle = "textTxb";

            /// <summary>Префикс контейнера подписи атрибута: «attributeCaptionContainer:» + ИМЯ атрибута (см. <see cref="Caption"/>). [Р] разведка; attributecaptiontemplates.xaml:11.</summary>
            public static readonly string CaptionPrefix = "attributeCaptionContainer:";

            /// <summary>Name пуст (Edit) [Т] поле ввода даты внутри dateTimeAttrContainer:&lt;атрибут&gt; (PilotTaskDialog.SetDeadline); [Д] дамп прогона окна задания.</summary>
            public const string DateTextBox = "PART_TextBox";

            /// <summary>(Text) [Р] разведка; obligatorytextblock.xaml:19. «*» - атрибут обязательный; видна и после заполнения (разведка).</summary>
            public const string ObligatoryMark = "obligatoryTxb";

            /// <summary>Name пуст (Button) [Р] разведка; buttonedit.xaml:88. «…»/справочник у поля ButtonEdit.</summary>
            public const string FieldRightButton = "PART_Right_Button";

            /// <summary>Name пуст (Button) [Д] дамп прогона; buttonedit.xaml:78.</summary>
            public const string FieldLeftButton = "PART_Left_Button";

            /// <summary>Name пуст (Button) [Р] разведка; datetimepicker.xaml:234. Кнопка календаря у поля даты.</summary>
            public const string CalendarToggle = "_calendarToggleButton";

            /// <summary>(CheckBox) [К] Ascon.Pilot.ClientCore.ObjectCard.Controls.CardEditorTemplates.xaml:307. ОДИН id на все логические атрибуты - искать внутри поля атрибута.</summary>
            public const string BoolCheckBox = "AUIDBoolAttrCheckbox";

            /// <summary>(Text) [К] Ascon.Pilot.ClientCore.ObjectCard.Controls.CardEditorTemplates.xaml:229; показ: раскрыт список справочного поля.</summary>
            [ShownWhen("раскрыт список справочного поля")]
            public const string RefBookItem = "AUIDRefBookItem";

            /// <summary>Name пуст (Button) [Р] разведка; combobox.xaml. Внутри NumeratorCombo(attr).</summary>
            public const string NumeratorDropDown = "toggleButton";

            /// <summary>(Text) [Р] разведка; Ascon.Pilot.ClientCore.ObjectCard.Numerator.NumeratorEditPresenter.xaml:11. Текст конфигурации нумератора; внутри NumeratorCombo(attr).</summary>
            public const string NumeratorText = "txbConfiguration";

            /// <summary>Name пуст (Edit) [Д] дамп прогона; timepicker.xaml:70; показ: ShowTimeEditor; скрыт при EnableDaysOnly+TimeSelected (datetimeviewtemplate.xaml:132-143).</summary>
            [ShownWhen("поле даты с выбором времени")]
            public const string TimeHours = "AUIDTimePickerHours";

            /// <summary>Name пуст (Edit) [Д] дамп прогона; timepicker.xaml:72; показ: ShowTimeEditor; скрыт при EnableDaysOnly+TimeSelected (datetimeviewtemplate.xaml:132-143).</summary>
            [ShownWhen("поле даты с выбором времени")]
            public const string TimeMinutes = "AUIDTimePickerMinutes";

            /// <summary>(Button) [Д] дамп прогона; Ascon.Pilot.ClientCore.ObjectCard.ObjectCardView.xaml:53; показ: у типа есть группы атрибутов. Name = заголовок группы.</summary>
            [ShownWhen("у типа есть группы атрибутов")]
            public const string GroupToggle = "groupToggleButton";

            /// <summary>Name пуст (Group) [Д] дамп прогона; Ascon.Pilot.ClientCore.ObjectCard.ObjectCardView.xaml:56; показ: у типа есть группы атрибутов.</summary>
            [ShownWhen("у типа есть группы атрибутов")]
            public const string GroupExpander = "groupExpander";

            /// <summary>(Text) [Д] дамп прогона; orgunititemtemplates.xaml:52; показ: в поле оргединиц выбрана единица. Name = «имя (должность)».</summary>
            [ShownWhen("в поле оргединиц выбрана единица")]
            public const string OrgUnitPresenter = "orgUnitPresenter";

            /// <summary>[К] orgunititemtemplates.xaml; показ: в поле оргединиц выбрана единица.</summary>
            [ShownWhen("в поле оргединиц выбрана единица")]
            public const string OrgUnitRemove = "removeOrgUnitItemButton";

            /// <summary>[К] autocompletebox.xaml; показ: есть подсказки ввода оргединицы.</summary>
            [ShownWhen("есть подсказки ввода оргединицы")]
            public const string AutocompletePopup = "PART_AutocompletePopup";

            /// <summary>[К] autocompletebox.xaml; показ: есть подсказки ввода оргединицы. Выбор - Down+Enter.</summary>
            [ShownWhen("есть подсказки ввода оргединицы")]
            public const string AutocompleteList = "autocompleteList";

            /// <summary>(Text) [Р] разведка; Ascon.Pilot.ClientCore.ObjectCard.ObjectCardView.xaml:75. «Создал(а) …».</summary>
            public const string CreatedBy = "createdByTxb";

            /// <summary>(Text) [Д] дамп прогона; Ascon.Pilot.ClientCore.ObjectCard.ObjectCardView.xaml:76. «Изменил(а) …».</summary>
            public const string ModifiedBy = "modifiedByTxb";

            /// <summary>Поле атрибута: "AUID" + ИМЯ атрибута в базе (не заголовок). [Р] разведка; [К] CardEditorTemplates.xaml:11-288.</summary>
            public static string Field(string attributeName) => "AUID" + attributeName;

            /// <summary>Подпись атрибута: "attributeCaptionContainer:" + ИМЯ атрибута. [Р] разведка; [К] attributecaptiontemplates.xaml:11.</summary>
            public static string Caption(string attributeName) => "attributeCaptionContainer:" + attributeName;

            /// <summary>Контейнер даты: "dateTimeAttrContainer:" + имя атрибута (у срока задания - HostId, TaskCardEditorTemplates.xaml:9). [Д] дамп прогона (deadlineDate); [К] CardEditorTemplates.xaml:33.</summary>
            public static string DateContainer(string attributeName) => "dateTimeAttrContainer:" + attributeName;

            /// <summary>Список нумератора: "AUID" + имя + "Combo". [Р] разведка; [К] NumeratorEditPresenter.xaml:51.</summary>
            public static string NumeratorCombo(string attributeName) => "AUID" + attributeName + "Combo";

            /// <summary>Кнопка правки нумератора: "AUID" + имя + "EditButton". [Р] разведка; [К] CardEditorTemplates.xaml:144.</summary>
            public static string NumeratorEditButton(string attributeName) => "AUID" + attributeName + "EditButton";

            /// <summary>Поле ввода оргединицы: "AUIDNewOrgUnitAutocomplete;Stage:" + stageName. В карточке stageName = ЗАГОЛОВОК атрибута, в этапе процесса - индекс этапа. [Р] разведка; [К] orgunititemtemplates.xaml:11.</summary>
            public static string OrgUnitInput(string stageName) => "AUIDNewOrgUnitAutocomplete;Stage:" + stageName;

            /// <summary>Пустое поле оргединиц: "AUIDEmptyOrgUnitPlaceholder;Stage:" + stageName (заголовок атрибута). [Р] разведка; [К] orgunititemtemplates.xaml:24.</summary>
            public static string OrgUnitPlaceholder(string stageName) => "AUIDEmptyOrgUnitPlaceholder;Stage:" + stageName;

            /// <summary>Выбранная оргединица: "AUIDOrgUnitItem:{имя};Index:{i};Stage:{stageName}". [Д] дамп прогона (AUIDOrgUnitItem:bot;Index:0;Stage:Исполнитель); [К] orgunititemtemplates.xaml:41.</summary>
            public static string OrgUnitItem(string personName, int index, string stageName) => "AUIDOrgUnitItem:" + personName + ";Index:" + index + ";Stage:" + stageName;

            /// <summary>Префикс id выбранной оргединицы (для поиска по началу id). [Д] дамп прогона; [К] orgunititemtemplates.xaml:41.</summary>
            public static readonly string OrgUnitItemPrefix = "AUIDOrgUnitItem:";

            /// <summary>Окно справочника (ReferenceBookView.xaml:30-33): DialogWindow, дочернее окно mainPilotView, id = заголовок (PilotTitles.Windows.ReferenceBook/ElementBook или options.Title). id самого дерева (referenceBookTreeView или treeView) не проверен - искать Tree.</summary>
            public static class ReferenceBook
            {
                /// <summary>(Edit) [К] Ascon.Pilot.Client.Dialogs.ReferenceBook.ReferenceBookView.xaml:30. ButtonEdit: Enter или FieldRightButton - поиск.</summary>
                public const string Search = "searchEdit";

                /// <summary>(Button) [К] buttonedit.xaml:88.</summary>
                public const string SearchButton = "PART_Right_Button";

                /// <summary>(Custom) [К] Ascon.Pilot.VisualElements.Controls.TreeViewControl.xaml:2.</summary>
                public const string TreeControl = "treeViewControl";

                /// <summary>(List) [К] Ascon.Pilot.VisualElements.Controls.TreeViewControl.xaml:153.</summary>
                public const string Tree = "ascTreeView";

                /// <summary>(Button) [К] Ascon.Pilot.VisualElements.Controls.TreeViewControl.xaml:23.</summary>
                public const string Expander = "expander";

                /// <summary>(CheckBox) [К] Ascon.Pilot.VisualElements.Controls.TreeViewControl.xaml:30; показ: справочник с выбором флажками.</summary>
                [ShownWhen("справочник с выбором флажками")]
                public const string CheckBox = "checkBox";

                /// <summary>(Text) [К] objectnodetemplate.xaml:52.</summary>
                public const string ItemTitle = "titleTextBlock";

                /// <summary>«OK» (Button) [К] okcancelbuttonspanel.xaml:9.</summary>
                public const string Ok = "btnOk";

                /// <summary>«Отмена» (Button) [К] okcancelbuttonspanel.xaml:11.</summary>
                public const string Cancel = "btnCancel";
            }

            /// <summary>Окно «Организационная структура» (OrgStructureView.xaml; выбор подписантов - PositionsSelectorView.xaml): id = Name = заголовок (PilotTitles.Windows.OrgStructure или options.Title).</summary>
            public static class OrgStructure
            {
                /// <summary>(Custom) [К] Ascon.Pilot.Client.SharingSettings.OrgStructure.OrgStructureView.xaml:31.</summary>
                public const string TreeControl = "treeViewControl";

                /// <summary>(List) [К] Ascon.Pilot.VisualElements.Controls.TreeViewControl.xaml:153.</summary>
                public const string Tree = "ascTreeView";

                /// <summary>«Добавить» (Button) [К] Ascon.Pilot.Client.SharingSettings.OrgStructure.OrgStructureView.xaml:53. Подпись меняется через OkButtonCaption.</summary>
                public const string Add = "btnAdd";

                /// <summary>«Отмена» (Button) [К] Ascon.Pilot.Client.SharingSettings.OrgStructure.OrgStructureView.xaml:54.</summary>
                public const string Cancel = "btnCancel";

                /// <summary>[К] Ascon.Pilot.Client.Dialogs.PositionsSelector.PositionsSelectorView.xaml:77; показ: окно выбора должностей.</summary>
                [ShownWhen("окно выбора должностей")]
                public const string EmptySelectionHint = "AUIDEmptySelectionHint";

                /// <summary>(Pane) [К] Ascon.Pilot.Client.Dialogs.PositionsSelector.PositionsSelectorView.xaml:79; показ: окно выбора должностей.</summary>
                [ShownWhen("окно выбора должностей")]
                public const string SelectedPositions = "AUIDSelectedPositions";

                /// <summary>(Text) [К] Ascon.Pilot.Client.Dialogs.PositionsSelector.PositionsSelectorView.xaml:84; показ: окно выбора должностей.</summary>
                [ShownWhen("окно выбора должностей")]
                public const string InfoMessage = "AUIDInfoMessage";

                /// <summary>Кнопка «выбрать все дочерние» в окне выбора должностей: "AUIDSelectAllChildrenButton_" + заголовок узла. [К] PositionsSelectorView.xaml:27.</summary>
                public static string SelectAllChildrenButton(string title) => "AUIDSelectAllChildrenButton_" + title;
            }

            /// <summary>Окно выбора документов (SDK ShowDocumentsSelectorDialog; ObjectsExplorerView.xaml:15-27): id = options.Title, Name = PilotTitles.WindowNames.ObjectSelector.</summary>
            public static class ObjectSelector
            {
                /// <summary>[К] Ascon.Pilot.Client.Dialogs.ObjectsExplorer.ObjectsExplorerView.xaml:15.</summary>
                public const string ListViewer = "objectListViewer";

                /// <summary>(List) [К] Ascon.Pilot.Client.Controls.ObjectsList.xaml:18. = Explorer.ListViewOf(ListViewer).</summary>
                public const string List = "objectListViewer.ListView";

                /// <summary>(Text) [К] Ascon.Pilot.Client.Dialogs.ObjectsExplorer.ObjectsExplorerView.xaml:20; показ: есть сообщение.</summary>
                [ShownWhen("есть сообщение")]
                public const string InfoMessage = "AUIDInfoMessage";

                /// <summary>«Вложить» (Button) [К] okcancelbuttonspanel.xaml:9.</summary>
                public const string Ok = "btnOk";

                /// <summary>«Отмена» (Button) [К] okcancelbuttonspanel.xaml:11.</summary>
                public const string Cancel = "btnCancel";
            }

            /// <summary>Окно выбора шаблона документа (documenttemplateselectortemplates.xaml:47-48): Name = PilotTitles.WindowNames.DocumentTemplateSelector; у пунктов шаблонов id нет, Name начинается с PilotTitles.WindowNames.DocumentTemplateItemPrefix.</summary>
            public static class DocumentTemplateSelector
            {
                /// <summary>(Button) [К] documenttemplateselectortemplates.xaml:47.</summary>
                public const string Skip = "btnSkip";

                /// <summary>(Button) [К] documenttemplateselectortemplates.xaml:48.</summary>
                public const string Cancel = "btnCancel";
            }

            /// <summary>Контекстное меню вложений и связей (AttachmentsContextMenuDirector.cs:23-130): карточка задания, вкладка «Связи». Пункты модулей - IMenu&lt;AttachmentManagerContext&gt;.</summary>
            public static class Attachments
            {
                /// <summary>(Menu) [К] AttachmentsContextMenuDirector.cs:23.</summary>
                public const string Container = "AUIDAttachmentsMenu";

                /// <summary>«Документ», Name пуст (MenuItem) [К] AttachmentsContextMenuDirector.cs:53; показ: нет выделения, не только чтение.</summary>
                [ShownWhen("нет выделения, список не только для чтения")]
                public const string AddObjectAttachments = "miAddObjectAttachments";

                /// <summary>«Файл», Name пуст (MenuItem) [К] AttachmentsContextMenuDirector.cs:54; показ: нет выделения, не только чтение.</summary>
                [ShownWhen("нет выделения, список не только для чтения")]
                public const string AddFileAttachments = "miAddFileAttachments";

                /// <summary>«Папка», Name пуст (MenuItem) [К] AttachmentsContextMenuDirector.cs:55; показ: нет выделения, не только чтение.</summary>
                [ShownWhen("нет выделения, список не только для чтения")]
                public const string AddFolderAttachments = "miAddFolderAttachments";

                /// <summary>«Сохранить как», Name пуст (MenuItem) [К] AttachmentsContextMenuDirector.cs:64; показ: выбран 1 внешний файл.</summary>
                [ShownWhen("выбран 1 внешний файл")]
                public const string SaveAsAttachment = "miSaveAsAttachment";

                /// <summary>«Копировать ссылку», Name пуст (MenuItem) [К] AttachmentsContextMenuDirector.cs:65; показ: выбран 1 внешний файл.</summary>
                [ShownWhen("выбран 1 внешний файл")]
                public const string CopyAttachmentLink = "miCopyAttachmentLink";

                /// <summary>«Расположение элемента», Name пуст (MenuItem) [К] AttachmentsContextMenuDirector.cs:90; показ: выбрано ровно 1 вложение (:88), в карточке задания - объект.</summary>
                [ShownWhen("выбрано 1 вложение-объект")]
                public const string OpenAttachment = "miOpenAttachment";

                /// <summary>«Копировать ссылку», Name пуст (MenuItem) [К] AttachmentsContextMenuDirector.cs:98; вызывается только из LinkedObjectsContextMenuDirector.cs:31 (в меню вложений задания пункта нет, AttachmentsContextMenuDirector.cs:46-75).</summary>
                [ShownWhen("вкладка «Связи», есть выделение")]
                public const string CopyAttachment = "miCopyAttachment";

                /// <summary>«Карточка», Name пуст (MenuItem) [К] AttachmentsContextMenuDirector.cs:104; показ: выбраны вложения-объекты.</summary>
                [ShownWhen("выбраны вложения-объекты")]
                public const string ShowProperties = "miShowProperties";

                /// <summary>«Совместный доступ...», Name пуст (MenuItem) [К] AttachmentsContextMenuDirector.cs:105; показ: выбраны вложения-объекты.</summary>
                [ShownWhen("выбраны вложения-объекты")]
                public const string ShowSharingSettings = "miShowSharingSettings";

                /// <summary>«Пакетная печать выбранных элементов», Name пуст (MenuItem) [К] AttachmentsContextMenuDirector.cs:113; показ: выбраны вложения-объекты. Опечатка клиента в id («Elemets»).</summary>
                [ShownWhen("выбраны вложения-объекты")]
                public const string BatchPrintingSelectedElements = "miBatchPrintingSelectedElemets";

                /// <summary>«Актуализировать», Name пуст (MenuItem) [К] AttachmentsContextMenuDirector.cs:130; показ: только меню вложений задания (:72; LinkedObjectsContextMenuDirector.cs:30-34 его не добавляет), выбрано 1 вложение-объект и CanActualizeAllAttachments (:128).</summary>
                [ShownWhen("вложения задания: выбрано 1 вложение-объект, CanActualizeAllAttachments")]
                public const string ActualizeAttachment = "miActualizeAttachment";

                /// <summary>«Удалить из списка», Name пуст (MenuItem) [К] AttachmentsContextMenuDirector.cs:122; показ: есть выделение (в карточке задания - без запрещённых вложений, :58), список не только для чтения (:117-123); включая внешние файлы.</summary>
                [ShownWhen("есть выделение, список не только для чтения")]
                public const string RemoveAttachments = "miRemoveAttachments";
            }
        }
    }
}
