using System;
using System.Collections.Generic;
using System.Linq;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Patterns;
using PilotUITesting.Uia;

namespace PilotUITesting.Client
{
    /// <summary>
    /// Список или дерево клиента (ascListView.ListView проводника, ascTreeView, tasksListView, annotationsListView): строки - ListItem/DataItem/TreeItem,
    /// заголовок строки - Name (или своя функция). Виртуализированные списки прокручиваются постранично до нужной строки.
    /// Выделение читается через SelectionItem.IsSelected строк (SelectionPattern.Selection не читается).
    /// </summary>
    public sealed class PilotList
    {
        private readonly PilotApp _app;
        private readonly Func<AutomationElement> _root;
        private readonly string _description;
        private readonly Func<AutomationElement, string> _rowTitle;

        internal PilotList(PilotApp app, Func<AutomationElement> root, string description, Func<AutomationElement, string> rowTitle)
        {
            _app = app;
            _root = root;
            _description = description;
            // сбой чтения не подменяется пустым заголовком: иначе «строки нет» при занятом клиенте
            _rowTitle = rowTitle ?? (row => UiQuery.Read(() => row.Properties.Name.ValueOrDefault ?? "", Uia.Wait.DefaultTimeout, "заголовок строки"));
        }

        public AutomationElement Root => _root() ?? throw new ElementNotFoundException(_description, "список", TimeSpan.Zero, TimeSpan.Zero, null, null);

        /// <summary>Заголовки строк, которые сейчас есть в дереве UI Automation (у виртуализированного списка - видимые).</summary>
        public IReadOnlyList<string> VisibleTitles()
        {
            return Rows(Root).Select(_rowTitle).ToList();
        }

        /// <summary>Строка с точным заголовком (с прокруткой списка); больше одной - неоднозначность; нет за timeout - исключение.</summary>
        public AutomationElement Row(string title, TimeSpan? timeout = null)
        {
            return Uia.Wait.ForStrict(() => TryRow(title), timeout ?? Uia.Wait.DefaultTimeout, $"строка «{title}» ({_description})",
                () => "видимые строки: " + string.Join("; ", VisibleTitles().Take(30)));
        }

        /// <summary>Строка с точным заголовком без ожидания (с прокруткой); null - нет. Клиент занят во время прокрутки - <see cref="ClientBusyException"/>.</summary>
        public AutomationElement TryRow(string title)
        {
            AutomationElement root = _root();
            if (root == null)
            {
                return null;
            }
            AutomationElement row = null;
            EachPage(root, false, scrolled =>
            {
                row = Matching(root, title);
                if (row != null && scrolled)
                {
                    ScrollIntoView(row);
                }
                return row != null;
            });
            return row;
        }

        /// <summary>Заголовки всех строк списка (виртуализированный список прокручивается сверху вниз), без повторов, в порядке просмотра.</summary>
        public IReadOnlyList<string> AllTitles()
        {
            AutomationElement root = Root;
            List<string> titles = new List<string>();
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            EachPage(root, true, _ =>
            {
                foreach (string title in Rows(root).Select(_rowTitle))
                {
                    if (seen.Add(title))
                    {
                        titles.Add(title);
                    }
                }
                return false;
            });
            return titles;
        }

        /// <summary>Добавить строку к выделению (SelectionItem.AddToSelection); постусловие - строка выделена, прежнее выделение сохранилось.</summary>
        public PilotList AddToSelection(string title)
        {
            AutomationElement row = Row(title);
            ISelectionItemPattern selection = UiQuery.Read(() => row.Patterns.SelectionItem.PatternOrDefault, Uia.Wait.DefaultTimeout, "SelectionItem строки")
                ?? throw new InvalidOperationException($"Строка «{title}» ({_description}) не поддерживает выделение (SelectionItemPattern).");
            IReadOnlyList<string> before = SelectedTitles();
            UiAct.RunOnce(row, $"добавить «{title}» к выделению", selection.AddToSelection, "AddToSelection");
            Uia.Wait.Strict(() =>
                {
                    IReadOnlyList<string> now = SelectedTitles();
                    return now.Contains(title) && before.All(now.Contains);
                }, Uia.Wait.DefaultTimeout, $"строка «{title}» добавлена к выделению ({_description})",
                () => "выделено: " + string.Join("; ", SelectedTitles()));
            return this;
        }

