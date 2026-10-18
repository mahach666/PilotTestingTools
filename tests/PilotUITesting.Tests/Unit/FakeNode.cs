using System;
using System.Collections.Generic;
using System.Drawing;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.Core.EventHandlers;
using FlaUI.Core.Identifiers;
using FlaUI.Core.Patterns;
using FlaUI.UIA3.Converters;
using UiaControlType = FlaUI.Core.Definitions.ControlType;

namespace PilotUITesting.Tests.Unit
{
    /// <summary>
    /// Узел дерева UI Automation без клиента: дети и свойства (AutomationId, Name, ClassName, ControlType) задаются в тесте,
    /// сбои запросов (занят, исчез) - функциями OnFindChildren/OnProperty. Прочие члены FlaUI не поддерживаются.
    /// </summary>
    internal sealed class FakeNode : FrameworkAutomationElementBase
    {
        internal FakeNode(AutomationBase automation, string id, UiaControlType type = UiaControlType.Pane, string name = "", string className = "")
            : base(automation)
        {
            FakeId = id;
            FakeType = type;
            FakeName = name;
            FakeClassName = className;
        }

        internal string FakeId { get; }

        internal UiaControlType FakeType { get; }

        internal string FakeName { get; }

        internal string FakeClassName { get; }

        internal List<FakeNode> Kids { get; } = new List<FakeNode>();

        /// <summary>Сбой запроса детей (null - без сбоя); аргумент - номер вызова начиная с 1.</summary>
        internal Func<int, Exception> OnFindChildren { get; set; }

        /// <summary>Сбой чтения свойства (null - без сбоя); аргумент - номер чтения начиная с 1.</summary>
        internal Func<int, Exception> OnProperty { get; set; }

        internal int FindChildrenCalls { get; private set; }

        internal int PropertyReads { get; private set; }

        internal AutomationElement Element => new AutomationElement(this);

        internal FakeNode Add(params FakeNode[] children)
        {
            Kids.AddRange(children);
            return this;
        }

        protected override object InternalGetPropertyValue(int propertyId, bool cached, bool useDefaultIfNotSupported)
        {
            PropertyReads++;
            Exception fault = OnProperty?.Invoke(PropertyReads);
            if (fault != null)
            {
                throw fault;
            }
            var ids = Automation.PropertyLibrary.Element;
            if (propertyId == ids.AutomationId.Id)
            {
                return FakeId;
            }
            if (propertyId == ids.Name.Id)
            {
                return FakeName;
            }
            if (propertyId == ids.ClassName.Id)
            {
                return FakeClassName;
            }
            if (propertyId == ids.ControlType.Id)
            {
                return ControlTypeConverter.ToControlTypeNative(FakeType);
            }
            return Automation.NotSupportedValue;
        }

        public override AutomationElement[] FindAll(TreeScope treeScope, ConditionBase condition)
        {
            if (treeScope != TreeScope.Children || !(condition is TrueCondition))
            {
                throw new NotSupportedException("FakeNode: поддерживается только FindAllChildren");
            }
            FindChildrenCalls++;
            Exception fault = OnFindChildren?.Invoke(FindChildrenCalls);
            if (fault != null)
            {
                throw fault;
            }
            return Kids.ConvertAll(k => k.Element).ToArray();
        }

        public override void SetFocus()
        {
            throw new NotSupportedException();
        }

        protected override object InternalGetPattern(int patternId, bool cached)
        {
            throw new NotSupportedException();
        }

        public override AutomationElement FindFirst(TreeScope treeScope, ConditionBase condition)
        {
            throw new NotSupportedException();
        }

        public override AutomationElement[] FindAllWithOptions(TreeScope treeScope, ConditionBase condition, TreeTraversalOptions traversalOptions, AutomationElement root)
        {
            throw new NotSupportedException();
        }

        public override AutomationElement FindFirstWithOptions(TreeScope treeScope, ConditionBase condition, TreeTraversalOptions traversalOptions, AutomationElement root)
        {
            throw new NotSupportedException();
        }

