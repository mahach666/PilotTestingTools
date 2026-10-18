using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Patterns;
using NUnit.Framework;
using PilotUITesting.Uia;
using Wait = PilotUITesting.Uia.Wait;

namespace PilotUITesting.Client
{
    /// <summary>
    /// Основа page object: всё ищется внутри <see cref="Root"/> (а не от главного окна: там бывают устаревшие копии содержимого),
    /// ожидания без явного таймаута (<see cref="Wait.DefaultTimeout"/>), нажатие - один раз, ошибки с описанием области и id вокруг.
    /// Наследник задаёт, где его корень (<see cref="ResolveRoot"/>).
    /// </summary>
    public abstract class PilotPage
    {
        protected PilotPage(PilotApp app, string description)
        {
            App = app ?? throw new ArgumentNullException(nameof(app));
            Description = description;
        }

        public PilotApp App { get; }

        /// <summary>Что это за область (для сообщений): «вкладка «Проекты и документы»», «окно «Создание»».</summary>
        public string Description { get; }

        /// <summary>Актуальный корень области; нет - <see cref="ElementNotFoundException"/>.</summary>
        public AutomationElement Root => ResolveRoot() ?? throw new ElementNotFoundException(Description, "корень области", TimeSpan.Zero, TimeSpan.Zero, null, null);

        /// <summary>Корень области сейчас; null - области нет (вкладка закрыта, окно не открыто).</summary>
        protected abstract AutomationElement ResolveRoot();

        public AutomationElement Find(string automationId, TimeSpan? timeout = null)
        {
            return Find(By.Id(automationId), timeout);
        }

        /// <summary>Элемент по селектору (первый в порядке обхода в ширину).</summary>
        public AutomationElement Find(By selector, TimeSpan? timeout = null)
        {
            return UiQuery.WaitFind(ResolveRoot, selector, timeout ?? Wait.DefaultTimeout, Description);
        }

        /// <summary>Элемент без ожидания; null - только по полному обходу (клиент ответил на все запросы).</summary>
        public AutomationElement TryFind(string automationId)
        {
            return TryFind(By.Id(automationId));
        }

        public AutomationElement TryFind(By selector)
        {
            AutomationElement root = ResolveRoot();
            return root == null ? null : UiQuery.TryFind(root, selector, Wait.DefaultTimeout);
        }

        public bool Has(string automationId)
        {
            return TryFind(automationId) != null;
        }

        public bool Has(By selector)
        {
            return TryFind(selector) != null;
        }

        /// <summary>Элемента нет: полные обходы без совпадений всё время stableFor (по умолчанию 1 с). Иначе - провал проверки NUnit.</summary>
        public void AssertAbsent(By selector, TimeSpan? stableFor = null)
        {
            if (!Wait.Stays(() => TryFind(selector) == null, stableFor ?? TimeSpan.FromSeconds(1), "нет " + selector))
            {
                Assert.Fail($"В области «{Description}» есть элемент {selector}, хотя его быть не должно.");
            }
        }

        /// <summary>Дождаться доступности элемента и нажать один раз (Invoke в фоне, без повтора).</summary>
        public void Click(string automationId, TimeSpan? timeout = null)
        {
            Click(By.Id(automationId), timeout);
        }

        public void Click(By selector, TimeSpan? timeout = null)
        {
            AutomationElement element = WaitEnabled(selector, timeout);
            UiAct.Press(element, selector + " в «" + Description + "»");
        }

        /// <summary>Кнопка без AutomationId по подписи (Name; знак ускорителя «_» в Name не входит). Больше одной - неоднозначность.</summary>
        public void ClickText(string caption, TimeSpan? timeout = null)
        {
            By selector = By.Type(ControlType.Button).And(By.Name(caption));
            AutomationElement button = Single(selector, timeout);
            Wait.Strict(() => button.SafeIsEnabled(), timeout ?? Wait.DefaultTimeout, $"кнопка «{caption}» доступна", null);
            UiAct.Press(button, $"кнопка «{caption}» в «{Description}»");
        }

        /// <summary>Клик мышью - для элементов, у которых Invoke не срабатывает (fallIntoButton, редактор ячейки DevExpress).</summary>
        public void MouseClick(By selector)
        {
            UiAct.Click(Find(selector), selector.ToString(), MouseButton.Left, false);
        }

