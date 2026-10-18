using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using PilotUITesting.Uia;

namespace PilotUITesting.Client
{
    /// <summary>Как поступить с окнами при уборке перед тестом.</summary>
    public sealed class UiResetOptions
    {
        /// <summary>AutomationId кнопок отмены (по умолчанию btnCancel и «2» - «Нет/Отмена» диалогов Pilot).</summary>
        public IReadOnlyList<string> CancelButtonIds { get; set; } = new[] { PilotIds.MessageBox.ClientCancel, PilotIds.MessageBox.Cancel };

        public UnsentChangesPolicy UnsentChanges { get; set; } = UnsentChangesPolicy.Fail;
    }

    /// <summary>Что сделала уборка окон и что осталось открытым.</summary>
    public sealed class UiResetReport
    {
        internal UiResetReport(IReadOnlyList<string> actions, IReadOnlyList<string> remaining)
        {
            Actions = actions;
            Remaining = remaining;
        }

        public IReadOnlyList<string> Actions { get; }

        public IReadOnlyList<string> Remaining { get; }

        public bool Clean => Remaining.Count == 0;
    }

    /// <summary>
    /// Окна процесса клиента: окна верхнего уровня и вложенные в них окна (дочерние окна главного окна - диалоги DialogService и модулей,
    /// окна ошибок; окна, вложенные в карточку, - прогресс, справочник, диалог файла). Всплывающие меню и списки (класс Popup) - отдельно.
    /// </summary>
    public sealed class PilotWindows
    {
        private const string PopupClass = "Popup";
        private readonly PilotApp _app;

        internal PilotWindows(PilotApp app)
        {
            _app = app;
        }

        /// <summary>Все окна процесса, кроме главного и всплывающих (Popup).</summary>
        public IReadOnlyList<PilotDialog> All()
        {
            return Elements(false).Select(w => new PilotDialog(_app, w)).ToList();
        }

        /// <summary>Окно с заголовком: Name == title или AutomationId == title (DialogService пишет заголовок в id, модули - в Name).</summary>
        public PilotDialog Wait(string title, TimeSpan? timeout = null)
        {
            return Wait(TitleSelector(title), timeout);
        }

        public PilotDialog Wait(By selector, TimeSpan? timeout = null)
        {
            return Uia.Wait.ForStrict(() => TryFind(selector), timeout ?? Uia.Wait.DefaultTimeout, "окно " + selector, DescribeAll);
        }

        public PilotDialog TryFind(string title)
        {
            return TryFind(TitleSelector(title));
        }

        /// <summary>Окно по селектору; больше одного - <see cref="AmbiguousElementException"/>.</summary>
        public PilotDialog TryFind(By selector)
        {
            List<AutomationElement> matches = Elements(false).Where(w => Matches(w, selector)).ToList();
            if (matches.Count > 1)
            {
                throw new AmbiguousElementException("окно " + selector, matches.Select(m => m.Describe()).ToList());
            }
            return matches.Count == 0 ? null : new PilotDialog(_app, matches[0]);
        }

        /// <summary>Выполнить действие и вернуть окно, которого до этого не было (сравнение по HWND и RuntimeId): окно из прошлого теста не подойдёт.</summary>
        public PilotDialog OpenedBy(Action open, By selector = null, TimeSpan? timeout = null)
        {
            HashSet<string> before = Snapshot();
            open();
            By match = selector ?? By.Type(ControlType.Window);
            return Uia.Wait.ForStrict(() => TryNew(before, match), timeout ?? Uia.Wait.DefaultTimeout, "новое окно " + match, DescribeAll);
        }

