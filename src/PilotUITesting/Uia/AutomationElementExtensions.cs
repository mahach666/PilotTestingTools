using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;

namespace PilotUITesting.Uia
{
    /// <summary>
    /// Поиск элементов с ожиданием и «мягкие» действия для WPF-контролов Pilot.
    /// Поиск по потомкам выполняется обходом в ширину через FindAllChildren: штатный FindFirst(Descendants)
    /// в окне Pilot с открытым модулем и встроенным просмотрщиком документов «зависает» на ~30 с и возвращает null.
    /// </summary>
    public static partial class AutomationElementExtensions
    {
        public const int DefaultMaxDepth = 60;

        // ------------------------------------------------------------------ обход дерева

        /// <summary>Первый потомок, удовлетворяющий условию (обход в ширину, без FindFirst(Descendants)).</summary>
        public static AutomationElement FindDescendantBfs(this AutomationElement root, Func<AutomationElement, bool> predicate, int maxDepth = DefaultMaxDepth)
        {
            if (root == null)
            {
                return null;
            }
            Queue<KeyValuePair<AutomationElement, int>> queue = new Queue<KeyValuePair<AutomationElement, int>>();
            queue.Enqueue(new KeyValuePair<AutomationElement, int>(root, 0));
            while (queue.Count > 0)
            {
                KeyValuePair<AutomationElement, int> current = queue.Dequeue();
                if (current.Value >= maxDepth)
                {
                    continue;
                }
                foreach (AutomationElement child in SafeChildren(current.Key))
                {
                    if (Safe(() => predicate(child)))
                    {
                        return child;
                    }
                    queue.Enqueue(new KeyValuePair<AutomationElement, int>(child, current.Value + 1));
                }
            }
            return null;
        }

        /// <summary>Все потомки, удовлетворяющие условию (обход в ширину).</summary>
        public static List<AutomationElement> FindDescendantsBfs(this AutomationElement root, Func<AutomationElement, bool> predicate, int maxDepth = DefaultMaxDepth)
        {
            List<AutomationElement> result = new List<AutomationElement>();
            if (root == null)
            {
                return result;
            }
            Queue<KeyValuePair<AutomationElement, int>> queue = new Queue<KeyValuePair<AutomationElement, int>>();
            queue.Enqueue(new KeyValuePair<AutomationElement, int>(root, 0));
            while (queue.Count > 0)
            {
                KeyValuePair<AutomationElement, int> current = queue.Dequeue();
                if (current.Value >= maxDepth)
                {
                    continue;
                }
                foreach (AutomationElement child in SafeChildren(current.Key))
                {
                    if (Safe(() => predicate(child)))
                    {
                        result.Add(child);
                    }
                    queue.Enqueue(new KeyValuePair<AutomationElement, int>(child, current.Value + 1));
                }
            }
            return result;
        }

        public static AutomationElement[] SafeChildren(this AutomationElement element)
        {
            try
            {
                return element?.FindAllChildren() ?? new AutomationElement[0];
            }
            catch
            {
                return new AutomationElement[0];
            }
        }

        /// <summary>Прямой потомок с указанным AutomationId (без ожидания).</summary>
        public static AutomationElement ChildById(this AutomationElement element, string automationId)
        {
            return element.SafeChildren().FirstOrDefault(c => c.SafeAutomationId() == automationId);
        }

        // ------------------------------------------------------------------ поиск с ожиданием

        public static AutomationElement ById(this AutomationElement root, string automationId, TimeSpan timeout)
        {
            return Wait.For(() => root.TryById(automationId), timeout, $"элемент с AutomationId '{automationId}'");
        }

        public static AutomationElement ByName(this AutomationElement root, string name, TimeSpan timeout)
        {
            return Wait.For(() => root.TryByName(name), timeout, $"элемент с именем '{name}'");
        }

        public static AutomationElement ByNameContains(this AutomationElement root, string part, TimeSpan timeout)
        {
            return Wait.For(() => root.FindDescendantBfs(e => e.SafeName().IndexOf(part, StringComparison.OrdinalIgnoreCase) >= 0), timeout, $"элемент, имя которого содержит '{part}'");
        }

        public static AutomationElement TryById(this AutomationElement root, string automationId)
        {
            return root.FindDescendantBfs(e => e.SafeAutomationId() == automationId);
        }

        public static AutomationElement TryByName(this AutomationElement root, string name)
        {
            return root.FindDescendantBfs(e => e.SafeName() == name);
        }

