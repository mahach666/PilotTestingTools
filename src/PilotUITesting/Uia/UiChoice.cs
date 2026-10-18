using System;
using System.Collections.Generic;
using System.Linq;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Patterns;
using PilotUITesting.Client;

namespace PilotUITesting.Uia
{
    /// <summary>Способ переключить флажок.</summary>
    public enum CheckInput
    {
        /// <summary>TogglePattern.Toggle (по умолчанию).</summary>
        TogglePattern,

        /// <summary>
        /// Клик мышью: для окон модулей, где доступность кнопок пересчитывает CommandManager только на ввод пользователя
        /// (программный Toggle не создаёт ввода). Требует DPI-осведомлённости процесса тестов.
        /// </summary>
        MouseClick,
    }

    /// <summary>Выбор пункта раскрывающегося списка и флажки - общая логика для карточек, окон модулей и страниц.</summary>
    internal static class UiChoice
    {
        /// <summary>
        /// Раскрыть список (ExpandCollapse), найти пункт ListItem с точной подписью (под списком или во всплывающем окне), выбрать его:
        /// SelectionItem.Select, а если паттерна нет - кликом мыши. Если у списка есть ValuePattern - дождаться этого значения.
        /// Возвращает подписи всех пунктов этого открытия.
        /// </summary>
        internal static IReadOnlyList<string> Choose(PilotApp app, AutomationElement combo, string itemCaption, string description)
        {
            if (combo == null)
            {
                throw new ArgumentNullException(nameof(combo));
            }
            IExpandCollapsePattern expand = UiQuery.Read(() => combo.Patterns.ExpandCollapse.PatternOrDefault, Wait.DefaultTimeout, "ExpandCollapse " + description)
                ?? throw new InvalidOperationException($"{description}: не раскрывается (нет ExpandCollapsePattern) - это не список ({combo.Describe()}).");
            UiAct.RunOnce(combo, "раскрыть " + description, expand.Expand, "Expand");
            By item = By.Where(f => f.ControlType == ControlType.ListItem && Caption(f.Element) == itemCaption, $"пункт «{itemCaption}»");
            AutomationElement found = Wait.ForStrict(() =>
                UiQuery.TryFind(combo, item, Wait.DefaultTimeout, 6) ?? app.Windows.Popups().Select(p => UiQuery.TryFind(p, item, Wait.DefaultTimeout, 8)).FirstOrDefault(e => e != null),
                Wait.DefaultTimeout, $"пункт «{itemCaption}» в {description}", null);
            IReadOnlyList<string> captions = Siblings(found);
            ISelectionItemPattern select = UiQuery.Read(() => found.Patterns.SelectionItem.PatternOrDefault, Wait.DefaultTimeout, "SelectionItem пункта");
            if (select != null)
            {
                UiAct.RunOnce(found, $"выбрать «{itemCaption}» в {description}", select.Select, "Select");
            }
            else
            {
                UiAct.Click(found, $"выбрать «{itemCaption}» в {description}", MouseButton.Left, false);
            }
            if (UiQuery.Read(() => combo.Patterns.Value.IsSupported, Wait.DefaultTimeout, "ValuePattern " + description))
            {
                string actual = "";
                if (!Wait.TryUntil(() => (actual = combo.SafeValue()) == itemCaption, TimeSpan.FromSeconds(3)))
                {
                    throw new ValueNotAppliedException(description, itemCaption, actual);
                }
            }
            return captions;
        }

        /// <summary>Подписи пунктов списка: раскрыть, прочитать, свернуть (ExpandCollapse.Collapse) с проверкой свёрнутости.</summary>
        internal static IReadOnlyList<string> Items(PilotApp app, AutomationElement combo, string description)
        {
            IExpandCollapsePattern expand = UiQuery.Read(() => combo.Patterns.ExpandCollapse.PatternOrDefault, Wait.DefaultTimeout, "ExpandCollapse " + description)
                ?? throw new InvalidOperationException($"{description}: не раскрывается (нет ExpandCollapsePattern) - это не список ({combo.Describe()}).");
            UiAct.RunOnce(combo, "раскрыть " + description, expand.Expand, "Expand");
            By anyItem = By.Type(ControlType.ListItem);
            AutomationElement first = Wait.ForStrict(() =>
                UiQuery.TryFind(combo, anyItem, Wait.DefaultTimeout, 6) ?? app.Windows.Popups().Select(p => UiQuery.TryFind(p, anyItem, Wait.DefaultTimeout, 8)).FirstOrDefault(e => e != null),
                Wait.DefaultTimeout, "пункты " + description, null);
            IReadOnlyList<string> captions = Siblings(first);
            UiAct.RunOnce(combo, "свернуть " + description, expand.Collapse, "Collapse");
            Wait.Strict(() => UiQuery.Read(() => expand.ExpandCollapseState.Value, Wait.DefaultTimeout, "раскрытие " + description) == ExpandCollapseState.Collapsed,
                Wait.DefaultTimeout, description + " свёрнут", null);
            return captions;
        }

        /// <summary>Флажок в заданное состояние; постусловие - прочитанное состояние.</summary>
        internal static void SetChecked(AutomationElement element, bool value, CheckInput input, string description)
        {
            ToggleState target = value ? ToggleState.On : ToggleState.Off;
            if (State(element, description) == target)
            {
                return;
            }
            if (input == CheckInput.MouseClick)
            {
                UiAct.Click(element, (value ? "отметить " : "снять ") + description, MouseButton.Left, false);
            }
            else
            {
                ITogglePattern toggle = UiQuery.Read(() => element.Patterns.Toggle.PatternOrDefault, Wait.DefaultTimeout, "Toggle " + description);
                UiAct.RunOnce(element, (value ? "отметить " : "снять ") + description, toggle.Toggle, "Toggle");
            }
            Wait.Strict(() => State(element, description) == target, Wait.DefaultTimeout, $"{description} = {(value ? "вкл" : "выкл")}", () => State(element, description).ToString());
        }

        /// <summary>Состояние TogglePattern без подмены сбоя; нет паттерна - исключение.</summary>
        internal static ToggleState State(AutomationElement element, string description)
        {
            ToggleState? state = UiQuery.Read(() => element.Patterns.Toggle.PatternOrDefault?.ToggleState.Value, Wait.DefaultTimeout, "состояние " + description);
            return state ?? throw new InvalidOperationException($"{description}: нет TogglePattern ({element.Describe()}).");
        }

        /// <summary>
        /// Подпись пункта списка: текст дочернего TextBlock, если он есть, иначе Name. У пунктов с моделью-объектом Name - имя типа .NET,
        /// а подпись только в тексте (проверено: ComboBoxEdit DevExpress в реестре, PluginUserState).
        /// </summary>
        internal static string Caption(AutomationElement item)
        {
            return item.Texts().FirstOrDefault() ?? item.SafeName();
        }

        private static IReadOnlyList<string> Siblings(AutomationElement item)
        {
            AutomationElement parent = UiQuery.Read(() => item.Parent, Wait.DefaultTimeout, "контейнер пунктов");
            if (parent == null)
            {
                return new[] { Caption(item) };
            }
            AutomationElement[] children = UiQuery.Read(() => parent.FindAllChildren(), Wait.DefaultTimeout, "пункты списка");
            return children.Where(c => UiQuery.Read(() => c.Properties.ControlType.Value, Wait.DefaultTimeout, "тип пункта") == ControlType.ListItem).Select(Caption).ToList();
        }
    }
}