        /// <summary>
        /// Выделить ровно эти строки: первую - <see cref="Select"/> (с фокусом), остальные - <see cref="AddToSelection"/>; постусловие -
        /// множество выделенных строк совпадает с titles.
        /// </summary>
        public PilotList SelectMany(params string[] titles)
        {
            if (titles == null || titles.Length == 0)
            {
                throw new ArgumentException("Нужна хотя бы одна строка", nameof(titles));
            }
            Select(titles[0]);
            foreach (string title in titles.Skip(1))
            {
                AddToSelection(title);
            }
            HashSet<string> expected = new HashSet<string>(titles, StringComparer.Ordinal);
            Uia.Wait.Strict(() => expected.SetEquals(SelectedTitles()), Uia.Wait.DefaultTimeout, $"выделены ровно {titles.Length} строк ({_description})",
                () => "выделено: " + string.Join("; ", SelectedTitles()));
            return this;
        }

        public bool Has(string title)
        {
            return TryRow(title) != null;
        }

        /// <summary>Выделить строку и передать ей фокус клавиатуры (без фокуса не работают меню и клавиши); постусловие - выделена и в фокусе.</summary>
        public PilotList Select(string title)
        {
            AutomationElement row = Row(title);
            ISelectionItemPattern selection = UiQuery.Read(() => row.Patterns.SelectionItem.PatternOrDefault, Uia.Wait.DefaultTimeout, "SelectionItem строки");
            if (selection == null)
            {
                throw new InvalidOperationException($"Строка «{title}» ({_description}) не поддерживает выделение (SelectionItemPattern).");
            }
            UiAct.RunOnce(row, $"выделить строку «{title}»", selection.Select, "Select");
            UiQuery.Read(() =>
            {
                row.Focus();
                return true;
            }, Uia.Wait.DefaultTimeout, "фокус на строку");
            Uia.Wait.Strict(() => row.SafeIsSelected() && HasFocusWithin(row), Uia.Wait.DefaultTimeout, $"строка «{title}» выделена и в фокусе ({_description})", null);
            return this;
        }

        /// <summary>Заголовки выделенных строк (SelectionItem.IsSelected, среди строк в дереве UI Automation).</summary>
        public IReadOnlyList<string> SelectedTitles()
        {
            return Rows(Root).Where(r => r.SafeIsSelected()).Select(_rowTitle).ToList();
        }

        public void WaitRow(string title, TimeSpan? timeout = null)
        {
            Row(title, timeout);
        }

        /// <summary>Дождаться, что строки с заголовком нет (по полному обходу).</summary>
        public void WaitNoRow(string title, TimeSpan? timeout = null)
        {
            Uia.Wait.Strict(() => TryRow(title) == null, timeout ?? Uia.Wait.DefaultTimeout, $"строки «{title}» нет ({_description})", null);
        }

        public override string ToString()
        {
            return _description;
        }

        private AutomationElement Matching(AutomationElement root, string title)
        {
            List<AutomationElement> found = Rows(root).Where(r => _rowTitle(r) == title).ToList();
            if (found.Count > 1)
            {
                throw new AmbiguousElementException($"строка «{title}» ({_description})", found.Select(f => f.Describe()).ToList());
            }
            return found.FirstOrDefault();
        }

        internal static IReadOnlyList<AutomationElement> Rows(AutomationElement list)
        {
            UiQueryResult faults = new UiQueryResult();
            List<AutomationElement> direct = UiQuery.Children(list, faults).Where(IsRow).ToList();
            if (!faults.Complete)
            {
                throw new ClientBusyException("строки списка", Uia.Wait.DefaultTimeout, TimeSpan.Zero, faults.BusyFaults, faults.LastFault);
            }
            if (direct.Count > 0)
            {
                return direct;
            }
            return UiQuery.FindAllComplete(list, By.Where(f => IsRowType(f.ControlType), "строка"), Uia.Wait.DefaultTimeout, 6);
        }

