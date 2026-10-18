using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Identifiers;
using FlaUI.Core.Patterns;

namespace PilotUITesting.Uia
{
    /// <summary>Добавления 0.2: поиск без явного таймаута, чтение ItemStatus/Value/Toggle, подробный дамп.</summary>
    public static partial class AutomationElementExtensions
    {
        /// <summary>Потомок с AutomationId, ожидание <see cref="Wait.DefaultTimeout"/>; нет - <see cref="ElementNotFoundException"/> с перечнем id области.</summary>
        public static AutomationElement ById(this AutomationElement root, string automationId)
        {
            return root.Find(By.Id(automationId));
        }

        /// <summary>Потомок с именем (Name), ожидание <see cref="Wait.DefaultTimeout"/>.</summary>
        public static AutomationElement ByName(this AutomationElement root, string name)
        {
            return root.Find(By.Name(name));
        }

        /// <summary>Строка списка, тексты которой содержат все фрагменты; ожидание <see cref="Wait.DefaultTimeout"/>.</summary>
        public static AutomationElement RowContaining(this AutomationElement list, params string[] parts)
        {
            return list.RowContaining(Wait.DefaultTimeout, parts);
        }

        /// <summary>
        /// Потомок по селектору с ожиданием (timeout null - <see cref="Wait.DefaultTimeout"/>).
        /// Отказ - <see cref="ElementNotFoundException"/> (при полных обходах); при неполных обходах - <see cref="ClientBusyException"/> (клиент не отвечал) или <see cref="PilotWaitTimeoutException"/> (узлы исчезали во время каждого обхода).
        /// </summary>
        public static AutomationElement Find(this AutomationElement root, By selector, TimeSpan? timeout = null, string scope = null)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }
            return UiQuery.WaitFind(() => root, selector, timeout ?? Wait.DefaultTimeout, scope ?? root.Describe());
        }

        /// <summary>
        /// Потомок по селектору без ожидания. null - только если обход прошёл полностью и элемента нет;
        /// полного обхода не было за <see cref="Wait.DefaultTimeout"/> - отказ: при неполных обходах - <see cref="ClientBusyException"/> (клиент не отвечал) или <see cref="PilotWaitTimeoutException"/> (узлы исчезали во время каждого обхода).
        /// </summary>
        public static AutomationElement TryFind(this AutomationElement root, By selector)
        {
            return UiQuery.TryFind(root, selector, Wait.DefaultTimeout);
        }

        /// <summary>Все потомки по селектору при полном обходе дерева (полного обхода не было за <see cref="Wait.DefaultTimeout"/> - отказ: при неполных обходах - <see cref="ClientBusyException"/> (клиент не отвечал) или <see cref="PilotWaitTimeoutException"/> (узлы исчезали во время каждого обхода)).</summary>
        public static IReadOnlyList<AutomationElement> FindAll(this AutomationElement root, By selector, int maxDepth = DefaultMaxDepth)
        {
            return UiQuery.FindAllComplete(root, selector, Wait.DefaultTimeout, maxDepth);
        }

        /// <summary>ItemStatus элемента (Pilot пишет туда состояния: «IsHidden:False; IsChecked:True;», «IsLoadingTabs:False»); "" - нет или не прочитано.</summary>
        public static string SafeItemStatus(this AutomationElement element)
        {
            return SafeRead(() => element?.Properties.ItemStatus.ValueOrDefault, "");
        }

        /// <summary>ItemType элемента (у строк проводника - системное имя типа, у tabHostView - последняя команда); "" - нет.</summary>
        public static string SafeItemType(this AutomationElement element)
        {
            return SafeRead(() => element?.Properties.ItemType.ValueOrDefault, "");
        }

        /// <summary>Значение ValuePattern; "" - паттерна нет или не прочитано.</summary>
        public static string SafeValue(this AutomationElement element)
        {
            return SafeRead(() => element?.Patterns.Value.PatternOrDefault?.Value.ValueOrDefault, "");
        }

        /// <summary>Состояние TogglePattern; null - паттерна нет или не прочитано.</summary>
        public static ToggleState? SafeToggleState(this AutomationElement element)
        {
            return SafeRead<ToggleState?>(() =>
            {
                ITogglePattern toggle = element?.Patterns.Toggle.PatternOrDefault;
                return toggle == null ? (ToggleState?)null : toggle.ToggleState.ValueOrDefault;
            }, null);
        }

        /// <summary>Элемент имеет фокус клавиатуры.</summary>
        public static bool SafeHasKeyboardFocus(this AutomationElement element)
        {
            return SafeRead(() => element != null && element.Properties.HasKeyboardFocus.ValueOrDefault, false);
        }

        /// <summary>
        /// Дамп поддерева. details: класс, прямоугольник, HelpText, ItemStatus, ItemType, Value и поддерживаемые паттерны.
        /// SelectionPattern.Selection не читается: у служебного списка SilentInvoker в кнопках панелей Pilot это чтение ВЫПОЛНЯЕТ команду кнопки.
        /// </summary>
        public static string DumpTree(this AutomationElement root, int maxDepth, bool details)
        {
            if (!details)
            {
                return root.DumpTree(maxDepth);
            }
            StringBuilder sb = new StringBuilder();
            DumpDetailed(root, sb, 0, maxDepth);
            return sb.ToString();
        }

        /// <summary>Описание элемента одной строкой для сообщений: [тип] id="…" name="…".</summary>
        public static string Describe(this AutomationElement element)
        {
            if (element == null)
            {
                return "(нет элемента)";
            }
            return $"[{element.SafeControlType()}] id=\"{element.SafeAutomationId()}\" name=\"{Trunc(element.SafeName(), 60)}\"";
        }

        private static void DumpDetailed(AutomationElement element, StringBuilder sb, int depth, int maxDepth)
        {
            if (element == null || depth > maxDepth)
            {
                return;
            }
            sb.Append(new string(' ', depth * 2));
            sb.Append('[').Append(element.SafeControlType()).Append(']');
            Append(sb, "cls", element.SafeClassName());
            Append(sb, "id", element.SafeAutomationId());
            Append(sb, "name", Trunc(element.SafeName(), 90));
            Append(sb, "help", Trunc(element.SafeHelpText(), 90));
            Append(sb, "status", Trunc(element.SafeItemStatus(), 90));
            Append(sb, "itype", Trunc(element.SafeItemType(), 90));
            Append(sb, "value", Trunc(element.SafeValue(), 60));
            if (!element.SafeIsEnabled())
            {
                sb.Append(" disabled");
            }
            if (element.SafeIsOffscreen())
            {
                sb.Append(" offscreen");
            }
            sb.Append(' ').Append(SafeRead(() => element.BoundingRectangle.ToString(), ""));
            string patterns = SafeRead(() => string.Join(",", element.GetSupportedPatterns().Select(PatternName)), "");
            if (patterns.Length > 0)
            {
                sb.Append(" pat=[").Append(patterns).Append(']');
            }
            sb.AppendLine();
            foreach (AutomationElement child in element.SafeChildren())
            {
                DumpDetailed(child, sb, depth + 1, maxDepth);
            }
        }

        private static string PatternName(PatternId id)
        {
            string name = id.Name ?? "";
            return name.EndsWith("Pattern", StringComparison.Ordinal) ? name.Substring(0, name.Length - "Pattern".Length) : name;
        }

        private static void Append(StringBuilder sb, string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                sb.Append(' ').Append(key).Append("=\"").Append(value).Append('"');
            }
        }

        private static T SafeRead<T>(Func<T> read, T fallback)
        {
            try
            {
                T value = read();
                return value == null ? fallback : value;
            }
            catch (Exception ex) when (UiFault.IsTransient(ex) || ex is FlaUI.Core.Exceptions.NotSupportedException)
            {
                return fallback;
            }
        }
    }
}