        public override AutomationElement FindAt(TreeScope treeScope, int index, ConditionBase condition)
        {
            throw new NotSupportedException();
        }

        public override bool TryGetClickablePoint(out Point point)
        {
            throw new NotSupportedException();
        }

        public override ActiveTextPositionChangedEventHandlerBase RegisterActiveTextPositionChangedEvent(TreeScope treeScope, Action<AutomationElement, ITextRange> action)
        {
            throw new NotSupportedException();
        }

        public override AutomationEventHandlerBase RegisterAutomationEvent(EventId @event, TreeScope treeScope, Action<AutomationElement, EventId> action)
        {
            throw new NotSupportedException();
        }

        public override PropertyChangedEventHandlerBase RegisterPropertyChangedEvent(TreeScope treeScope, Action<AutomationElement, PropertyId, object> action, PropertyId[] properties)
        {
            throw new NotSupportedException();
        }

        public override StructureChangedEventHandlerBase RegisterStructureChangedEvent(TreeScope treeScope, Action<AutomationElement, StructureChangeType, int[]> action)
        {
            throw new NotSupportedException();
        }

        public override NotificationEventHandlerBase RegisterNotificationEvent(TreeScope treeScope, Action<AutomationElement, NotificationKind, NotificationProcessing, string, string> action)
        {
            throw new NotSupportedException();
        }

        public override TextEditTextChangedEventHandlerBase RegisterTextEditTextChangedEventHandler(TreeScope treeScope, TextEditChangeType textEditChangeType, Action<AutomationElement, TextEditChangeType, string[]> action)
        {
            throw new NotSupportedException();
        }

        public override void UnregisterActiveTextPositionChangedEventHandler(ActiveTextPositionChangedEventHandlerBase eventHandler)
        {
            throw new NotSupportedException();
        }

        public override void UnregisterAutomationEventHandler(AutomationEventHandlerBase eventHandler)
        {
            throw new NotSupportedException();
        }

        public override void UnregisterPropertyChangedEventHandler(PropertyChangedEventHandlerBase eventHandler)
        {
            throw new NotSupportedException();
        }

        public override void UnregisterStructureChangedEventHandler(StructureChangedEventHandlerBase eventHandler)
        {
            throw new NotSupportedException();
        }

        public override void UnregisterNotificationEventHandler(NotificationEventHandlerBase eventHandler)
        {
            throw new NotSupportedException();
        }

        public override void UnregisterTextEditTextChangedEventHandler(TextEditTextChangedEventHandlerBase eventHandler)
        {
            throw new NotSupportedException();
        }

        public override PatternId[] GetSupportedPatterns()
        {
            throw new NotSupportedException();
        }

        public override PropertyId[] GetSupportedProperties()
        {
            throw new NotSupportedException();
        }

        public override AutomationElement GetUpdatedCache()
        {
            throw new NotSupportedException();
        }

        public override AutomationElement[] GetCachedChildren()
        {
            throw new NotSupportedException();
        }

        public override AutomationElement GetCachedParent()
        {
            throw new NotSupportedException();
        }

        public override object GetCurrentMetadataValue(PropertyId targetId, int metadataId)
        {
            throw new NotSupportedException();
        }

        protected override IAutomationPattern<IAnnotationPattern> InitializeAnnotationPattern()
        {
            throw new NotSupportedException();
        }

        protected override IAutomationPattern<IDockPattern> InitializeDockPattern()
        {
            throw new NotSupportedException();
        }

        protected override IAutomationPattern<IDragPattern> InitializeDragPattern()
        {
            throw new NotSupportedException();
        }

        protected override IAutomationPattern<IDropTargetPattern> InitializeDropTargetPattern()
        {
            throw new NotSupportedException();
        }

        protected override IAutomationPattern<IExpandCollapsePattern> InitializeExpandCollapsePattern()
        {
            throw new NotSupportedException();
        }

