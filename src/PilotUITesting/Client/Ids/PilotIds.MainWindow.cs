namespace PilotUITesting.Client
{
    public static partial class PilotIds
    {
        /// <summary>Главное окно клиента (PureWindow). HelpText окна: «AUIDPilotWindow;IsActive:{bool};IsVisible:{bool};State:{WindowState}» [Р] разведка.</summary>
        public static class MainWindow
        {
            /// <summary>(Window) [Р] разведка; Ascon.Pilot.Pilot.MainView.xaml:2. ItemStatus = имя продукта.</summary>
            public const string Container = "mainPilotView";

            /// <summary>Name пуст (Image) [Р] разведка; purewindow.xaml:45.</summary>
            public const string Icon = "PART_Icon";

            /// <summary>Name пуст (Menu) [Р] разведка; Ascon.Pilot.Pilot.MainView.xaml:113.</summary>
            public const string MainMenu = "mainMenu";

            /// <summary>Name пуст (Button) [Р] разведка; Ascon.Pilot.Pilot.MainView.xaml:28. ItemStatus = «логин (имя)», HelpText «Подключено - …»; открывает окно userProfileWindow.</summary>
            public const string UserProfile = "AUIDCurrentUserProfile";

            /// <summary>Name пуст (Image) [Р] разведка; Ascon.Pilot.Pilot.MainView.xaml:29. HelpText = состояние подключения ЗАГЛАВНЫМИ (PilotTitles.Connection), ItemStatus «IsAdmin:{bool}».</summary>
            public const string ConnectionState = "AUIDConnectionState";

            /// <summary>Name пуст (Custom) [Р] разведка; Ascon.Pilot.Pilot.MainView.xaml:153. HelpText «ConnectionState:{Online|Connecting|NoSettings|Error|Offline}», ItemStatus «IsLoadingTabs:{bool}», ItemType = последняя команда pipe (mniShowDocumentCommand:…).</summary>
            public const string TabHost = "tabHostView";

            /// <summary>Name пуст (Custom) [Р] разведка; Ascon.Pilot.UpdateManager.ClientUpdatePanelView.xaml:2.</summary>
            public const string UpdatePanel = "AUIDUpdatePanel";

            /// <summary>(Window) [Р] разведка; Ascon.Pilot.Controls.Notifications.NotificationWindow.xaml:2; показ: после запуска клиента, закрывается само. Окно верхнего уровня (не потомок mainPilotView).</summary>
            [ShownWhen("после запуска клиента, закрывается само")]
            public const string NotificationWindow = "notificationWindow";

            /// <summary>Name пуст (Button) [Р] разведка; purewindow.xaml:55.</summary>
            public const string Minimize = "btnMinimize";

            /// <summary>Name пуст (Button) [Д] дамп прогона (тот же шаблон PureWindow; также D08:23); purewindow.xaml:56; показ: окно не развёрнуто (purewindow.xaml:69-70).</summary>
            [ShownWhen("окно не развёрнуто")]
            public const string Maximize = "btnMaximize";

            /// <summary>Name пуст (Button) [Р] разведка; purewindow.xaml:57; показ: окно развёрнуто.</summary>
            [ShownWhen("окно развёрнуто")]
            public const string RestoreDown = "btnRestoreDown";

            /// <summary>Name пуст (Button) [Р] разведка; purewindow.xaml:58. ОПАСНО: закрывает клиент. Искать только прямым потомком mainPilotView: тот же id у кнопки заголовка любого DialogWindow (DialogWindow.Close) и у кнопки окна «О программе».</summary>
            public const string CloseClientButton = "btnClose";

            /// <summary>Name пуст (Button) [Р] разведка; Ascon.Pilot.Pilot.MainView.xaml:147. ОПАСНО: служебная кнопка клиента, показывает тестовое окно непредвиденной ошибки. Не нажимать в сценариях.</summary>
            public const string ShowTestUnexpectedWindow = "showTestUnexpectedWindow";

            /// <summary>Name пуст (Button) [Р] разведка; Ascon.Pilot.Pilot.MainView.xaml:148. ОПАСНО: служебная кнопка клиента, показывает тестовое окно ошибки. Не нажимать в сценариях.</summary>
            public const string ShowTestErrorWindow = "showTestErrorWindow";

            /// <summary>Name пуст (Button) [Р] разведка; Ascon.Pilot.Pilot.MainView.xaml:149. ОПАСНО: служебная кнопка клиента, показывает тестовое окно ошибки сервера. Не нажимать в сценариях.</summary>
            public const string ShowTestServerErrorWindow = "showTestServerErrorWindow";

            /// <summary>Name пуст (Button) [Р] разведка; Ascon.Pilot.Pilot.MainView.xaml:150. ОПАСНО: служебная кнопка клиента, показывает тестовое окно «Отправка изменений». Не нажимать в сценариях.</summary>
            public const string ShowUnacceptedChangesErrorWindow = "showUnacceptedChangesErrorWindow";
        }

        /// <summary>Главное меню (MainMenuDirector.cs:23-38). Пункты модулей (IMenu&lt;MainViewContext&gt;) добавляются после встроенных, id = name модуля.</summary>
        public static class MainMenu
        {
            /// <summary>Name пуст (Menu) [Р] разведка; Ascon.Pilot.Pilot.MainView.xaml:113.</summary>
            public const string Container = "mainMenu";

            /// <summary>«СЕРВИС» (MenuItem) [Р] разведка; MainMenuDirector.cs:25.</summary>
            public const string Service = "mniService";

            /// <summary>«Учетная запись», Name пуст (MenuItem) [К] MainMenuDirector.cs:26; показ: открыто подменю «СЕРВИС».</summary>
            [ShownWhen("открыто подменю «СЕРВИС»")]
            public const string AccountSettings = "mniConnSettings";

            /// <summary>«Настройки» (MenuItem) [К] MainMenuDirector.cs:27; показ: открыто подменю «СЕРВИС».</summary>
            [ShownWhen("открыто подменю «СЕРВИС»")]
            public const string Options = "mniOptions";

            /// <summary>«Настройки клиента», Name пуст (MenuItem) [К] MainMenuDirector.cs:28; показ: открыто подменю «Настройки».</summary>
            [ShownWhen("открыто подменю «Настройки»")]
            public const string ClientOptions = "mniClientOptions";

            /// <summary>«Управление общими настройками», Name пуст (MenuItem) [К] MainMenuDirector.cs:29; показ: открыто подменю «Настройки», команда доступна.</summary>
            [ShownWhen("открыто подменю «Настройки», команда доступна")]
            public const string CommonSettings = "mniCommonSettings";

            /// <summary>«Модули расширения», Name пуст (MenuItem) [К] MainMenuDirector.cs:30; показ: открыто подменю «Настройки».</summary>
            [ShownWhen("открыто подменю «Настройки»")]
            public const string Extensions = "mniExtensions";

            /// <summary>«Управление шаблонами документов», Name пуст (MenuItem) [К] MainMenuDirector.cs:31; показ: открыто подменю «Настройки».</summary>
            [ShownWhen("открыто подменю «Настройки»")]
            public const string DocumentTemplatesManager = "mniDocumentTemplatesManager";

            /// <summary>«Управление кешем», Name пуст (MenuItem) [К] MainMenuDirector.cs:32; показ: открыто подменю «СЕРВИС».</summary>
            [ShownWhen("открыто подменю «СЕРВИС»")]
            public const string CacheManagement = "mniCacheManagement";

            /// <summary>«Справка», Name пуст (MenuItem) [К] MainMenuDirector.cs:34; показ: открыто подменю «СЕРВИС».</summary>
            [ShownWhen("открыто подменю «СЕРВИС»")]
            public const string Help = "mniHelp";

            /// <summary>«О программе {продукт}», Name пуст (MenuItem) [К] MainMenuDirector.cs:35; MainViewModelBase.cs:1323 (ruVE:353); показ: открыто подменю «СЕРВИС».</summary>
            [ShownWhen("открыто подменю «СЕРВИС»")]
            public const string About = "mniAbout";

            /// <summary>«Выход», Name пуст (MenuItem) [К] MainMenuDirector.cs:37; показ: открыто подменю «СЕРВИС». ОПАСНО: закрывает клиент.</summary>
            [ShownWhen("открыто подменю «СЕРВИС»")]
            public const string Exit = "mniExit";
        }

        /// <summary>Кнопки заголовка окна DialogWindow (dialogwindow.xaml:18-20): подтверждения, справочник, выбор, диалоги модулей. Искать только внутри найденного окна: btnClose совпадает с id кнопки закрытия главного окна.</summary>
        public static class DialogWindow
        {
            /// <summary>(Button) [К] dialogwindow.xaml:18; показ: ResizeMode окна CanMinimize/CanResize.</summary>
            [ShownWhen("ResizeMode окна CanMinimize/CanResize")]
            public const string Minimize = "btnMinimize";

            /// <summary>(Button) [К] dialogwindow.xaml:19; показ: ResizeMode окна CanResize.</summary>
            [ShownWhen("ResizeMode окна CanResize")]
            public const string Restore = "btnRestore";

            /// <summary>Name пуст (Button) [Р] разведка; dialogwindow.xaml:20. Скрыта при IsVisibleCloseButton=false (dialogwindow.xaml:54; клиент это свойство не выставляет).</summary>
            public const string Close = "btnClose";
        }
    }
}