        public static AutomationElement TryByControlTypeAndName(this AutomationElement root, ControlType type, string name)
        {
            return root.FindDescendantBfs(e => e.SafeName() == name && e.SafeControlType() == type);
        }

        // ------------------------------------------------------------------ безопасные свойства

        public static string SafeName(this AutomationElement element)
        {
            try
            {
                return element?.Name ?? "";
            }
            catch
            {
                return "";
            }
        }

        public static string SafeAutomationId(this AutomationElement element)
        {
            try
            {
                return element?.AutomationId ?? "";
            }
            catch
            {
                return "";
            }
        }

        public static string SafeClassName(this AutomationElement element)
        {
            try
            {
                return element?.ClassName ?? "";
            }
            catch
            {
                return "";
            }
        }

        public static ControlType SafeControlType(this AutomationElement element)
        {
            try
            {
                return element?.ControlType ?? ControlType.Unknown;
            }
            catch
            {
                return ControlType.Unknown;
            }
        }

        public static bool SafeIsEnabled(this AutomationElement element)
        {
            try
            {
                return element != null && element.IsEnabled;
            }
            catch
            {
                return false;
            }
        }

        public static bool SafeIsOffscreen(this AutomationElement element)
        {
            try
            {
                return element == null || element.IsOffscreen;
            }
            catch
            {
                return true;
            }
        }

        /// <summary>Подсказка элемента: строковый ToolTip WPF попадает в свойство HelpText.</summary>
        public static string SafeHelpText(this AutomationElement element)
        {
            try
            {
                return element?.Properties.HelpText.ValueOrDefault ?? "";
            }
            catch
            {
                return "";
            }
        }

