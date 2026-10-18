namespace PilotUITesting.Client
{
    public static partial class PilotIds
    {
        /// <summary>Просмотрщик XPS/PDF (XpsPreview.xaml, XpsRenderView.xaml). В проводнике - внутри Explorer.Preview.</summary>
        public static class Viewer
        {
            /// <summary>Name пуст (Custom) [Р] разведка; Ascon.Pilot.XpsRender.Viewer.XpsPreview.xaml:2.</summary>
            public const string Container = "AUIDXpsPreview";

            /// <summary>Name пуст (Custom) [Р] разведка; Ascon.Pilot.XpsRender.Viewer.XpsPreview.xaml:21.</summary>
            public const string Render = "xpsRenderView";

            /// <summary>Name пуст (Document) [Р] разведка; Ascon.Pilot.XpsRender.Viewer.XpsRenderView.xaml:209. ItemStatus «IsDocumentLoaded:{bool}» - признак готовности (разведка).</summary>
            public const string Document = "xMainDoc";

            /// <summary>«Полноэкранный режим просмотра», Name пуст (Button) [Р] разведка; ToolbarCommands.cs:88; показ: IsFullScreenModeAllowed. Отдельная кнопка справа от панели, НЕ внутри AUIDRenderToolbar; F11.</summary>
            [ShownWhen("полноэкранный режим разрешён")]
            public const string FullScreen = "fullScreenToolbarButton";

            /// <summary>(ComboBox) [К] Ascon.Pilot.XpsRender.Viewer.XpsRenderView.xaml:85; показ: по умолчанию Visibility=Hidden (XpsRenderView.xaml:85).</summary>
            [ShownWhen("список версий показан")]
            public const string Versions = "VersionsCombobox";

            /// <summary>Name пуст (ToolBar) [Р] разведка; Ascon.Pilot.XpsRender.Viewer.XpsRenderView.xaml:78. Контейнер панели поиска по документу, внутри - FindToolbar. Grid-родитель xSearchBox (AutomationId «SearchToolbox», :63) в UIA не виден: toolBar - прямой потомок xMainDoc (001:669; 036:70); Grid скрыт в режимах создания/правки и презентации (IsSimplifiedModeConverter).</summary>
            public const string SearchBar = "toolBar";

            /// <summary>Name пуст (ToolBar) [Р] разведка; FindToolbarStyler.cs:22. Панель поиска DocumentViewer.</summary>
            public const string FindToolbar = "FindToolbar";

            /// <summary>Name пуст (Edit) [Р] разведка; FindToolbarStyler.cs:28.</summary>
            public const string FindText = "FindTextBox";

            /// <summary>Name пуст (Button) [Р] разведка; FindToolbarStyler.cs:40.</summary>
            public const string FindNext = "FindNextButton";

            /// <summary>Name пуст (Button) [Р] разведка; FindToolbarStyler.cs:34.</summary>
            public const string FindPrevious = "FindPreviousButton";

            /// <summary>(Text) [К] Ascon.Pilot.XpsRender.Viewer.XpsRenderView.xaml:230; показ: документ загружается. Признак загрузки (StackPanel xBusyPanel, :221, в UIA не виден - Panel без AutomationPeer); ещё признак - ItemStatus IsDocumentLoaded у Document.</summary>
            [ShownWhen("документ загружается")]
            public const string BusyText = "busyText";

            /// <summary>(Text) [К] Ascon.Pilot.XpsRender.Viewer.XpsRenderView.xaml:242; показ: ошибка загрузки. StackPanel xErrorPanel (:235) в UIA не виден.</summary>
            [ShownWhen("ошибка загрузки")]
            public const string ErrorText = "errorText";

            /// <summary>(Text) [К] Ascon.Pilot.XpsRender.Viewer.XpsRenderView.xaml:62; показ: у документа нет XPS/PDF.</summary>
            [ShownWhen("у документа нет XPS/PDF")]
            public const string NoXps = "xNoXPS";

            /// <summary>Name пуст (Tab) [Р] разведка; Ascon.Pilot.XpsRender.Viewer.XpsPreview.xaml:31. Вкладки по Name (PilotTitles.Viewer.RemarksTab); HelpText «IsExpanded:{bool}».</summary>
            public const string SideTabs = "xpsViewerTabControl";

            /// <summary>Name пуст (Thumb) [Р] разведка; Ascon.Pilot.XpsRender.Viewer.XpsPreview.xaml:30.</summary>
            public const string Splitter = "xpsPreviewSplitter";

            /// <summary>(Menu) [К] Ascon.Pilot.XpsRender.Controls.GraphicLayerElementControl.xaml:65; показ: открыто меню графического слоя.</summary>
            [ShownWhen("открыто меню графического слоя")]
            public const string GraphicLayerMenu = "xGraphicLayerContextMenu";

            /// <summary>(Window) [К] XpsRenderViewModel.cs:1471; показ: режим презентации.</summary>
            [ShownWhen("режим презентации")]
            public const string PresentationWindow = "AUIDPresentationView";

            /// <summary>Панель просмотрщика (ToolbarManager.cs; порядок - ToolbarCommands.GetCommandsOrder). Поле номера страницы - ToolbarStripe.InputBox. Кнопки модулей (IToolbar&lt;XpsRenderContext&gt;) - в конце.</summary>
            public static class Toolbar
            {
                /// <summary>Name пуст (Custom) [Р] разведка; Ascon.Pilot.XpsRender.Viewer.XpsRenderView.xaml:155.</summary>
                public const string Container = "AUIDRenderToolbar";

                /// <summary>«Увеличить масштаб», Name пуст (Button) [Р] разведка; ToolbarManager.cs:200. Ctrl+"+".</summary>
                public const string ZoomIn = "zoomInToolbarButton";

                /// <summary>«Уменьшить масштаб», Name пуст (Button) [Р] разведка; ToolbarManager.cs:201. Ctrl+"-".</summary>
                public const string ZoomOut = "zoomOutToolbarButton";

                /// <summary>«Масштаб 1:1», Name пуст (Button) [Р] разведка; ToolbarManager.cs:202. Ctrl+"1".</summary>
                public const string SetActualSize = "setActualSizeToolbarButton";

                /// <summary>«Выравнивание по ширине окна», Name пуст (Button) [Р] разведка; ToolbarManager.cs:203.</summary>
                public const string FitToPage = "fitToPageToolbarButton";

                /// <summary>«Выравнивание по высоте окна», Name пуст (Button) [Р] разведка; ToolbarManager.cs:204.</summary>
                public const string FitToPageHeight = "fitToPageHeightToolbarButton";

                /// <summary>«Печать», Name пуст (Button) [Р] разведка; ToolbarManager.cs:217; показ: не для XPS-печати (IsXpsPrintPreview). Ctrl+P.</summary>
                [ShownWhen("не режим печати XPS")]
                public const string Print = "printToolbarButton";

                /// <summary>«Режим презентации», Name пуст (Button) [Р] разведка; ToolbarManager.cs:218; показ: не для XPS-печати. F5.</summary>
                [ShownWhen("не режим печати XPS")]
                public const string PresentationMode = "presentationModeButton";

                /// <summary>«Режим выбора», Name пуст (Button) [Р] разведка; ToolbarManager.cs:270. Переключатель; ItemStatus IsChecked.</summary>
                public const string SelectNotes = "selectNotesToolbarButton";

                /// <summary>«Измерить длину», Name пуст (Button) [Р] разведка; ToolbarManager.cs:273; показ: есть calculationCommands (AreaCalculationManager.cs:42-47). Переключатель.</summary>
                [ShownWhen("есть команды измерения")]
                public const string LengthCalculation = "lengthCalculationToolbarButton";

                /// <summary>«Вычислить площадь», Name пуст (Button) [Р] разведка; ToolbarManager.cs:274; показ: (AreaCalculationManager.cs:36-41). Переключатель.</summary>
                [ShownWhen("есть команды измерения")]
                public const string AreaCalculation = "areaCalculationToolbarButton";

                /// <summary>«Отображать замечания», Name пуст (Button) [Р] разведка; ToolbarManager.cs:277. Переключатель «Отображать замечания».</summary>
                public const string Annotations = "miAnnotations";

                /// <summary>«Режим отображения» (Button) [К] ToolbarManager.cs:566-567; показ: SupportsRenderMode. Подпись без текста на кнопке, но Name = «Режим отображения» (кнопка-меню WithHeader + WithShowCaption(false), как tbiRemarksMenuButton_doc_remark 001:479 и tbiAnnotationsSortButtonAscending 001:705).</summary>
                [ShownWhen("документ поддерживает режимы отображения")]
                public const string OptimizeVisualization = "miOptimizeVisualization";

                /// <summary>«Без оптимизации» (MenuItem) [К] ToolbarManager.cs:571; показ: открыто подменю «Режим отображения».</summary>
                [ShownWhen("открыто подменю «Режим отображения»")]
                public const string RegularRenderMode = "miRegularRenderMode";

                /// <summary>«Оптимизация геометрии» (MenuItem) [К] ToolbarManager.cs:574; показ: открыто подменю «Режим отображения».</summary>
                [ShownWhen("открыто подменю «Режим отображения»")]
                public const string OptimizedRenderMode = "miOptimizedRenderMode";

                /// <summary>«Оптимизация геометрии и текста» (MenuItem) [К] ToolbarManager.cs:577; показ: открыто подменю «Режим отображения».</summary>
                [ShownWhen("открыто подменю «Режим отображения»")]
                public const string TurboRenderMode = "miTurboRenderMode";

                /// <summary>«Отображать штампы», Name пуст (Button) [К] ToolbarManager.cs:286; показ: нет RenderMode, но есть PDF-штампы.</summary>
                [ShownWhen("нет режимов отображения, есть штампы PDF")]
                public const string DisplayStamps = "tbiDisplayStamps";

                /// <summary>«Предыдущая страница», Name пуст (Button) [К] ToolbarManager.cs:420; показ: страниц &gt; 1 (PostLoadedInit). Page Up.</summary>
                [ShownWhen("страниц больше одной")]
                public const string GoToPreviousPage = "GoToPreviousPageButton";

                /// <summary>«Следующая страница», Name пуст (Button) [К] ToolbarManager.cs:421; показ: страниц &gt; 1. Page Down.</summary>
                [ShownWhen("страниц больше одной")]
                public const string GoToNextPage = "GoToNextPageButton";

                /// <summary>«Добавить линию», Name пуст (Button) [К] ToolbarManager.cs:343; показ: вид замечания разрешён типом; если тип замечаний один - кнопка прямо на панели без подписи, иначе пункт подменю tbiRemarksMenuButton_{тип} с подписью (ToolbarManager.cs:307-335).</summary>
                [ShownWhen("вид замечания разрешён типом")]
                public const string AddLineAnnotation = "tbiAddLineAnnotation";

                /// <summary>«Добавить фигуру», Name пуст (Button) [К] ToolbarManager.cs:348; показ: вид замечания разрешён типом; если тип замечаний один - кнопка прямо на панели без подписи, иначе пункт подменю tbiRemarksMenuButton_{тип} с подписью (ToolbarManager.cs:307-335).</summary>
                [ShownWhen("вид замечания разрешён типом")]
                public const string AddFigureAnnotation = "tbiAddFigureAnnotation";

                /// <summary>«Добавить графическое замечание», Name пуст (Button) [К] ToolbarManager.cs:353; показ: вид замечания разрешён типом; если тип замечаний один - кнопка прямо на панели без подписи, иначе пункт подменю tbiRemarksMenuButton_{тип} с подписью (ToolbarManager.cs:307-335).</summary>
                [ShownWhen("вид замечания разрешён типом")]
                public const string AddGraphicAnnotation = "tbiAddGraphicAnnotation";

                /// <summary>«Добавить точечное замечание», Name пуст (Button) [К] ToolbarManager.cs:358; показ: вид замечания разрешён типом; если тип замечаний один - кнопка прямо на панели без подписи, иначе пункт подменю tbiRemarksMenuButton_{тип} с подписью (ToolbarManager.cs:307-335).</summary>
                [ShownWhen("вид замечания разрешён типом")]
                public const string AddAnnotationForPoint = "tbiAddAnnotationForPoint";

                /// <summary>«Добавить замечание к тексту», Name пуст (Button) [К] ToolbarManager.cs:363; показ: вид замечания разрешён типом; если тип замечаний один - кнопка прямо на панели без подписи, иначе пункт подменю tbiRemarksMenuButton_{тип} с подписью (ToolbarManager.cs:307-335).</summary>
                [ShownWhen("вид замечания разрешён типом")]
                public const string AddAnnotationForText = "tbiAddAnnotationForText";

                /// <summary>«Режим геометрия и текст», Name пуст (Button) [К] XpsRenderViewModel.cs:1944; показ: режим сравнения документов.</summary>
                [ShownWhen("режим сравнения документов")]
                public const string TextAndGeometryCompare = "textAndGeometryCompareToggleButton";

                /// <summary>«Режим текста», Name пуст (Button) [К] XpsRenderViewModel.cs:1946; показ: режим сравнения документов.</summary>
                [ShownWhen("режим сравнения документов")]
                public const string TextCompare = "textCompareToggleButton";

                /// <summary>«Режим наложения», Name пуст (Button) [К] XpsRenderViewModel.cs:1947; показ: режим сравнения документов.</summary>
                [ShownWhen("режим сравнения документов")]
                public const string OverlayCompare = "overlayCompareToggleButton";

                /// <summary>«Ручной режим», Name пуст (Button) [К] XpsRenderViewModel.cs:1948; показ: режим сравнения документов.</summary>
                [ShownWhen("режим сравнения документов")]
                public const string ManualCompare = "manualCompareToggleButton";

                /// <summary>«Сравнение выбранных страниц», Name пуст (Button) [К] XpsRenderViewModel.cs:1949; показ: режим сравнения документов.</summary>
                [ShownWhen("режим сравнения документов")]
                public const string CompareByPages = "compareByPages";

                /// <summary>«Предыдущее различие», Name пуст (Button) [К] XpsRenderViewModel.cs:1965; показ: режим сравнения документов.</summary>
                [ShownWhen("режим сравнения документов")]
                public const string PreviousDifference = "previousDifference";

                /// <summary>«Следующее различие», Name пуст (Button) [К] XpsRenderViewModel.cs:1966; показ: режим сравнения документов.</summary>
                [ShownWhen("режим сравнения документов")]
                public const string NextDifference = "nextDifference";

                /// <summary>«Сравнение области», Name пуст (Button) [К] XpsRenderViewModel.cs:1905; показ: режим сравнения документов.</summary>
                [ShownWhen("режим сравнения документов")]
                public const string DrawAreaCompare = "drawAreaCompareButton";

                /// <summary>«Выбрать цвет подсветки различий при сравнении», Name пуст (Button) [К] XpsRenderViewModel.cs:1974; показ: режим сравнения.</summary>
                [ShownWhen("режим сравнения документов")]
                public const string PickDiffColor = "pickDiffColor";

                /// <summary>«Навигация по проекту», Name пуст (Button) [К] FullscreenContextStore.cs:282; показ: только полноэкранный просмотр (DocumentContainerFactory.cs:57-60).</summary>
                [ShownWhen("полноэкранный просмотр")]
                public const string Navigation = "navigationToolbarButton";

                /// <summary>«Предыдущий документ», Name пуст (Button) [К] FullscreenContextStore.cs:286; показ: только полноэкранный просмотр (DocumentContainerFactory.cs:57-60).</summary>
                [ShownWhen("полноэкранный просмотр")]
                public const string NavigateLeft = "navigateLeftToolbarButton";

                /// <summary>«Следующий документ», Name пуст (Button) [К] FullscreenContextStore.cs:290; показ: только полноэкранный просмотр (DocumentContainerFactory.cs:57-60).</summary>
                [ShownWhen("полноэкранный просмотр")]
                public const string NavigateRight = "navigateRightToolbarButton";

                /// <summary>Кнопка-меню типа замечаний: "tbiRemarksMenuButton_" + имя типа БЕЗ преобразований (есть при нескольких типах замечаний). [Р] разведка; [К] ToolbarManager.cs:552.</summary>
                public static string RemarksMenuButton(string remarkTypeName) => "tbiRemarksMenuButton_" + remarkTypeName;
            }

            /// <summary>Контекстное меню страницы (XpsRenderView.xaml:214; XpsRenderViewModel.cs:873-970). Пункты модулей (IMenu&lt;XpsRenderClickPointContext&gt;) - в конце.</summary>
            public static class PageMenu
            {
                /// <summary>(Menu) [К] Ascon.Pilot.XpsRender.Viewer.XpsRenderView.xaml:214.</summary>
                public const string Container = "xContextMenu";

                /// <summary>«Копировать» (MenuItem) [К] XpsRenderViewModel.cs:882.</summary>
                public const string Copy = "miCopy";

                /// <summary>«Добавить линию» (MenuItem) [К] XpsRenderViewModel.cs:933; показ: можно добавлять замечания, вид Line. При нескольких типах замечаний - в подменю RemarkItem(тип).</summary>
                [ShownWhen("можно добавлять замечания вида «линия»")]
                public const string AddLineAnnotation = "miAddLineAnnotation";

                /// <summary>«Добавить графическое замечание» (MenuItem) [К] XpsRenderViewModel.cs:939; показ: вид Pencil.</summary>
                [ShownWhen("можно добавлять замечания вида «карандаш»")]
                public const string AddGraphicAnnotation = "miAddGraphicAnnotation";

                /// <summary>«Добавить фигуру» (MenuItem) [К] XpsRenderViewModel.cs:945; показ: вид Figure.</summary>
                [ShownWhen("можно добавлять замечания вида «фигура»")]
                public const string AddFigureAnnotation = "miAddFigureAnnotation";

                /// <summary>«Добавить замечание к тексту» (MenuItem) [К] XpsRenderViewModel.cs:951; показ: вид TextStickyNote.</summary>
                [ShownWhen("можно добавлять замечания к тексту")]
                public const string AddAnnotationForText = "miAddAnnotationForText";

                /// <summary>«Добавить точечное замечание» (MenuItem) [К] XpsRenderViewModel.cs:957; показ: вид PointStickyNote.</summary>
                [ShownWhen("можно добавлять точечные замечания")]
                public const string AddAnnotationForPoint = "miAddAnnotationForPoint";

                /// <summary>«Повернуть страницу по часовой» (MenuItem) [К] XpsRenderViewModel.cs:915.</summary>
                public const string RotateCW = "miRotateCW";

                /// <summary>«Повернуть страницу против часовой» (MenuItem) [К] XpsRenderViewModel.cs:916.</summary>
                public const string RotateCCW = "miRotateCCW";

                /// <summary>«Полноэкранный режим просмотра» (MenuItem) [К] XpsRenderViewModel.cs:920; показ: IsFullScreenModeAllowed.</summary>
                [ShownWhen("полноэкранный режим разрешён")]
                public const string Fullscreen = "miFullscreen";

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

                /// <summary>«Сравнить с...» (MenuItem) [К] ModelComparator.cs:139; показ: режим сравнения не активен.</summary>
                [ShownWhen("режим сравнения выключен")]
                public const string CompareWith = "miCompareWith";

                /// <summary>«Режим сравнения», Name пуст (MenuItem) [К] ModelComparator.cs:142; показ: открыто подменю «Сравнить с...».</summary>
                [ShownWhen("открыто подменю «Сравнить с...»")]
                public const string CompareMode = "miCompareMode";

                /// <summary>«Выбрать другой документ...», Name пуст (MenuItem) [К] ModelComparator.cs:145; показ: открыто подменю «Сравнить с...».</summary>
                [ShownWhen("открыто подменю «Сравнить с...»")]
                public const string ChooseDocumentForCompare = "miChooseDocumentForCompare";

                /// <summary>(MenuItem) [К] ModelComparator.cs:151; показ: открыто подменю «Сравнить с...».</summary>
                [ShownWhen("открыто подменю «Сравнить с...»")]
                public const string PreviousVersions = "miPreviousVersions";

                /// <summary>(MenuItem) [К] ModelComparator.cs:167; показ: открыто подменю «Сравнить с...», версий больше трёх.</summary>
                [ShownWhen("открыто подменю «Сравнить с...», версий больше трёх")]
                public const string More = "miMore";

                /// <summary>«Следующая страница» (MenuItem) [К] XpsRenderViewModel.cs:967; показ: ViewMode == Presentation.</summary>
                [ShownWhen("режим презентации")]
                public const string NextPage = "miNextPage";

                /// <summary>«Предыдущая страница» (MenuItem) [К] XpsRenderViewModel.cs:968; показ: ViewMode == Presentation.</summary>
                [ShownWhen("режим презентации")]
                public const string PreviousPage = "miPreviousPage";

                /// <summary>«Выйти из режима презентации (F5)» (MenuItem) [К] XpsRenderViewModel.cs:970 (ruXR:175); показ: ViewMode == Presentation.</summary>
                [ShownWhen("режим презентации")]
                public const string ExitPresentationMode = "miExitPresentationMode";

                /// <summary>Подменю типа замечаний: "miRemarkItem_" + имя типа БЕЗ преобразований (при нескольких типах). [К] XpsRenderViewModel.cs:902.</summary>
                public static string RemarkItem(string remarkTypeName) => "miRemarkItem_" + remarkTypeName;

                /// <summary>Версия в подменю «Сравнить с...»: "miCompareWithHistory" + i (i с 0 по порядку версий от новой; первые три - сразу, остальные - после More). [К] ModelComparator.cs:159,283.</summary>
                public static string CompareWithHistory(int index) => "miCompareWithHistory" + index;
            }

            /// <summary>Вкладка «Замечания» правой панели просмотрщика (AnnotationsTabView.xaml). Строка списка - ListItem, Name = «текст - …»; элементы строки ниже.</summary>
            public static class Annotations
            {
                /// <summary>Name пуст (Custom) [Р] разведка; Ascon.Pilot.XpsRender.Annotations.AnnotationsTabView.xaml:2.</summary>
                public const string Container = "annotationsTabView";

                /// <summary>Name пуст (ComboBox) [Р] разведка; Ascon.Pilot.XpsRender.Annotations.AnnotationsTabView.xaml:161. Пункты с id = подпись фильтра.</summary>
                public const string Filter = "filterComboBox";

                /// <summary>Name пуст (Edit) [Р] разведка; Ascon.Pilot.XpsRender.Annotations.AnnotationsTabView.xaml:167.</summary>
                public const string Search = "filterEdit";

                /// <summary>Name пуст (List) [Р] разведка; Ascon.Pilot.XpsRender.Annotations.AnnotationsTabView.xaml:168.</summary>
                public const string List = "annotationsListView";

                /// <summary>Name пуст (Thumb) [Р] разведка; Ascon.Pilot.XpsRender.Annotations.AnnotationsTabView.xaml:178.</summary>
                public const string Splitter = "splitter";

                /// <summary>(Text) [Р] разведка; Ascon.Pilot.XpsRender.Annotations.AnnotationsTabView.xaml:40.</summary>
                public const string Author = "authorTxb";

                /// <summary>(Text) [Р] разведка; Ascon.Pilot.XpsRender.Annotations.AnnotationsTabView.xaml:41.</summary>
                public const string Date = "dateTxb";

                /// <summary>Name пуст (Document) [Р] разведка; Ascon.Pilot.XpsRender.Annotations.AnnotationsTabView.xaml:60. RichTextBox; текст - в HelpText.</summary>
                public const string Text = "remarkTextBlock";

                /// <summary>(Button) [К] Ascon.Pilot.XpsRender.Annotations.AnnotationsTabView.xaml:75.</summary>
                public const string Toggle = "toggleButton";

                /// <summary>Name пуст (Image) [Р] разведка; userobjectstatecontrol.xaml:19; показ: у замечания есть атрибут состояния. Сам UserObjectStateControl (x:Name stateControl, AnnotationsTabView.xaml:132) в UIA не виден: Control без AutomationPeer (UserObjectStateControl.cs:9), его элементы - прямые потомки ListItem.</summary>
                [ShownWhen("у замечания есть атрибут состояния")]
                public const string StateIcon = "stateIcon";

                /// <summary>«заголовок состояния» (Hyperlink) [Р] разведка; commnadpopuplinkcontrol.xaml:12; показ: у замечания есть атрибут состояния. Name = заголовок состояния (например «Снято»).</summary>
                [ShownWhen("у замечания есть атрибут состояния")]
                public const string StateLink = "commandPopupLink";

                /// <summary>Переход состояния замечания (кнопка панели и пункт меню): "mi" + имя атрибута + "_" + имя состояния, без преобразований. [Р] разведка; [К] AnnotationCommandsHelper.cs:94; AnnotationsTabViewModel.cs:747.</summary>
                public static string AnnotationStateTransition(string attributeName, string stateName) => "mi" + attributeName + "_" + stateName;

                /// <summary>Панель списка замечаний (AnnotationsTabViewModel.cs:713-753). Кнопки модулей (IToolbar&lt;DocumentAnnotationsListContext&gt;) - в конце. Переходы состояния выделенных замечаний - Annotations.AnnotationStateTransition.</summary>
                public static class Toolbar
                {
                    /// <summary>Name пуст (Custom) [Р] разведка; Ascon.Pilot.XpsRender.Annotations.AnnotationsTabView.xaml:166.</summary>
                    public const string Container = "annotationsToolbar";

                    /// <summary>«Показать в версии актуальной для замечания» (Button) [К] AnnotationsTabViewModel.cs:724; показ: CanShowInActual.</summary>
                    [ShownWhen("замечание не из актуальной версии")]
                    public const string ShowInActualVersionCommand = "tbiShowInActualVersionCommand";

                    /// <summary>«Добавить в актуальную версию» (Button) [К] AnnotationsTabViewModel.cs:731; показ: CanCopyToActual.</summary>
                    [ShownWhen("можно перенести в актуальную версию")]
                    public const string CopyToActualVersionCommand = "tbiCopyToActualVersionCommand";

                    /// <summary>«Редактировать», Name пуст (Button) [Р] разведка; AnnotationsTabViewModel.cs:740.</summary>
                    public const string EditAnnotation = "tbiEditAnnotation";

                    /// <summary>«Копировать ссылку», Name пуст (Button) [Р] разведка; AnnotationsTabViewModel.cs:741. Копирует ССЫЛКУ; Ctrl+C.</summary>
                    public const string Copy = "tbiCopy";

                    /// <summary>«Совместный доступ...», Name пуст (Button) [Р] разведка; AnnotationsTabViewModel.cs:742.</summary>
                    public const string ShowSharingSettings = "tbiShowSharingSettings";

                    /// <summary>Кнопка-меню сортировки: "tbiAnnotationsSortButton" + текущее направление. [Р] разведка; [К] AnnotationsTabViewModel.cs:717.</summary>
                    public static string SortButton(bool descending) => "tbiAnnotationsSortButton" + (descending ? "Descending" : "Ascending");
                }

                /// <summary>Контекстное меню строки замечания (AnnotationsListContextMenuDirector.cs:17; AnnotationCommandsHelper.cs). Пункты модулей (IMenu&lt;DocumentAnnotationsListContext&gt;) - в конце (разведка).</summary>
                public static class Menu
                {
                    /// <summary>Name пуст (Menu) [Р] разведка; AnnotationsListContextMenuDirector.cs:17.</summary>
                    public const string Container = "AUIDAnnotationsListContextMenu";

                    /// <summary>«Редактировать», Name пуст (MenuItem) [Р] разведка; AnnotationCommandsHelper.cs:16.</summary>
                    public const string EditCommand = "miEditCommand";

                    /// <summary>«Показать в версии актуальной для замечания» (MenuItem) [К] AnnotationCommandsHelper.cs:69; показ: CanShowInActual.</summary>
                    [ShownWhen("замечание не из актуальной версии")]
                    public const string ShowInActualVersionCommand = "miShowInActualVersionCommand";

                    /// <summary>«Добавить в актуальную версию» (MenuItem) [К] AnnotationCommandsHelper.cs:79; показ: CanCopyToActual.</summary>
                    [ShownWhen("можно перенести в актуальную версию")]
                    public const string CopyToActualVersionCommand = "miCopyToActualVersionCommand";

                    /// <summary>«Копировать ссылку», Name пуст (MenuItem) [Р] разведка; AnnotationCommandsHelper.cs:22; показ: CopyLinkCommand != null и выбрано 1 замечание. Копирует ССЫЛКУ.</summary>
                    [ShownWhen("выбрано 1 замечание")]
                    public const string Copy = "miCopy";

                    /// <summary>«Совместный доступ...», Name пуст (MenuItem) [Р] разведка; AnnotationCommandsHelper.cs:51; показ: ShowSharingSettingsCommand != null (AnnotationCommandsHelper.cs:47-54).</summary>
                    [ShownWhen("есть команда совместного доступа")]
                    public const string ShowSharingSettings = "miShowSharingSettings";

                    /// <summary>«Удалить» (MenuItem) [К] AnnotationsListContextMenuDirector.cs:65; показ: IsAnnotationDeleteAvailable.</summary>
                    [ShownWhen("удаление замечания разрешено")]
                    public const string DeleteAnnotationCommand = "miDeleteAnnotationCommand";
                }
            }

            /// <summary>Стикер замечания на странице (Thumb; ItemStatus «IsSelected:{bool}; IsPopupOpen:{bool}; IsInEditMode:{bool}; X:{x}; Y:{y};», Name начинается с текста замечания) [Р] разведка.</summary>
            public static class Sticker
            {
                /// <summary>(Thumb) [Р] разведка; Ascon.Pilot.XpsRender.Annotations.Sticker.AnnotationSticker.xaml:76; показ: на странице есть замечание.</summary>
                [ShownWhen("на странице есть замечание")]
                public const string Container = "AUIDAnnotationSticker";

                /// <summary>Name пуст (Custom) [Р] разведка; Ascon.Pilot.XpsRender.Annotations.Sticker.AnnotationSticker.xaml:39; показ: на странице есть непросмотренное замечание.</summary>
                [ShownWhen("на странице есть непросмотренное замечание")]
                public const string NotViewedIcon = "notViewedIcon";

                /// <summary>Name пуст (Image) [Р] разведка; Ascon.Pilot.XpsRender.Annotations.Sticker.AnnotationSticker.xaml:42; показ: на странице есть замечание. HelpText = состояние.</summary>
                [ShownWhen("на странице есть замечание")]
                public const string StatusIcon = "statusImage";

                /// <summary>Контекстное меню стикера (AnnotationStickerContextMenuDirector.cs:26-50). Сначала пункты Annotations.Menu (AnnotationCommandsHelper.GetCommands: EditCommand, ShowInActualVersionCommand, CopyToActualVersionCommand, Copy, ShowSharingSettings, переходы Annotations.AnnotationStateTransition), затем miHideAnnotation (если HideAnnotationCommand != null) и miDeleteAnnotationCommand (если DeleteAnnotationCommand != null), в конце - пункты модулей (:49).</summary>
                public static class Menu
                {
                    /// <summary>(Menu) [К] AnnotationStickerContextMenuDirector.cs:14.</summary>
                    public const string Container = "annotationStickerContextMenu";

                    /// <summary>(MenuItem) [К] AnnotationStickerContextMenuDirector.cs:42; показ: HideAnnotationCommand != null (:40).</summary>
                    [ShownWhen("есть команда скрытия замечания")]
                    public const string HideAnnotation = "miHideAnnotation";

                    /// <summary>«Удалить» (MenuItem) [К] AnnotationStickerContextMenuDirector.cs:46; показ: DeleteAnnotationCommand != null (:44).</summary>
                    [ShownWhen("есть команда удаления замечания")]
                    public const string DeleteAnnotationCommand = "miDeleteAnnotationCommand";
                }
            }
        }
    }
}
