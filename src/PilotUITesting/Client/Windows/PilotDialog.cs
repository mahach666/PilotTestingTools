using System;
using FlaUI.Core.AutomationElements;
using PilotUITesting.Uia;

namespace PilotUITesting.Client
{
    /// <summary>
    /// Окно клиента или модуля - верхнего уровня (карточка объекта, окна задания и процесса) или вложенное (дочерние окна главного:
    /// диалоги DialogService, диалоги модулей, окна ошибок). Закрытость проверяется по HWND окна.
    /// </summary>
    public class PilotDialog : PilotPage
    {
        private readonly Window _window;
        private readonly IntPtr _handle;

        public PilotDialog(PilotApp app, AutomationElement window)
            : base(app, "окно «" + Describe(window) + "»")
        {
            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }
            _window = window.AsWindow();
            _handle = UiQuery.Read(() => window.Properties.NativeWindowHandle.ValueOrDefault, Wait.DefaultTimeout, "HWND окна");
            Title = window.SafeName();
            Id = window.SafeAutomationId();
            ClassName = window.SafeClassName();
        }

        /// <summary>Для наследников, оборачивающих уже найденное окно.</summary>
        protected PilotDialog(PilotDialog dialog)
            : this(dialog.App, dialog._window)
        {
        }

        public Window Window => _window;

        /// <summary>Name окна (у диалогов модулей и DialogService - заголовок; у окон задания/процесса - AUIDNextTasksEdit/AUIDWorkflowEditView).</summary>
        public string Title { get; }

        /// <summary>AutomationId окна (у окон DialogService - заголовок; у диалогов модулей пуст).</summary>
        public string Id { get; }

        public string ClassName { get; }

        /// <summary>HWND окна (0 - у элемента нет своего окна).</summary>
        public IntPtr Handle => _handle;

        /// <summary>Окно существует и видимо (по HWND; у элемента без HWND - по доступности элемента).</summary>
        public bool IsOpen => _handle != IntPtr.Zero ? Native.IsShown(_handle) : _window.SafeIsAvailable();

        protected override AutomationElement ResolveRoot()
        {
            return IsOpen ? _window : null;
        }

        public void WaitClosed(TimeSpan? timeout = null)
        {
            Wait.Strict(() => !IsOpen, timeout ?? Wait.DefaultTimeout, "закрытие " + Description, null);
        }

        /// <summary>Закрыть окно (WindowPattern.Close) и дождаться закрытия.</summary>
        public void Close()
        {
            PilotLog.Action("закрыть " + Description);
            UiQuery.Read(() =>
            {
                _window.Close();
                return true;
            }, Wait.DefaultTimeout, "закрытие " + Description);
            WaitClosed();
        }

        /// <summary>Нажать кнопку по AutomationId и дождаться закрытия окна.</summary>
        public void ClickAndWaitClosed(string automationId)
        {
            Click(automationId);
            WaitClosed();
        }

        /// <summary>
        /// requireEnabled: false - кнопка нажимается кликом мышью без ожидания доступности: в окнах, состояние которых дерево
        /// UI Automation перестаёт обновлять, кнопка в дереве остаётся «недоступна», хотя на экране доступна. Закрытие окна
        /// проверяется по HWND и от дерева не зависит.
        /// </summary>
        public void ClickAndWaitClosed(string automationId, bool requireEnabled)
        {
            if (requireEnabled)
            {
                ClickAndWaitClosed(automationId);
                return;
            }
            UiAct.Click(Find(automationId), $"{automationId} в «{Description}» (без проверки доступности)", FlaUI.Core.Input.MouseButton.Left, false);
            WaitClosed();
        }

        /// <summary>Нажать кнопку по подписи (кнопки окон модулей без AutomationId) и дождаться закрытия окна.</summary>
        public void ClickTextAndWaitClosed(string caption)
        {
            ClickText(caption);
            WaitClosed();
        }

        private static string Describe(AutomationElement window)
        {
            string name = window.SafeName();
            return name.Length > 0 ? name : window.SafeAutomationId();
        }
    }
}
