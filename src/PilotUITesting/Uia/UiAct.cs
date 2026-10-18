using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Exceptions;
using FlaUI.Core.Input;
using FlaUI.Core.Patterns;
using FlaUI.Core.WindowsAPI;

namespace PilotUITesting.Uia
{
    /// <summary>
    /// Действия над элементами по правилу «одно действие - один вызов»: нажатие не повторяется при сбое
    /// (повтор мог бы нажать кнопку дважды - второй раз создать объект); результат проверяет вызывающий по постусловию.
    /// Invoke выполняется в фоне: синхронный обработчик пункта меню в Pilot держит вызов до нескольких секунд.
    /// Мышь и клавиатура - только если в точке клика / в фокусе окно процесса клиента, иначе ввод ушёл бы в чужое окно.
    /// </summary>
    internal static class UiAct
    {
        /// <summary>Сколько ждать возврата Invoke, прежде чем продолжить (обработчик выполняется дальше в фоне).</summary>
        internal static TimeSpan InvokeGrace { get; set; } = TimeSpan.FromSeconds(3);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(Point point);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(uint attach, uint attachTo, bool doAttach);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint flags, int dx, int dy, uint data, UIntPtr extraInfo);

        private const uint MouseEventMove = 0x0001;

        /// <summary>
        /// Нажать: Invoke; нет Invoke - Toggle; нет и его - клик мышью (требует DPI-осведомлённости).
        /// Элемент недоступен - исключение до действия.
        /// </summary>
        internal static void Press(AutomationElement element, string description)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }
            TimeSpan read = Wait.DefaultTimeout;
            if (!UiQuery.Read(() => element.Properties.IsEnabled.ValueOrDefault, read, "доступность " + description))
            {
                throw new InvalidOperationException($"Нельзя нажать {description}: элемент недоступен (IsEnabled = false).");
            }
            IInvokePattern invoke = UiQuery.Read(() => element.Patterns.Invoke.PatternOrDefault, read, "InvokePattern " + description);
            if (invoke != null)
            {
                RunOnce(element, description, invoke.Invoke, "Invoke");
                return;
            }
            ITogglePattern toggle = UiQuery.Read(() => element.Patterns.Toggle.PatternOrDefault, read, "TogglePattern " + description);
            if (toggle != null)
            {
                RunOnce(element, description, toggle.Toggle, "Toggle");
                return;
            }
            Click(element, description, MouseButton.Left, false);
        }

        /// <summary>Выполнить вызов паттерна один раз в фоне; сбой «занят» - дождаться отклика клиента, не повторяя.</summary>
        internal static void RunOnce(AutomationElement element, string description, Action call, string how)
        {
            PilotLog.Action(how + " " + description);
            Task task = Task.Run(call);
            bool finished;
            try
            {
                finished = task.Wait(InvokeGrace);
            }
            catch (AggregateException aggregate)
            {
                Exception ex = aggregate.InnerException ?? aggregate;
                switch (UiFault.Classify(ex))
                {
                    case UiFaultKind.Busy:
                        PilotLog.Warn($"Клиент был занят во время {how} {description}; повтора нет - результат проверяется ожиданием постусловия.");
                        WaitResponsive(element);
                        return;
                    case UiFaultKind.Gone:
                        throw new InvalidOperationException($"{description}: элемент исчез из дерева UI Automation до нажатия.", ex);
                    default:
                        if (ex is ElementNotEnabledException)
                        {
                            throw new InvalidOperationException($"{description}: элемент недоступен для нажатия.", ex);
                        }
                        throw new InvalidOperationException($"{how} {description} завершился ошибкой: {ex.Message}", ex);
                }
            }
            if (!finished)
            {
                PilotLog.Action($"{how} {description}: обработчик ещё выполняется, продолжаем без ожидания его возврата");
                task.ContinueWith(t => PilotLog.Warn($"{how} {description} завершился ошибкой в фоне: {t.Exception?.InnerException}"), TaskContinuationOptions.OnlyOnFaulted);
            }
        }

        /// <summary>Клик мышью в центр элемента (или его кликабельную точку) - для элементов, где Invoke не срабатывает.</summary>
        internal static void Click(AutomationElement element, string description, MouseButton button, bool doubleClick)
        {
            string action = (doubleClick ? "двойной " : "") + (button == MouseButton.Right ? "правый клик" : "клик") + " по " + description;
            ProcessDpi.RequireForMouse(action);
            Point point = ClickPoint(element, description);
            int pid = UiQuery.Read(() => element.Properties.ProcessId.ValueOrDefault, Wait.DefaultTimeout, "процесс " + description);
            EnsurePointBelongsTo(point, pid, element, action);
            PilotLog.Action(action + $" ({point.X},{point.Y})");
            if (doubleClick)
            {
                Mouse.DoubleClick(point, button);
            }
            else
            {
                Mouse.Click(point, button);
            }
        }

        /// <summary>Нажать клавиши (одновременно) с фокусом на элементе; фокус должен оказаться в окне процесса клиента.</summary>
        internal static void Keys(AutomationElement focusTarget, string description, params VirtualKeyShort[] keys)
        {
            int pid = UiQuery.Read(() => focusTarget.Properties.ProcessId.ValueOrDefault, Wait.DefaultTimeout, "процесс " + description);
            UiQuery.Read(() =>
            {
                focusTarget.Focus();
                return true;
            }, Wait.DefaultTimeout, "фокус на " + description);
            EnsureForeground(pid, focusTarget, description);
            PilotLog.Action("клавиши " + string.Join("+", keys) + " -> " + description);
            if (keys.Length == 1)
            {
                Keyboard.Type(keys[0]);
            }
            else
            {
                Keyboard.TypeSimultaneously(keys);
            }
        }

        /// <summary>Набрать текст с клавиатуры в элемент (для полей автодополнения, где нужен настоящий ввод); фокус - в окне клиента.</summary>
        internal static void TypeText(AutomationElement focusTarget, string text, string description)
        {
            int pid = UiQuery.Read(() => focusTarget.Properties.ProcessId.ValueOrDefault, Wait.DefaultTimeout, "процесс " + description);
            UiQuery.Read(() =>
            {
                focusTarget.Focus();
                return true;
            }, Wait.DefaultTimeout, "фокус на " + description);
            EnsureForeground(pid, focusTarget, description);
            PilotLog.Action($"ввод «{text}» -> {description}");
            Keyboard.Type(text ?? "");
        }

        /// <summary>Набрать текст, не меняя фокус (открытый редактор ячейки: перенос фокуса сбил бы правку); активно должно быть окно клиента.</summary>
        internal static void TypeToForeground(int processId, string text, string description)
        {
            EnsureForeground(processId, null, description);
            PilotLog.Action($"ввод «{text}» -> {description}");
            Keyboard.Type(text ?? "");
        }

        /// <summary>Нажать клавиши, не меняя фокус (он уже там, где нужно: например, в выделенной строке после перехода piloturi).</summary>
        internal static void KeysToForeground(int processId, string description, params VirtualKeyShort[] keys)
        {
            EnsureForeground(processId, null, description);
            PilotLog.Action("клавиши " + string.Join("+", keys) + " -> " + description);
            if (keys.Length == 1)
            {
                Keyboard.Type(keys[0]);
            }
            else
            {
                Keyboard.TypeSimultaneously(keys);
            }
        }

        internal static void WaitResponsive(AutomationElement element)
        {
            try
            {
                FlaUI.Core.Input.Wait.UntilResponsive(element, Wait.UiBusyTimeout);
            }
            catch (Exception ex) when (UiFault.Classify(ex) == UiFaultKind.Gone)
            {
                // окно уже закрылось - ждать нечего
            }
        }

        private static Point ClickPoint(AutomationElement element, string description)
        {
            Rectangle rect = UiQuery.Read(() => element.BoundingRectangle, Wait.DefaultTimeout, "прямоугольник " + description);
            if (rect.Width <= 0 || rect.Height <= 0)
            {
                throw new InvalidOperationException($"Нельзя кликнуть по {description}: у элемента пустой прямоугольник ({rect}) - он скрыт или вне экрана.");
            }
            return new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
        }

        private static void EnsurePointBelongsTo(Point point, int processId, AutomationElement element, string action)
        {
            if (OwnerPid(WindowFromPoint(point)) == processId)
            {
                return;
            }
            BringToFront(element);
            uint owner = OwnerPid(WindowFromPoint(point));
            if (owner != processId)
            {
                throw new InvalidOperationException($"Отмена: {action} - в точке ({point.X},{point.Y}) окно другого процесса (pid {owner}), а не клиента Pilot (pid {processId}). Окно клиента перекрыто.");
            }
        }

        private static void EnsureForeground(int processId, AutomationElement element, string description)
        {
            if (OwnerPid(GetForegroundWindow()) == processId)
            {
                return;
            }
            if (element != null)
            {
                BringToFront(element);
            }
            uint owner = OwnerPid(GetForegroundWindow());
            if (owner != processId)
            {
                throw new InvalidOperationException($"Отмена ввода с клавиатуры ({description}): активно окно другого процесса (pid {owner}), а не клиента Pilot (pid {processId}). Клавиши не отправлены.");
            }
        }

        internal static void BringToFront(AutomationElement element)
        {
            AutomationElement window = element;
            try
            {
                while (window.Parent != null && window.Parent.Parent != null)
                {
                    window = window.Parent;
                }
                window.SetForeground();
                IntPtr handle = window.Properties.NativeWindowHandle.ValueOrDefault;
                if (handle != IntPtr.Zero && GetForegroundWindow() != handle)
                {
                    // Windows не отдаёт передний план фоновому процессу (запрет переключения фокуса): ввод потока
                    // теста на время присоединяется к потоку текущего переднего окна, тогда SetForegroundWindow разрешён.
                    uint foregroundThread = GetWindowThreadProcessId(GetForegroundWindow(), out _);
                    uint ownThread = GetCurrentThreadId();
                    bool attached = foregroundThread != 0 && foregroundThread != ownThread && AttachThreadInput(ownThread, foregroundThread, true);
                    try
                    {
                        // Нулевое движение мыши: процесс теста становится источником последнего ввода, и Windows
                        // разрешает ему сменить передний план. Курсор не сдвигается, ввод в чужое окно не уходит.
                        mouse_event(MouseEventMove, 0, 0, 0, UIntPtr.Zero);
                        BringWindowToTop(handle);
                        SetForegroundWindow(handle);
                    }
                    finally
                    {
                        if (attached)
                        {
                            AttachThreadInput(ownThread, foregroundThread, false);
                        }
                    }

                    PilotLog.Action("окно клиента выведено на передний план" + (GetForegroundWindow() == handle ? "" : " - не удалось, передний план у другого окна"));
                }
            }
            catch (Exception ex) when (UiFault.IsTransient(ex))
            {
                PilotLog.Warn("Не удалось вывести окно клиента на передний план: " + ex.Message);
            }
        }

        private static uint OwnerPid(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero)
            {
                return 0;
            }
            GetWindowThreadProcessId(hwnd, out uint pid);
            return pid;
        }
    }
}