        /// <summary>
        /// Выполнить действие и наблюдать observeFor: новые окна (по HWND и RuntimeId, под selector; null - любые окна). Возвращает их, как
        /// только появятся, иначе пустой список - для проверок «окно не появилось». Неоднозначность не исключение.
        /// </summary>
        public IReadOnlyList<PilotDialog> NewWindowsAfter(Action action, TimeSpan observeFor, By selector = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            HashSet<string> before = Snapshot();
            action();
            By match = selector ?? By.Type(ControlType.Window);
            Stopwatch sw = Stopwatch.StartNew();
            while (true)
            {
                bool expired = sw.Elapsed >= observeFor;
                List<AutomationElement> fresh = Elements(false).Where(w =>
                {
                    string key = Key(w);
                    return key != null && !before.Contains(key) && Matches(w, match);
                }).ToList();
                if (fresh.Count > 0)
                {
                    return fresh.Select(w => new PilotDialog(_app, w)).ToList();
                }
                if (expired)
                {
                    return new PilotDialog[0];
                }
                System.Threading.Thread.Sleep(Uia.Wait.PollInterval);
            }
        }

        /// <summary>Ключи открытых окон (для поиска окна, появившегося после действия).</summary>
        internal HashSet<string> Snapshot()
        {
            return new HashSet<string>(Elements(false).Select(Key).Where(k => k != null));
        }

        /// <summary>Окно по селектору, которого не было в снимке; больше одного - неоднозначность; нет - null.</summary>
        internal PilotDialog TryNew(HashSet<string> before, By match)
        {
            AutomationElement[] fresh = Elements(false).Where(w =>
            {
                string key = Key(w);
                return key != null && !before.Contains(key) && Matches(w, match);
            }).ToArray();
            if (fresh.Length > 1)
            {
                throw new AmbiguousElementException("новое окно " + match, fresh.Select(f => f.Describe()).ToList());
            }
            return fresh.Length == 1 ? new PilotDialog(_app, fresh[0]) : null;
        }

        public void WaitGone(string title, TimeSpan? timeout = null)
        {
            By selector = TitleSelector(title);
            Uia.Wait.Strict(() => Elements(false).All(w => !Matches(w, selector)), timeout ?? Uia.Wait.DefaultTimeout, "закрытие окна «" + title + "»", DescribeAll);
        }

        /// <summary>Сообщение или подтверждение Pilot с заголовком.</summary>
        public PilotMessageBox WaitMessageBox(string title, TimeSpan? timeout = null)
        {
            return new PilotMessageBox(Wait(title, timeout));
        }

        /// <summary>Системный диалог открытия/сохранения файла (#32770).</summary>
        public WindowsFileDialog WaitFileDialog(TimeSpan? timeout = null)
        {
            return new WindowsFileDialog(Wait(By.ClassName(PilotIds.WindowsFileDialog.WindowClass), timeout));
        }

        /// <summary>Открытые окна ошибок Pilot (AUIDPilotError-…).</summary>
        public IReadOnlyList<PilotErrorWindow> Errors()
        {
            return Nodes(false)
                .Where(n => n.Id.StartsWith(PilotIds.ErrorWindow.WindowIdPrefix, StringComparison.Ordinal))
                .Select(n => new PilotErrorWindow(new PilotDialog(_app, n.Element)))
                .ToList();
        }

        /// <summary>Открытые окна «Отправка изменений» (сервер не принял изменения).</summary>
        public IReadOnlyList<PilotUnsentChangesWindow> UnsentChanges()
        {
            return Nodes(false)
                .Where(n => n.Id == PilotIds.UnsentChanges.Container)
                .Select(n => new PilotUnsentChangesWindow(new PilotDialog(_app, n.Element)))
                .ToList();
        }

