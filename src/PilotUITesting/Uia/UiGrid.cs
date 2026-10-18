using System;
using System.Collections.Generic;
using System.Linq;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Patterns;

namespace PilotUITesting.Uia
{
    /// <summary>Колонка таблицы: FieldName (Name заголовка) и подпись (текст заголовка).</summary>
    public sealed class UiGridColumn
    {
        internal UiGridColumn(string fieldName, string caption)
        {
            FieldName = fieldName;
            Caption = caption;
        }

        public string FieldName { get; }

        public string Caption { get; }

        public override string ToString()
        {
            return $"{FieldName} «{Caption}»";
        }
    }

    /// <summary>
    /// Таблица с GridPattern. Устройство проверено на DevExpress GridControl 20.1 (вкладка модуля, Pilot-ICE 26.11):
    /// заголовки - Header -> HeaderItem (Name = FieldName колонки, подпись - в дочернем Text; Invoke по заголовку сортирует - не вызывается);
    /// строки - DataItem; ячейка - элемент строки с AutomationId = FieldName, значение - ValuePattern.Value (показанный текст).
    /// SetValue по ячейке не вызывается: у DevExpress IsReadOnly = False, запись изменила бы данные.
    /// WPF DataGrid (проверено на .NET Framework 4.8): HeaderItem с Name = заголовок, у ячеек нет AutomationId - ячейка по номеру колонки
    /// (GridPattern.GetItem), у колонок-шаблонов Value пуст - текст берётся из содержимого. Скрытые колонки WPF DataGrid не поддержаны (исключение).
    /// </summary>
    public sealed class UiGrid
    {
        private readonly Func<AutomationElement> _root;
        private readonly string _description;

        public UiGrid(Func<AutomationElement> root, string description)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
            _description = description;
        }

        public AutomationElement Root => _root() ?? throw new ElementNotFoundException(_description, "таблица", TimeSpan.Zero, TimeSpan.Zero, null, null);

        /// <summary>Число строк (GridPattern.RowCount).</summary>
        public int RowCount => UiQuery.Read(() => GridPattern(Root).RowCount.Value, Wait.DefaultTimeout, "число строк " + _description);

        /// <summary>Колонки по заголовкам таблицы.</summary>
        public IReadOnlyList<UiGridColumn> Columns()
        {
            AutomationElement root = Root;
            AutomationElement header = UiQuery.TryFind(root, By.Type(ControlType.Header), Wait.DefaultTimeout, 4)
                ?? throw new InvalidOperationException($"{_description}: нет заголовка (Header).");
            UiQueryResult faults = new UiQueryResult();
            return UiQuery.Children(header, faults)
                .Where(h => h.SafeControlType() == ControlType.HeaderItem)
                .Select(h => new UiGridColumn(h.SafeName(), h.SafeChildren().Where(c => c.SafeControlType() == ControlType.Text).Select(c => c.SafeName()).FirstOrDefault(t => t.Length > 0) ?? ""))
                .ToList();
        }

        /// <summary>
        /// Показанный текст ячейки. column - FieldName или подпись колонки.
        /// Value пуст (колонки-шаблоны WPF DataGrid) - тексты внутри ячейки через перевод строки.
        /// </summary>
        public string Cell(int row, string column)
        {
            AutomationElement cell = CellElement(row, column);
            string value = UiQuery.Read(() => cell.Patterns.Value.PatternOrDefault?.Value.ValueOrDefault ?? "", Wait.DefaultTimeout, $"ячейка [{row}, {column}]");
            return value.Length > 0 ? value : string.Join(Environment.NewLine, cell.Texts());
        }

        /// <summary>Значения колонки во всех строках (ровно RowCount).</summary>
        public IReadOnlyList<string> Column(string column)
        {
            int count = RowCount;
            return Enumerable.Range(0, count).Select(r => Cell(r, column)).ToList();
        }

        /// <summary>Строки как словари «FieldName -> текст» по указанным колонкам.</summary>
        public IReadOnlyList<IReadOnlyDictionary<string, string>> ReadAll(params string[] columns)
        {
            string[] fields = columns.Select(FieldName).ToArray();
            int count = RowCount;
            List<IReadOnlyDictionary<string, string>> rows = new List<IReadOnlyDictionary<string, string>>();
            for (int r = 0; r < count; r++)
            {
                Dictionary<string, string> values = new Dictionary<string, string>(StringComparer.Ordinal);
                foreach (string field in fields)
                {
                    values[field] = Cell(r, field);
                }
                rows.Add(values);
            }
            return rows;
        }

        /// <summary>Номер строки со значением в колонке; -1 - нет; больше одной - неоднозначность.</summary>
        public int FindRow(string column, string value)
        {
            List<int> matches = new List<int>();
            IReadOnlyList<string> values = Column(column);
            for (int r = 0; r < values.Count; r++)
            {
                if (values[r] == value)
                {
                    matches.Add(r);
                }
            }
            if (matches.Count > 1)
            {
                throw new AmbiguousElementException($"строка с «{value}» в колонке {column} ({_description})", matches.Select(m => "строка " + m).ToList());
            }
            return matches.Count == 0 ? -1 : matches[0];
        }

