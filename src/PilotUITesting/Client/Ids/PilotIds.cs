using System.Linq;

namespace PilotUITesting.Client
{
    /// <summary>AutomationId клиента Pilot-ICE Enterprise / Pilot-BIM 26.11 по участкам интерфейса. Каждая public const string - точный AutomationId (исключения с суффиксом Class - это ClassName, см. их описание); префиксы - static readonly; составные id - методы; подписи (UIA Name) - PilotTitles. Id пункта меню и кнопки панели = name из построителя без преобразований (и у модулей SDK), составные имена чистятся XamlName. Id вне каталога можно передавать строкой.</summary>
    /// <remarks>
    /// Формат описания: «подпись» (ControlType) [уровень] доказательство; источник; показ: условие. «Name пуст» - Name в UIA пустой (подпись только в дочернем
    /// Text или в HelpText, со знаком «_»). Уровни: [Р] проверено на живом клиенте Pilot-ICE Enterprise 26.11 (разведка 2026-09-24);
    /// [Д] встречалось в дампах UI Automation реальных прогонов (UI-тесты модулей на Pilot-BIM и Pilot-ICE); [К] только код клиента
    /// 26.11 («Файл:строка» - декомпиляция его сборок); [Т] используется проходящими тестами. [ShownWhen] - элемент есть не всегда;
    /// CatalogAssert его отсутствие не считает ошибкой. Константы с суффиксом Class - ClassName, а не AutomationId.
    /// </remarks>
    public static partial class PilotIds
    {
        /// <summary>Точная копия StringExtensions.GetXamlName клиента: остаются только буквы и цифры (char.IsLetterOrDigit). Так клиент чистит имена типов, отчётов и шаблонов в составных id. [К] Ascon.Pilot.Core/StringExtensions.cs:56-59.</summary>
        public static string XamlName(string s) => new string(s.Where(char.IsLetterOrDigit).ToArray());

        /// <summary>Общие части любой панели ToolbarStripe (шаблон темы toolbarstripe.xaml). Кнопка панели: id = name из построителя, ItemStatus «IsHidden:{bool}» (у переключателя ещё «IsChecked:{bool};»). У панелей модулей контейнер без id - искать кнопки внутри корня представления модуля [Р] разведка.</summary>
        public static class ToolbarStripe
        {
            /// <summary>Name пуст (Button) [Р] разведка; toolbarstripe.xaml:263. «…» - не поместившиеся кнопки; показ: есть не поместившиеся кнопки (ToolbarStripe.cs:102, иначе Hidden). В annotationsToolbar появилась только после выделения строки (разведка: 037:24, в 036 нет).</summary>
            [ShownWhen("на панели есть не поместившиеся кнопки (OverflowingItems)")]
            public const string MoreButton = "PART_MoreButton";

            /// <summary>(Menu) [К] toolbarstripe.xaml:270; показ: открыто меню PART_MoreButton. Пункты с теми же id, что у скрытых кнопок.</summary>
            [ShownWhen("открыто меню PART_MoreButton")]
            public const string MoreMenu = "moreButtonMenu";

            /// <summary>(Edit) [К] toolbarstripe.xaml:196; показ: на панели есть IToolbarInputItem. Поле IToolbarInputItem: id всегда pageNumber (x:Name), name элемента в UIA не попадает.</summary>
            [ShownWhen("на панели есть IToolbarInputItem")]
            public const string InputBox = "pageNumber";

            /// <summary>«/ N» (Text) [К] toolbarstripe.xaml:208; показ: на панели есть IToolbarInputItem. Текст справа от pageNumber, StringFormat «/ {0}».</summary>
            [ShownWhen("на панели есть IToolbarInputItem")]
            public const string InputTotal = "TotalCount";

            /// <summary>(List) [Р] разведка; SilentInvoker.cs. ОПАСНО: это ClassName, НЕ AutomationId. Служебный [List] внутри каждой кнопки ToolbarStripe; чтение SelectionPattern.Selection у него ВЫПОЛНЯЕТ команду кнопки (SilentInvokeAutomationPeer.GetSelection -> Command.Execute, разведка). В дампах и инспекторах Selection у него не читать.</summary>
            public const string SilentInvokerClass = "SilentInvoker";

            /// <summary>Выпадающее меню кнопки-меню: id кнопки + "ContextMenu". [Р] разведка; [К] toolbarstripe.xaml:113.</summary>
            public static string MenuOf(string buttonId) => buttonId + "ContextMenu";

            /// <summary>Стрелка split-кнопки: id кнопки + "MoreButton". [Р] разведка; [К] toolbarstripe.xaml:152.</summary>
            public static string SplitArrowOf(string buttonId) => buttonId + "MoreButton";
        }
    }
}