        /// <summary>
        /// Закрыть то, что осталось от упавшего теста: всплывающие меню (Esc), окна ошибок (текст - в журнал), «Отправка изменений»
        /// (по политике), подтверждение удаления («Не удалять»), прочие окна кнопками отмены или WindowPattern.Close - от вложенных к внешним.
        /// Вкладки и представления модулей не трогаются. До 5 кругов; каждое закрытие проверяется по HWND.
        /// </summary>
        public UiResetReport ResetUi(UiResetOptions options)
        {
            options = options ?? new UiResetOptions();
            List<string> actions = new List<string>();
            for (int round = 0; round < 5; round++)
            {
                bool acted = false;
                foreach (AutomationElement popup in Popups())
                {
                    string what = "всплывающее меню/список " + popup.Describe();
                    Uia.UiAct.KeysToForeground(_app.ProcessId, "Esc: " + what, FlaUI.Core.WindowsAPI.VirtualKeyShort.ESCAPE);
                    actions.Add("Esc: " + what);
                    acted = true;
                    break;
                }
                if (acted)
                {
                    continue;
                }
                foreach (PilotErrorWindow error in Errors())
                {
                    actions.Add($"закрыто окно ошибки «{error.Title}»: {error.Message}");
                    PilotLog.Warn($"Окно ошибки Pilot «{error.Title}»: {error.Message}");
                    error.Close();
                    acted = true;
                }
                foreach (PilotUnsentChangesWindow unsent in UnsentChanges())
                {
                    if (options.UnsentChanges != UnsentChangesPolicy.Discard)
                    {
                        throw new InvalidOperationException(
                            $"Открыто окно «{unsent.Title}»: сервер не принял изменения клиента - {unsent.Message}. " +
                            "Удалить эти изменения может только человек (или \"unsentChanges\": \"Discard\" в pilot-ui-tests.json - стирает ВСЕ неотправленные изменения клиента).");
                    }
                    actions.Add($"стёрты неотправленные изменения: {unsent.Message}");
                    unsent.DiscardChanges();
                    acted = true;
                }
                PilotDialog delete = TryFind(By.Id(PilotTitles.Windows.DeleteConfirmation));
                if (delete != null)
                {
                    actions.Add("подтверждение удаления: «Не удалять»");
                    delete.ClickAndWaitClosed(PilotIds.MessageBox.Cancel);
                    acted = true;
                }
                if (acted)
                {
                    continue;
                }
                // прочие окна: сначала самые вложенные
                PilotDialog other = Nodes(false).Where(n => n.Id != PilotIds.MainWindow.NotificationWindow).Reverse().Select(n => new PilotDialog(_app, n.Element)).FirstOrDefault(d => d.IsOpen);
                if (other == null)
                {
                    break;
                }
                string description = other.Description;
                AutomationElement cancel = options.CancelButtonIds.Select(id => other.TryFind(id)).FirstOrDefault(b => b != null && b.SafeIsEnabled());
                if (cancel != null)
                {
                    Uia.UiAct.Press(cancel, "отмена в " + description);
                }
                else
                {
                    UiQuery.Read(() =>
                    {
                        other.Window.Close();
                        return true;
                    }, Uia.Wait.DefaultTimeout, "закрытие " + description);
                }
                if (!Uia.Wait.TryUntil(() => !other.IsOpen, TimeSpan.FromSeconds(10)))
                {
                    break;
                }
                actions.Add("закрыто " + description + (cancel != null ? " кнопкой " + cancel.SafeAutomationId() : ""));
            }
            List<string> remaining = Nodes(false).Where(n => n.Id != PilotIds.MainWindow.NotificationWindow).Select(n => n.Element.Describe())
                .Concat(Popups().Select(p => "всплывающее " + p.Describe())).ToList();
            foreach (string action in actions)
            {
                PilotLog.Action("уборка окон: " + action);
            }
            return new UiResetReport(actions, remaining);
        }

        /// <summary>Открытые всплывающие окна (класс Popup): контекстные меню, меню кнопок панелей, выпадающие списки редакторов.</summary>
        public IReadOnlyList<AutomationElement> Popups()
        {
            return Nodes(true).Where(n => n.ClassName == PopupClass && IsShown(n.Element)).Select(n => n.Element).ToList();
        }

        /// <summary>Дождаться элемента во всплывающем окне (например, пункта выпадающего списка редактора ячейки).</summary>
        public AutomationElement WaitInPopup(By selector, TimeSpan? timeout = null)
        {
            return Uia.Wait.ForStrict(() => Popups().Select(p => UiQuery.TryFind(p, selector, Uia.Wait.DefaultTimeout, 8)).FirstOrDefault(e => e != null),
                timeout ?? Uia.Wait.DefaultTimeout, selector + " во всплывающем окне", () => Popups().Count + " всплывающих окон");
        }