        /// <summary>
        /// Элемент ячейки: в строке (DataItem) элемент с AutomationId = FieldName (DevExpress). У ячеек строки нет AutomationId (WPF DataGrid) -
        /// GridPattern.GetItem(row, номер колонки среди заголовков).
        /// </summary>
        public AutomationElement CellElement(int row, string column)
        {
            string field = FieldName(column);
            AutomationElement rowElement = RowElement(row);
            UiQueryResult faults = new UiQueryResult();
            IReadOnlyList<AutomationElement> cells = UiQuery.Children(rowElement, faults);
            AutomationElement cell = cells.FirstOrDefault(c => c.SafeAutomationId() == field);
            if (cell != null)
            {
                return cell;
            }
            if (cells.All(c => c.SafeAutomationId().Length == 0))
            {
                // номер колонки - порядок заголовков; GetItem WPF DataGrid считает и скрытые колонки (заголовков у них нет), тогда номера разъезжаются
                List<string> fields = Columns().Select(c => c.FieldName).ToList();
                AutomationElement root = Root;
                int columnCount = UiQuery.Read(() => GridPattern(root).ColumnCount.Value, Wait.DefaultTimeout, "число колонок " + _description);
                if (columnCount != fields.Count)
                {
                    throw new InvalidOperationException($"{_description}: колонок в GridPattern {columnCount}, заголовков {fields.Count} (есть скрытые колонки) - ячейку по номеру колонки не найти.");
                }
                int index = fields.IndexOf(field);
                return UiQuery.Read(() => GridPattern(root).GetItem(row, index), Wait.DefaultTimeout, $"ячейка [{row}, {index}]")
                    ?? throw new InvalidOperationException($"{_description}: GridPattern.GetItem({row}, {index}) не вернул ячейку колонки {field}.");
            }
            throw new InvalidOperationException($"{_description}: в строке {row} нет ячейки {field} (колонка скрыта?). Ячейки: {string.Join(", ", rowElement.SafeChildren().Select(c => c.SafeAutomationId()))}");
        }

        /// <summary>
        /// Строка (DataItem) - родитель ячейки GridPattern.GetItem(row, 0). Строка вне дерева UI Automation (вертикальная виртуализация не проверена) -
        /// исключение, а не пропуск.
        /// </summary>
        public AutomationElement RowElement(int row)
        {
            AutomationElement root = Root;
            int count = RowCount;
            if (row < 0 || row >= count)
            {
                throw new ArgumentOutOfRangeException(nameof(row), $"{_description}: строк {count}, запрошена {row}.");
            }
            AutomationElement cell = UiQuery.Read(() => GridPattern(root).GetItem(row, 0), Wait.DefaultTimeout, $"ячейка [{row}, 0]")
                ?? throw new InvalidOperationException($"{_description}: GridPattern.GetItem({row}, 0) не вернул ячейку (строка не материализована в дереве UI Automation).");
            AutomationElement parent = UiQuery.Read(() => cell.Parent, Wait.DefaultTimeout, "строка ячейки");
            if (parent == null || parent.SafeControlType() != ControlType.DataItem)
            {
                throw new InvalidOperationException($"{_description}: родитель ячейки [{row}, 0] не DataItem ({parent.Describe()}).");
            }
            return parent;
        }

        /// <summary>Выделить строку (SelectionItem.Select) и передать фокус таблице.</summary>
        public void SelectRow(int row)
        {
            AutomationElement rowElement = RowElement(row);
            ISelectionItemPattern selection = UiQuery.Read(() => rowElement.Patterns.SelectionItem.PatternOrDefault, Wait.DefaultTimeout, "SelectionItem строки")
                ?? throw new InvalidOperationException($"{_description}: строка {row} не поддерживает выделение.");
            UiAct.RunOnce(rowElement, $"выделить строку {row} ({_description})", selection.Select, "Select");
            AutomationElement root = Root;
            UiQuery.Read(() =>
            {
                root.Focus();
                return true;
            }, Wait.DefaultTimeout, "фокус на таблицу");
            Wait.Strict(() => rowElement.SafeIsSelected(), Wait.DefaultTimeout, $"строка {row} выделена ({_description})", null);
        }

        public override string ToString()
        {
            return _description;
        }

        private string FieldName(string column)
        {
            IReadOnlyList<UiGridColumn> columns = Columns();
            List<UiGridColumn> byField = columns.Where(c => c.FieldName == column).ToList();
            List<UiGridColumn> byCaption = columns.Where(c => c.Caption == column).ToList();
            if (byField.Count == 1 && byCaption.Count > 0 && byCaption.Any(c => c.FieldName != column))
            {
                throw new AmbiguousElementException($"колонка «{column}» ({_description})", byField.Concat(byCaption).Select(c => c.ToString()).ToList());
            }
            if (byField.Count == 1)
            {
                return column;
            }
            if (byCaption.Count == 1)
            {
                return byCaption[0].FieldName;
            }
            if (byCaption.Count > 1)
            {
                throw new AmbiguousElementException($"колонка «{column}» ({_description})", byCaption.Select(c => c.ToString()).ToList());
            }
            throw new InvalidOperationException($"{_description}: нет колонки «{column}». Колонки: {string.Join(", ", columns)}");
        }

        private IGridPattern GridPattern(AutomationElement root)
        {
            return root.Patterns.Grid.PatternOrDefault ?? throw new InvalidOperationException($"{_description}: элемент не поддерживает GridPattern ({root.Describe()}).");
        }
    }
}