        /// <summary>Элемент выделен (SelectionItemPattern: вкладка, строка списка).</summary>
        public static bool SafeIsSelected(this AutomationElement element)
        {
            try
            {
                return element != null && element.Patterns.SelectionItem.IsSupported && element.Patterns.SelectionItem.Pattern.IsSelected.Value;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>Элемент всё ещё существует в дереве UI Automation (окно не закрыто, строка не удалена).</summary>
        public static bool SafeIsAvailable(this AutomationElement element)
        {
            try
            {
                return element != null && element.IsAvailable;
            }
            catch
            {
                return false;
            }
        }

        // ------------------------------------------------------------------ действия

        /// <summary>Нажатие через Invoke; если паттерн не поддерживается - клик мышью по центру.</summary>
        public static void Press(this AutomationElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }
            Wait.Retry(() =>
            {
                if (element.Patterns.Invoke.IsSupported)
                {
                    element.Patterns.Invoke.Pattern.Invoke();
                    return;
                }
                element.Click();
            });
        }

        /// <summary>Выделение элемента списка: SelectionItem, иначе клик.</summary>
        public static void SelectItem(this AutomationElement element)
        {
            Wait.Retry(() =>
            {
                if (element.Patterns.SelectionItem.IsSupported)
                {
                    element.Patterns.SelectionItem.Pattern.Select();
                    return;
                }
                element.Click();
            });
        }

        /// <summary>Ввод текста в поле: ValuePattern, иначе фокус и набор с клавиатуры.</summary>
        public static void SetText(this AutomationElement element, string text)
        {
            Wait.Retry(() =>
            {
                element.Focus();
                if (element.Patterns.Value.IsSupported)
                {
                    element.Patterns.Value.Pattern.SetValue(text ?? "");
                    return;
                }
                FlaUI.Core.Input.Keyboard.Type(text ?? "");
            });
        }

        // ------------------------------------------------------------------ содержимое

        /// <summary>Все текстовые узлы поддерева (для проверки содержимого строк списков).</summary>
        public static IReadOnlyList<string> Texts(this AutomationElement element)
        {
            return element.FindDescendantsBfs(e => e.SafeControlType() == ControlType.Text)
                .Select(e => e.SafeName())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
        }

        private static bool IsRow(AutomationElement e)
        {
            ControlType type = e.SafeControlType();
            return type == ControlType.ListItem || type == ControlType.DataItem || type == ControlType.TreeItem;
        }

        /// <summary>Строки списка/дерева (ListItem, DataItem, TreeItem): сначала прямые потомки, иначе обход поддерева.</summary>
        public static IReadOnlyList<AutomationElement> Rows(this AutomationElement list)
        {
            List<AutomationElement> direct = list.SafeChildren().Where(IsRow).ToList();
            return direct.Count > 0 ? direct : list.FindDescendantsBfs(IsRow, 6);
        }

        /// <summary>
        /// Все строки списка с учётом виртуализации: WPF ListView отдаёт в UI Automation только видимые строки,
        /// поэтому список прокручивается сверху вниз (ScrollPattern), строки собираются по имени без повторов.
        /// </summary>
        public static IReadOnlyList<AutomationElement> AllRows(this AutomationElement list)
        {
            List<AutomationElement> rows = new List<AutomationElement>();
            HashSet<string> seen = new HashSet<string>();
            Action collect = () =>
            {
                foreach (AutomationElement row in list.Rows())
                {
                    string key = row.SafeName() + "|" + string.Join("|", row.Texts());
                    if (seen.Add(key))
                    {
                        rows.Add(row);
                    }
                }
            };
            bool scrollable = false;
            FlaUI.Core.Patterns.IScrollPattern scroll = null;
            try
            {
                if (list.Patterns.Scroll.IsSupported)
                {
                    scroll = list.Patterns.Scroll.Pattern;
                    scrollable = scroll.VerticallyScrollable.Value;
                }
            }
            catch
            {
                scrollable = false;
            }
            if (!scrollable)
            {
                collect();
                return rows;
            }
            try
            {
                for (double percent = 0; percent <= 100; percent += 25)
                {
                    double p = percent;
                    Wait.Retry(() => scroll.SetScrollPercent(-1, p));
                    System.Threading.Thread.Sleep(150);
                    collect();
                }
                Wait.Retry(() => scroll.SetScrollPercent(-1, 0));
            }
            catch
            {
                collect();
            }
            return rows;
        }

        /// <summary>Найти строку с прокруткой списка; найденная строка прокручивается в видимую область (ScrollItem).</summary>
        public static AutomationElement FindRowScrolling(this AutomationElement list, Func<AutomationElement, bool> predicate)
        {
            AutomationElement found = list.Rows().FirstOrDefault(r => Safe(() => predicate(r)));
            if (found == null)
            {
                found = list.AllRows().FirstOrDefault(r => Safe(() => predicate(r)));
            }
            if (found != null)
            {
                try
                {
                    if (found.Patterns.ScrollItem.IsSupported)
                    {
                        found.Patterns.ScrollItem.Pattern.ScrollIntoView();
                    }
                }
                catch
                {
                }
            }
            return found;
        }

        /// <summary>Строка списка, тексты которой содержат все указанные фрагменты.</summary>
        public static AutomationElement RowContaining(this AutomationElement list, TimeSpan timeout, params string[] parts)
        {
            return Wait.For(() => list.TryRowContaining(parts), timeout, $"строка списка с текстом '{string.Join("', '", parts)}'");
        }

        public static AutomationElement TryRowContaining(this AutomationElement list, params string[] parts)
        {
            return list.Rows().FirstOrDefault(r => RowMatches(r, parts));
        }

        private static bool RowMatches(AutomationElement row, string[] parts)
        {
            string all = string.Join(" | ", row.Texts()) + " | " + row.SafeName();
            return parts.All(p => all.IndexOf(p, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        /// <summary>Текстовый дамп поддерева - для сообщений об ошибках и артефактов.</summary>
        public static string DumpTree(this AutomationElement root, int maxDepth = DefaultMaxDepth)
        {
            StringBuilder sb = new StringBuilder();
            Dump(root, sb, 0, maxDepth);
            return sb.ToString();
        }

        private static void Dump(AutomationElement element, StringBuilder sb, int depth, int maxDepth)
        {
            if (element == null || depth > maxDepth)
            {
                return;
            }
            string value = "";
            try
            {
                if (element.Patterns.Value.IsSupported)
                {
                    value = " value=\"" + Trunc(element.Patterns.Value.Pattern.Value.Value, 60) + "\"";
                }
            }
            catch
            {
            }
            sb.AppendLine($"{new string(' ', depth * 2)}[{element.SafeControlType()}] name=\"{Trunc(element.SafeName(), 90)}\" id=\"{element.SafeAutomationId()}\"{value}{(element.SafeIsEnabled() ? "" : " disabled")}{(element.SafeIsOffscreen() ? " offscreen" : "")}");
            foreach (AutomationElement child in element.SafeChildren())
            {
                Dump(child, sb, depth + 1, maxDepth);
            }
        }

        private static bool Safe(Func<bool> f)
        {
            try
            {
                return f();
            }
            catch
            {
                return false;
            }
        }

        private static string Trunc(string s, int len)
        {
            if (s == null)
            {
                return "";
            }
            s = s.Replace("\r", " ").Replace("\n", " ");
            return s.Length <= len ? s : s.Substring(0, len) + "…";
        }
    }
}