        /// <summary>Окна процесса: верхнего уровня (кроме главного) и вложенные окна (дочерние окна-«Window» любых окон процесса, рекурсивно).</summary>
        internal IReadOnlyList<AutomationElement> Elements(bool includePopups)
        {
            return Nodes(includePopups).Select(n => n.Element).ToList();
        }

        /// <summary>Окно в перечне и его свойства, прочитанные при обходе.</summary>
        private sealed class WindowNode
        {
            internal WindowNode(AutomationElement element, string id, string className, IntPtr handle)
            {
                Element = element;
                Id = id;
                ClassName = className;
                Handle = handle;
            }

            internal IntPtr Handle { get; }

            internal AutomationElement Element { get; }

            internal string Id { get; }

            internal string ClassName { get; }
        }

        /// <summary>
        /// Перечень окон только по полному обходу: если клиент занят, обход повторяется (не дольше <see cref="Uia.Wait.DefaultTimeout"/>),
        /// затем <see cref="ClientBusyException"/> - неполный перечень дал бы ложное «окон нет» уборке, проверке ошибок и снимку до действия.
        /// Окно, закрывшееся во время обхода, в перечень не входит.
        /// </summary>
        private List<WindowNode> Nodes(bool includePopups)
        {
            Stopwatch sw = Stopwatch.StartNew();
            int busy = 0;
            Exception last = null;
            while (true)
            {
                UiQueryResult faults = new UiQueryResult();
                List<WindowNode> result = Pass(_app.TopLevelWindows(), includePopups, faults);
                if (faults.Complete)
                {
                    return result;
                }
                busy += faults.BusyFaults;
                last = faults.LastFault ?? last;
                if (sw.Elapsed >= Uia.Wait.DefaultTimeout)
                {
                    throw new ClientBusyException("перечень окон клиента", Uia.Wait.DefaultTimeout, sw.Elapsed, busy, last);
                }
                System.Threading.Thread.Sleep(Uia.Wait.PollInterval);
            }
        }