        /// <summary>
        /// Обойти список по страницам: visit(scrolled) - true остановит обход. Непрокручиваемый список - одна страница (текущий вид).
        /// fromTop = false - сначала текущий вид без прокрутки. ScrollPattern WPF только ставит команду в очередь (выполняется на следующем
        /// проходе layout): после каждой прокрутки ждём её результата, иначе прочитали бы прежнюю позицию и прежние строки.
        /// </summary>
        private void EachPage(AutomationElement root, bool fromTop, Func<bool, bool> visit)
        {
            IScrollPattern scroll = UiQuery.Read(() => root.Patterns.Scroll.PatternOrDefault, Uia.Wait.DefaultTimeout, "ScrollPattern " + _description);
            bool scrollable = scroll != null && UiQuery.Read(() => scroll.VerticallyScrollable.ValueOrDefault, Uia.Wait.DefaultTimeout, "прокручиваемость");
            if (!scrollable)
            {
                visit(false);
                return;
            }
            if (!fromTop && visit(false))
            {
                return;
            }
            UiQuery.Read(() =>
            {
                scroll.SetScrollPercent(ScrollPatternConstants.NoScroll, 0);
                return true;
            }, Uia.Wait.DefaultTimeout, "прокрутка в начало");
            Uia.Wait.Strict(() => Percent(scroll) <= 0, Uia.Wait.DefaultTimeout, $"прокрутка в начало ({_description})", () => "позиция " + Percent(scroll));
            for (int page = 0; page < 1000; page++)
            {
                if (visit(true))
                {
                    return;
                }
                double before = Percent(scroll);
                if (before >= ScrollPatternConstants.End)
                {
                    return;
                }
                try
                {
                    scroll.Scroll(ScrollAmount.NoAmount, ScrollAmount.LargeIncrement);
                }
                catch (Exception ex) when (UiFault.Classify(ex) == UiFaultKind.Busy)
                {
                    // вызов уже мог встать в очередь клиента: повтор пролистал бы лишнюю страницу мимо непросмотренных строк.
                    // Проход прерывается; ожидания (Row, WaitNoRow) начнут его заново с начала списка
                    throw new ClientBusyException($"прокрутка «{_description}» на страницу", Uia.Wait.DefaultTimeout, TimeSpan.Zero, 1, ex);
                }
                // позиция меньше конца - страница вниз обязана сдвинуть список
                Uia.Wait.Strict(() => Percent(scroll) > before, Uia.Wait.DefaultTimeout, $"прокрутка на страницу ({_description})", () => "позиция " + Percent(scroll) + ", была " + before);
            }
            throw new InvalidOperationException($"{_description}: прокрутка не дошла до конца за 1000 страниц.");
        }

        private double Percent(IScrollPattern scroll)
        {
            return UiQuery.Read(() => scroll.VerticalScrollPercent.Value, Uia.Wait.DefaultTimeout, "позиция прокрутки " + _description);
        }

        /// <summary>Строка ли это; сбой чтения не подменяется ответом «нет» (занят - повтор, исчез - исключение для ожидания).</summary>
        private static bool IsRow(AutomationElement e)
        {
            return IsRowType(UiQuery.Read(() => e.Properties.ControlType.Value, Uia.Wait.DefaultTimeout, "тип элемента списка"));
        }

        private static bool IsRowType(ControlType type)
        {
            return type == ControlType.ListItem || type == ControlType.DataItem || type == ControlType.TreeItem;
        }

        private static bool HasFocusWithin(AutomationElement row)
        {
            if (row.SafeHasKeyboardFocus())
            {
                return true;
            }
            return UiQuery.Find(row, By.Where(f => f.Element.Properties.HasKeyboardFocus.ValueOrDefault, "фокус"), 6, 1).Matches.Count > 0;
        }

        private static void ScrollIntoView(AutomationElement row)
        {
            IScrollItemPattern item = UiQuery.Read(() => row.Patterns.ScrollItem.PatternOrDefault, Uia.Wait.DefaultTimeout, "ScrollItem строки");
            if (item != null)
            {
                UiQuery.Read(() =>
                {
                    item.ScrollIntoView();
                    return true;
                }, Uia.Wait.DefaultTimeout, "прокрутка к строке");
            }
        }
    }

    internal static class ScrollPatternConstants
    {
        /// <summary>UIA_ScrollPatternNoScroll: не менять прокрутку по этой оси.</summary>
        internal const double NoScroll = -1;

        /// <summary>Конец прокрутки: 100 % с допуском на погрешность деления offset*100/(extent - viewport) в double.</summary>
        internal const double End = 100 - 1e-6;
    }
}
