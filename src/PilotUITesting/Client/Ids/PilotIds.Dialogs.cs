namespace PilotUITesting.Client
{
    public static partial class PilotIds
    {
        /// <summary>Кнопки окон-сообщений. YesNoCancelMessageBoxView (YesNoCancelMessageBoxView.xaml:26-28) - подтверждения удаления/отзыва, AskConfirmation; подписи - PilotTitles.Buttons. Клиентский MessageBoxView - панель OkCancelButtonsPanel (okcancelbuttonspanel.xaml:9-11). Окно: id = Name = заголовок.</summary>
        public static class MessageBox
        {
            /// <summary>«Да | Удалить | Отозвать» (Button) [К] Ascon.Pilot.VisualElements.Dialogs.Messagebox.YesNoCancelMessageBoxView.xaml:26. AutomationId задан явно, x:Name btnOk в UIA не виден.</summary>
            public const string Yes = "6";

            /// <summary>(Button) [К] Ascon.Pilot.VisualElements.Dialogs.Messagebox.YesNoCancelMessageBoxView.xaml:27; показ: окно с тремя кнопками (MessageBoxButton.YesNoCancel).</summary>
            [ShownWhen("окно с тремя кнопками (MessageBoxButton.YesNoCancel)")]
            public const string No = "7";

            /// <summary>«Нет | Не удалять | Не отзывать» (Button) [К] Ascon.Pilot.VisualElements.Dialogs.Messagebox.YesNoCancelMessageBoxView.xaml:28.</summary>
            public const string Cancel = "2";

            /// <summary>(Button) [Д] дамп прогона (тот же шаблон); okcancelbuttonspanel.xaml:9; показ: IsOkButtonVisible. В ShowActionConfirmationDialog - «Да»; в ShowInfo скрыта.</summary>
            [ShownWhen("IsOkButtonVisible")]
            public const string ClientOk = "btnOk";

            /// <summary>(Button) [К] okcancelbuttonspanel.xaml:10; показ: IsNoButtonVisible.</summary>
            [ShownWhen("IsNoButtonVisible")]
            public const string ClientNo = "btnNo";

            /// <summary>(Button) [Д] дамп прогона (тот же шаблон); okcancelbuttonspanel.xaml:11. В ShowInfo единственная кнопка «OK» - это btnCancel (MessageBoxModel.cs:52-56); в ShowActionConfirmationDialog - «Нет».</summary>
            public const string ClientCancel = "btnCancel";
        }

        /// <summary>Окно ошибки клиента (ExceptionDialog.xaml): PureWindow, дочернее окно mainPilotView; id = "AUIDPilotError-Unexpected:{есть AdditionalInfo}", Name = заголовок.</summary>
        public static class ErrorWindow
        {
            /// <summary>Префикс id окна ошибки (поиск по началу id). [Д] дамп прогона; [К] ExceptionDialog.xaml:3-8.</summary>
            public static readonly string WindowIdPrefix = "AUIDPilotError-";

            /// <summary>id окна ошибки: "AUIDPilotError-Unexpected:True|False" (True - есть подробности AdditionalInfo). [Д] дамп прогона; [К] ExceptionDialog.xaml:3-8.</summary>
            public static string WindowId(bool unexpected) => "AUIDPilotError-Unexpected:" + (unexpected ? "True" : "False");

            /// <summary>Name пуст (Edit) [Д] дамп прогона; Ascon.Pilot.VisualElements.Dialogs.ExceptionDialog.xaml:38. ValuePattern - текст ошибки.</summary>
            public const string Message = "exceptionMessageText";

            /// <summary>«Открыть папку журнала ошибок» (Hyperlink) [Д] дамп прогона; ExceptionDialog.xaml:43; показ: есть подробности (AdditionalInfo, ExceptionDialog.xaml:42).</summary>
            [ShownWhen("есть подробности (AdditionalInfo)")]
            public const string OpenLogFolder = "lnkLicence";

            /// <summary>«Дополнительная информация» (Button) [Д] дамп прогона; ExceptionDialog.xaml:47; показ: есть подробности (AdditionalInfo). Тот же id у Expander (Group, ExceptionDialog.xaml:48), он есть и без подробностей (дамп прогона), поэтому искать с ControlType Button.</summary>
            [ShownWhen("есть подробности (AdditionalInfo)")]
            public const string Details = "additionalInfoExpander";

            /// <summary>«Копировать» (Button) [Д] дамп прогона; Ascon.Pilot.VisualElements.Dialogs.ExceptionDialog.xaml:57; показ: есть подробности (AdditionalInfo).</summary>
            [ShownWhen("есть подробности (AdditionalInfo)")]
            public const string Copy = "copyButton";

            /// <summary>«Закрыть» (Button) [Д] дамп прогона; Ascon.Pilot.VisualElements.Dialogs.ExceptionDialog.xaml:58.</summary>
            public const string Close = "closeButton";
        }

        /// <summary>Окно «Отправка изменений» - сервер отклонил изменения (UnacceptedExceptionDialog.xaml:2-31). Name = «&lt;продукт&gt; – Отправка изменений».</summary>
        public static class UnsentChanges
        {
            /// <summary>(Window) [Д] дамп прогона; Ascon.Pilot.VisualElements.Dialogs.UnacceptedExceptionDialog.xaml:2.</summary>
            public const string Container = "AUIDUnacceptedExceptionWindow";

            /// <summary>Name пуст (Edit) [Д] дамп прогона; Ascon.Pilot.VisualElements.Dialogs.UnacceptedExceptionDialog.xaml:22.</summary>
            public const string Message = "exceptionMessageText";

            /// <summary>(Text) [Д] дамп прогона; Ascon.Pilot.VisualElements.Dialogs.UnacceptedExceptionDialog.xaml:24.</summary>
            public const string Info = "ServerExceptionDlgInfoText";

            /// <summary>«Удалить изменения», Name пуст (Button) [Д] дамп прогона; okcancelbuttonspanel.xaml:9. ОПАСНО: стирает все неотправленные изменения пользователя.</summary>
            public const string DiscardChanges = "btnOk";

            /// <summary>«Найти элемент» | «Отмена», Name пуст (Button) [Д] дамп прогона; okcancelbuttonspanel.xaml:11. «Отмена» без действия - если элемент не загрузился или недоступен (UnacceptedExceptionDialogViewModel.cs:113-126).</summary>
            public const string FindElement = "btnCancel";
        }

        /// <summary>Окно длительной операции (ProcessView.xaml:2-28): дочернее окно активного окна (например, карточки), Name = заголовок операции. Кнопки «Отмена»/«Закрыть» без id - PilotTitles.Buttons.Cancel/Close.</summary>
        public static class Progress
        {
            /// <summary>(Window) [Д] дамп прогона; Ascon.Pilot.Controls.Dialogs.ProcessView.xaml:2.</summary>
            public const string Container = "AUIDUpdateProcessWindow";

            /// <summary>(Text) [Д] дамп прогона; Ascon.Pilot.Controls.Dialogs.ProcessView.xaml:13; показ: операция идёт без ошибки. Текст операции.</summary>
            [ShownWhen("операция идёт без ошибки")]
            public const string Description = "AUIDUpdateProcessDescription";

            /// <summary>(Text) [К] Ascon.Pilot.Controls.Dialogs.ProcessView.xaml:14; показ: операция завершилась ошибкой.</summary>
            [ShownWhen("операция завершилась ошибкой")]
            public const string ExceptionTitle = "AUIDProcessExceptionTitle";

            /// <summary>Name пуст (Custom) [Д] дамп прогона; ProcessView.xaml:15; показ: операция завершилась ошибкой. Без ошибки есть в дереве offscreen (D08:16), поэтому признак ошибки - !IsOffscreen или наличие AUIDProcessException.</summary>
            [ShownWhen("операция завершилась ошибкой")]
            public const string InfoPanel = "AUIDInfoPanel";

            /// <summary>(Text) [К] Ascon.Pilot.Controls.Dialogs.ProcessView.xaml:16; показ: операция завершилась ошибкой.</summary>
            [ShownWhen("операция завершилась ошибкой")]
            public const string Exception = "AUIDProcessException";

            /// <summary>(Text) [К] Ascon.Pilot.Controls.Dialogs.ProcessView.xaml:18; показ: операция завершилась ошибкой.</summary>
            [ShownWhen("операция завершилась ошибкой")]
            public const string Solution = "AUIDProcessSolution";
        }

        /// <summary>Системный диалог открытия и сохранения файла Windows: дочернее окно карточки или окно верхнего уровня.</summary>
        public static class WindowsFileDialog
        {
            /// <summary>(Window) [Т] PilotObjectDialog.cs:24. Это ClassName окна, НЕ AutomationId (CatalogAssert должен её пропускать).</summary>
            public const string WindowClass = "#32770";

            /// <summary>(ComboBox) [Т] PilotObjectDialog.cs:25. «Имя файла»: ComboBox с вложенным Edit того же id; значение - ValuePattern у Edit.</summary>
            public const string FileName = "1148";

            /// <summary>(Edit) «Имя файла» диалога СОХРАНЕНИЯ: Edit внутри ComboBox FileNameControlHost; значение - ValuePattern. Разведано 06.10.2026 (экспорт редактора автоблокировок, клиент 26.11).</summary>
            public const string SaveFileName = "1001";

            /// <summary>«Открыть» / «Сохранить» (Button) [Т] PilotObjectDialog.cs:26.</summary>
            public const string Ok = "1";

            /// <summary>«Отмена» (Button) [Т] PilotObjectDialog.cs:27.</summary>
            public const string Cancel = "2";
        }
    }
}