        protected override IAutomationPattern<IGridItemPattern> InitializeGridItemPattern()
        {
            throw new NotSupportedException();
        }

        protected override IAutomationPattern<IGridPattern> InitializeGridPattern()
        {
            throw new NotSupportedException();
        }

        protected override IAutomationPattern<IInvokePattern> InitializeInvokePattern()
        {
            throw new NotSupportedException();
        }

        protected override IAutomationPattern<IItemContainerPattern> InitializeItemContainerPattern()
        {
            throw new NotSupportedException();
        }

        protected override IAutomationPattern<ILegacyIAccessiblePattern> InitializeLegacyIAccessiblePattern()
        {
            throw new NotSupportedException();
        }

        protected override IAutomationPattern<IMultipleViewPattern> InitializeMultipleViewPattern()
        {
            throw new NotSupportedException();
        }

        protected override IAutomationPattern<IObjectModelPattern> InitializeObjectModelPattern()
        {
            throw new NotSupportedException();
        }

        protected override IAutomationPattern<IRangeValuePattern> InitializeRangeValuePattern()
        {
            throw new NotSupportedException();
        }

        protected override IAutomationPattern<IScrollItemPattern> InitializeScrollItemPattern()
        {
            throw new NotSupportedException();
        }

        protected override IAutomationPattern<IScrollPattern> InitializeScrollPattern()
        {
            throw new NotSupportedException();
        }

        protected override IAutomationPattern<ISelectionItemPattern> InitializeSelectionItemPattern()
        {
            throw new NotSupportedException();
        }

        protected override IAutomationPattern<ISelection2Pattern> InitializeSelection2Pattern()
        {
            throw new NotSupportedException();
        }

        protected override IAutomationPattern<ISelectionPattern> InitializeSelectionPattern()
        {
            throw new NotSupportedException();
        }

        protected override IAutomationPattern<ISpreadsheetItemPattern> InitializeSpreadsheetItemPattern()
        {
            throw new NotSupportedException();
        }

        protected override IAutomationPattern<ISpreadsheetPattern> InitializeSpreadsheetPattern()
        {
            throw new NotSupportedException();
        }

        protected override IAutomationPattern<IStylesPattern> InitializeStylesPattern()
        {
            throw new NotSupportedException();
        }

        protected override IAutomationPattern<ISynchronizedInputPattern> InitializeSynchronizedInputPattern()
        {
            throw new NotSupportedException();
        }

        protected override IAutomationPattern<ITableItemPattern> InitializeTableItemPattern()
        {
            throw new NotSupportedException();
        }

        protected override IAutomationPattern<ITablePattern> InitializeTablePattern()
        {
            throw new NotSupportedException();
        }

        protected override IAutomationPattern<ITextChildPattern> InitializeTextChildPattern()
        {
            throw new NotSupportedException();
        }

        protected override IAutomationPattern<ITextEditPattern> InitializeTextEditPattern()
        {
            throw new NotSupportedException();
        }

        protected override IAutomationPattern<IText2Pattern> InitializeText2Pattern()
        {
            throw new NotSupportedException();
        }

        protected override IAutomationPattern<ITextPattern> InitializeTextPattern()
        {
            throw new NotSupportedException();
        }

        protected override IAutomationPattern<ITogglePattern> InitializeTogglePattern()
        {
            throw new NotSupportedException();
        }

        protected override IAutomationPattern<ITransform2Pattern> InitializeTransform2Pattern()
        {
            throw new NotSupportedException();
        }

        protected override IAutomationPattern<ITransformPattern> InitializeTransformPattern()
        {
            throw new NotSupportedException();
        }

        protected override IAutomationPattern<IValuePattern> InitializeValuePattern()
        {
            throw new NotSupportedException();
        }

        protected override IAutomationPattern<IVirtualizedItemPattern> InitializeVirtualizedItemPattern()
        {
            throw new NotSupportedException();
        }

        protected override IAutomationPattern<IWindowPattern> InitializeWindowPattern()
        {
            throw new NotSupportedException();
        }
    }
}