        public void DoubleClick(By selector)
        {
            UiAct.Click(Find(selector), selector.ToString(), MouseButton.Left, true);
        }

        public void RightClick(By selector)
        {
            UiAct.Click(Find(selector), selector.ToString(), MouseButton.Right, false);
        }

        /// <summary>Записать значение через ValuePattern и прочитать обратно; не применилось - <see cref="ValueNotAppliedException"/>.</summary>
        public void SetValue(string automationId, string value)
        {
            SetValue(Find(automationId), value, "поле " + automationId + " в «" + Description + "»");
        }

        /// <summary>Значение элемента: Value, иначе Name, иначе тексты поддерева.</summary>
        public string ValueOf(string automationId)
        {
            AutomationElement element = Find(automationId);
            string value = element.SafeValue();
            if (value.Length > 0)
            {
                return value;
            }
            string name = element.SafeName();
            return name.Length > 0 ? name : string.Join(" ", element.Texts());
        }

        /// <summary>Значение единственного элемента по селектору: Value, иначе Name, иначе тексты поддерева.</summary>
        public string ValueOf(By selector)
        {
            AutomationElement element = Single(selector, null);
            string value = UiQuery.Read(() => element.Patterns.Value.PatternOrDefault?.Value.Value, Wait.DefaultTimeout, "Value " + selector);
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }
            string name = UiQuery.Read(() => element.Properties.Name.ValueOrDefault ?? "", Wait.DefaultTimeout, "Name " + selector);
            return name.Length > 0 ? name : string.Join(" ", element.Texts());
        }

        /// <summary>Записать значение в единственный элемент по селектору (ValuePattern) и прочитать обратно.</summary>
        public void SetValue(By selector, string value)
        {
            SetValue(Single(selector, null), value, selector + " в «" + Description + "»");
        }

        /// <summary>Доступность единственного элемента по селектору; сбой чтения - исключение, а не «недоступен».</summary>
        public bool IsEnabled(By selector)
        {
            AutomationElement element = Single(selector, null);
            return UiQuery.Read(() => element.Properties.IsEnabled.Value, Wait.DefaultTimeout, "доступность " + selector);
        }

        /// <summary>Состояние флажка (TogglePattern) единственного элемента; null - у элемента нет TogglePattern.</summary>
        public bool? IsChecked(By selector)
        {
            AutomationElement element = Single(selector, null);
            ToggleState? state = UiQuery.Read(() => element.Patterns.Toggle.PatternOrDefault?.ToggleState.Value, Wait.DefaultTimeout, "состояние " + selector);
            return state == null ? (bool?)null : state == ToggleState.On;
        }

        /// <summary>Флажок в состояние value; постусловие - прочитанное состояние.</summary>
        public void SetChecked(By selector, bool value, CheckInput input = CheckInput.TogglePattern)
        {
            UiChoice.SetChecked(Single(selector, null), value, input, selector + " в «" + Description + "»");
        }

        /// <summary>Подписи пунктов раскрывающегося списка (раскрыть, прочитать, свернуть).</summary>
        public IReadOnlyList<string> ComboItems(By combo)
        {
            return UiChoice.Items(App, Single(combo, null), combo + " в «" + Description + "»");
        }

        /// <summary>Выбрать в раскрывающемся списке пункт с точной подписью; возвращает подписи всех пунктов.</summary>
        public IReadOnlyList<string> Choose(By combo, string itemCaption)
        {
            return UiChoice.Choose(App, Single(combo, null), itemCaption, combo + " в «" + Description + "»");
        }

        /// <summary>Все непустые тексты области.</summary>
        public IReadOnlyList<string> Texts()
        {
            return Root.Texts();
        }

        /// <summary>Первый текст области, удовлетворяющий условию; null - нет.</summary>
        public string TryText(Func<string, bool> match)
        {
            return Texts().FirstOrDefault(match);
        }

        public string WaitText(Func<string, bool> match, TimeSpan? timeout = null, [CallerArgumentExpression("match")] string description = null)
        {
            return Wait.ForStrict(() => TryText(match), timeout ?? Wait.DefaultTimeout, "текст " + description + " в «" + Description + "»", null);
        }