        private static List<WindowNode> Pass(AutomationElement[] topLevel, bool includePopups, UiQueryResult faults)
        {
            List<WindowNode> result = new List<WindowNode>();
            foreach (AutomationElement top in topLevel)
            {
                if (!Visit(top, true, result, includePopups, faults, 0))
                {
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Окна ошибок Pilot одним проходом без повторов - для диагностики в сообщениях о таймаутах: она не должна ждать занятый клиент
        /// (иначе каждое сообщение о таймауте задерживалось бы на время занятости). Клиент занят - пометка вместо перечня.
        /// </summary>
        internal IReadOnlyList<string> DescribeErrorsOnce()
        {
            UiQueryResult faults = new UiQueryResult();
            List<WindowNode> nodes;
            try
            {
                nodes = Pass(_app.TopLevelWindowsOnce(), false, faults);
            }
            catch (Exception ex) when (UiFault.IsTransient(ex))
            {
                return new[] { "Окна ошибок Pilot не проверены: " + ex.GetType().Name + ": " + ex.Message };
            }
            if (!faults.Complete)
            {
                return new[] { "Окна ошибок Pilot не проверены: клиент занят (" + faults.LastFault?.Message + ")" };
            }
            return nodes.Where(n => n.Id.StartsWith(PilotIds.ErrorWindow.WindowIdPrefix, StringComparison.Ordinal)).Select(DescribeErrorOnce).ToList();
        }

        private static string DescribeErrorOnce(WindowNode node)
        {
            string head = $"Открыто окно ошибки Pilot ({node.Id})";
            try
            {
                UiQueryResult found = UiQuery.Find(node.Element, By.Id(PilotIds.ErrorWindow.Message), UiQuery.DefaultMaxDepth, 1);
                if (found.Matches.Count == 0)
                {
                    return head + (found.Complete ? ": текста ошибки нет" : ": текст не прочитан (клиент занят)");
                }
                AutomationElement message = found.Matches[0];
                string text = message.Patterns.Value.PatternOrDefault?.Value.ValueOrDefault ?? "";
                return head + ": " + text;
            }
            catch (Exception ex) when (UiFault.IsTransient(ex))
            {
                return head + ": текст не прочитан (" + ex.GetType().Name + ": " + ex.Message + ")";
            }
        }

        /// <summary>Добавить окно (если это окно) и обойти его дочерние окна; false - клиент занят, обход прерван.</summary>
        private static bool Visit(AutomationElement element, bool topLevel, List<WindowNode> result, bool includePopups, UiQueryResult faults, int depth)
        {
            ControlType type;
            string id = "";
            string className = "";
            IntPtr handle = IntPtr.Zero;
            try
            {
                ElementFacts facts = new ElementFacts(element);
                type = facts.ControlType;
                if (type == ControlType.Window)
                {
                    id = facts.AutomationId;
                    className = facts.ClassName;
                    handle = element.Properties.NativeWindowHandle.ValueOrDefault;
                }
            }
            catch (Exception ex) when (UiFault.IsTransient(ex))
            {
                if (UiFault.Classify(ex) == UiFaultKind.Busy)
                {
                    faults.Complete = false;
                    faults.BusyFaults++;
                    faults.LastFault = ex;
                    return false;
                }
                // окно закрылось во время обхода - в перечень не входит
                return true;
            }
            if (!topLevel && type != ControlType.Window)
            {
                return true;
            }
            bool main = topLevel && id == PilotApp.MainWindowId;
            // Окно с владельцем (DialogWindow модуля) UI Automation отдаёт дважды: как окно верхнего уровня
            // и как потомка главного окна. Второй раз в перечень не попадает - иначе поиск по заголовку неоднозначен.
            bool seen = handle != IntPtr.Zero && result.Any(n => n.Handle == handle);
            if (type == ControlType.Window && !main && !seen && (includePopups || className != PopupClass))
            {
                result.Add(new WindowNode(element, id, className, handle));
            }
            if (depth >= 4)
            {
                return true;
            }
            foreach (AutomationElement child in UiQuery.Children(element, faults))
            {
                if (!Visit(child, false, result, includePopups, faults, depth + 1))
                {
                    return false;
                }
            }
            return faults.BusyFaults == 0;
        }

        /// <summary>Окно подходит под селектор; клиент занят - повтор чтения; окно закрылось - не подходит.</summary>
        private static bool Matches(AutomationElement window, By selector)
        {
            try
            {
                return UiQuery.Read(() => selector.Matches(new ElementFacts(window)), Uia.Wait.DefaultTimeout, "окно " + selector);
            }
            catch (Exception ex) when (UiFault.Classify(ex) == UiFaultKind.Gone)
            {
                return false;
            }
        }

        private static bool IsShown(AutomationElement window)
        {
            IntPtr handle;
            try
            {
                handle = UiQuery.Read(() => window.Properties.NativeWindowHandle.ValueOrDefault, Uia.Wait.DefaultTimeout, "HWND всплывающего окна");
            }
            catch (Exception ex) when (UiFault.Classify(ex) == UiFaultKind.Gone)
            {
                // окно закрылось
                return false;
            }
            return Native.IsShown(handle);
        }

        /// <summary>Ключ окна (HWND + RuntimeId); null - окно исчезло. «Клиент занят» - чтение повторяется (UiQuery.Read).</summary>
        private static string Key(AutomationElement window)
        {
            try
            {
                return UiQuery.Read(() =>
                {
                    IntPtr handle = window.Properties.NativeWindowHandle.ValueOrDefault;
                    int[] runtime = window.Properties.RuntimeId.ValueOrDefault;
                    return handle.ToInt64() + "|" + (runtime == null ? "" : string.Join(".", runtime));
                }, Uia.Wait.DefaultTimeout, "ключ окна");
            }
            catch (Exception ex) when (UiFault.Classify(ex) == UiFaultKind.Gone)
            {
                return null;
            }
        }

        private static By TitleSelector(string title)
        {
            return By.Where(f => f.Name == title || f.AutomationId == title, "окно с заголовком «" + title + "»");
        }

        private string DescribeAll()
        {
            IReadOnlyList<AutomationElement> windows = Elements(false);
            return windows.Count == 0 ? "открытых окон, кроме главного, нет" : "открыты окна: " + string.Join("; ", windows.Select(w => w.Describe()));
        }
    }
}