        /// <summary>Панель команд (ToolbarStripe) по её AutomationId.</summary>
        public PilotToolbar ToolbarById(string toolbarId)
        {
            return new PilotToolbar(App, () => TryFind(toolbarId), "панель " + toolbarId + " в «" + Description + "»");
        }

        /// <summary>Панель без AutomationId (панели модулей): её корень - UIA-родитель любой кнопки панели.</summary>
        public PilotToolbar ToolbarWith(string anyButtonId)
        {
            return new PilotToolbar(App, () => TryFind(anyButtonId)?.Parent, "панель с кнопкой " + anyButtonId + " в «" + Description + "»");
        }

        /// <summary>Карточка атрибутов (поля AUID&lt;атрибут&gt;) во всей области.</summary>
        public PilotCard Card => new PilotCard(App, ResolveRoot, "карточка в «" + Description + "»");

        /// <summary>Карточка атрибутов внутри контейнера с AutomationId (ascTaskCard, workflowCard, executorsCard, ObjectCardView…).</summary>
        public PilotCard CardIn(string containerId)
        {
            return new PilotCard(App, () => TryFind(containerId), "карточка " + containerId + " в «" + Description + "»");
        }

        /// <summary>Список или дерево по AutomationId; rowTitle - как получить заголовок строки (по умолчанию Name).</summary>
        public PilotList ListById(string listId, Func<AutomationElement, string> rowTitle = null)
        {
            return new PilotList(App, () => TryFind(listId), "список " + listId + " в «" + Description + "»", rowTitle);
        }

        /// <summary>Список или дерево по селектору (списки без AutomationId); rowTitle - заголовок строки (по умолчанию Name).</summary>
        public PilotList ListBy(By selector, Func<AutomationElement, string> rowTitle = null)
        {
            return new PilotList(App, () => TryFind(selector), "список " + selector + " в «" + Description + "»", rowTitle);
        }

        /// <summary>Таблица (GridPattern; проверено на DevExpress GridControl 20.1).</summary>
        public UiGrid GridById(string gridId)
        {
            return new UiGrid(() => TryFind(gridId), "таблица " + gridId + " в «" + Description + "»");
        }

        /// <summary>Подробный дамп области (для разбора в тесте).</summary>
        public string Dump(int maxDepth = 40)
        {
            return Root.DumpTree(maxDepth, true);
        }

        public override string ToString()
        {
            return Description;
        }

        internal AutomationElement WaitEnabled(By selector, TimeSpan? timeout)
        {
            TimeSpan limit = timeout ?? Wait.DefaultTimeout;
            Find(selector, limit);
            // элемент ищется заново на каждом опросе: найденный заранее мог быть пересоздан (панель модуля после загрузки данных)
            return Wait.ForStrict(() =>
            {
                AutomationElement element = TryFind(selector);
                return element != null && element.Properties.IsEnabled.Value ? element : null;
            }, limit, selector + " доступен в «" + Description + "»", null);
        }

        internal AutomationElement Single(By selector, TimeSpan? timeout)
        {
            TimeSpan limit = timeout ?? Wait.DefaultTimeout;
            Find(selector, limit);
            IReadOnlyList<AutomationElement> all = Root.FindAll(selector);
            if (all.Count > 1)
            {
                throw new AmbiguousElementException(selector + " в «" + Description + "»", all.Select(e => e.Describe()).ToList());
            }
            if (all.Count == 0)
            {
                // элемент был найден и тут же исчез - поиск ещё раз с ожиданием
                return Find(selector, limit);
            }
            return all[0];
        }

        internal static void SetValue(AutomationElement element, string value, string target)
        {
            IValuePattern pattern = UiQuery.Read(() => element.Patterns.Value.PatternOrDefault, Wait.DefaultTimeout, "ValuePattern " + target);
            if (pattern == null)
            {
                throw new InvalidOperationException($"{target}: нет ValuePattern - ввод значения через UI Automation невозможен ({element.Describe()}).");
            }
            PilotLog.Action($"значение «{value}» -> {target}");
            pattern.SetValue(value ?? "");
            string actual = "";
            if (!Wait.TryUntil(() => (actual = element.SafeValue()) == (value ?? ""), TimeSpan.FromSeconds(3)))
            {
                throw new ValueNotAppliedException(target, value, actual);
            }
        }
    }
}
