using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;

namespace PilotUITesting.Inspector
{
    /// <summary>
    /// Инструмент автора тестов: дамп дерева UI Automation окон процесса, скриншоты, клики по элементам.
    ///   PilotUITesting.Inspector dump  [processName=Ascon.Pilot.PilotBIM] [outDir] [maxDepth=12]
    ///   PilotUITesting.Inspector shot  [processName] [outDir]
    ///   PilotUITesting.Inspector start "exePath" [outDir]                 - запустить процесс, через 15 с сделать дамп
    ///   PilotUITesting.Inspector click processName "id=AutomationId|name=Имя|text=Часть имени" [outDir] [waitMs=3000]
    ///   PilotUITesting.Inspector cancel processName [outDir] [id1,id2,...] - закрыть все модальные окна кнопками отмены
    ///   PilotUITesting.Inspector row   processName "текст строки" [outDir]   - выделить строку списка/дерева
    ///   PilotUITesting.Inspector type  processName "id=AutomationId" "текст" [outDir]
    ///   PilotUITesting.Inspector keys  processName "{ENTER}" [outDir]      - нажать клавиши в активном окне
    /// Процесс инспектора объявляет себя осведомлённым о DPI (Per-Monitor V2) до обращения к UI Automation.
    /// </summary>
    internal static class Program
    {
        private static int Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            // до первого обращения к UI Automation: иначе на экране больше 100 % координаты UIA и мыши расходятся (клики мимо)
            Console.WriteLine(PilotUITesting.Client.PilotApp.EnableDpiAwareness());
            string cmd = args.Length > 0 ? args[0] : "dump";
            try
            {
                switch (cmd)
                {
                    case "start":
                    {
                        string exe = args[1];
                        string outDir = args.Length > 2 ? args[2] : DefaultOut();
                        Process.Start(exe);
                        Thread.Sleep(15000);
                        return Dump(Path.GetFileNameWithoutExtension(exe), outDir, 12, true);
                    }
                    case "shot":
                        return Dump(Arg(args, 1, "Ascon.Pilot.PilotBIM"), Arg(args, 2, DefaultOut()), 0, true);
                    case "click":
                    {
                        string proc = args[1];
                        string selector = args[2];
                        string outDir = Arg(args, 3, DefaultOut());
                        int waitMs = args.Length > 4 ? int.Parse(args[4]) : 3000;
                        Click(proc, selector);
                        Thread.Sleep(waitMs);
                        return Dump(proc, outDir, 12, true);
                    }
                    case "row":
                    {
                        string proc = args[1];
                        string text = args[2];
                        string outDir = Arg(args, 3, DefaultOut());
                        SelectRow(proc, text);
                        Thread.Sleep(1500);
                        return Dump(proc, outDir, 12, true);
                    }
                    case "cancel":
                    {
                        string proc = args[1];
                        string outDir = Arg(args, 2, DefaultOut());
                        string[] ids = args.Length > 3 ? args[3].Split(',') : new[] { "ProjectCancelButton", "FolderCancelButton", "StageCancelButton", "ConfirmCancelButton", "btnCancel" };
                        PilotUiConfig config = new PilotUiConfig { Client = proc.IndexOf("BIM", StringComparison.OrdinalIgnoreCase) >= 0 ? PilotClientKind.PilotBim : PilotClientKind.PilotIce, AttachToRunning = true, KeepClientOpen = true };
                        using (PilotUITesting.Client.PilotApp app = PilotUITesting.Client.PilotApp.StartOrAttach(config))
                        {
                            Console.WriteLine("закрыто окон: " + app.CloseDialogs(ids));
                        }
                        return Dump(proc, outDir, 12, true);
                    }
                    case "type":
                    {
                        string proc = args[1];
                        string selector = args[2];
                        string text = args[3];
                        string outDir = Arg(args, 4, DefaultOut());
                        TypeText(proc, selector, text);
                        Thread.Sleep(1500);
                        return Dump(proc, outDir, 12, true);
                    }
                    case "keys":
                    {
                        string proc = args[1];
                        string keys = args[2];
                        string outDir = Arg(args, 3, DefaultOut());
                        System.Windows.Forms.SendKeys.SendWait(keys);
                        Thread.Sleep(2000);
                        return Dump(proc, outDir, 12, true);
                    }
                    default:
                        return Dump(Arg(args, 1, "Ascon.Pilot.PilotBIM"), Arg(args, 2, DefaultOut()), args.Length > 3 ? int.Parse(args[3]) : 12, true);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return 1;
            }
        }

        private static string Arg(string[] args, int index, string fallback)
        {
            return args.Length > index ? args[index] : fallback;
        }

        private static string DefaultOut()
        {
            return Path.Combine(Path.GetTempPath(), "PilotUITesting");
        }

        private static AutomationElement[] ProcessWindows(UIA3Automation automation, string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length == 0)
            {
                throw new InvalidOperationException("Процесс не найден: " + processName);
            }
            AutomationElement desktop = automation.GetDesktop();
            return processes
                .SelectMany(p => desktop.FindAllChildren(cf => cf.ByProcessId(p.Id)))
                .ToArray();
        }

        private static AutomationElement Find(UIA3Automation automation, string processName, string selector)
        {
            string kind = selector.Substring(0, selector.IndexOf('=')).Trim();
            string value = selector.Substring(selector.IndexOf('=') + 1);
            foreach (AutomationElement window in Enumerable.Reverse(ProcessWindows(automation, processName)))
            {
                AutomationElement found;
                switch (kind)
                {
                    case "id":
                        found = PilotUITesting.Uia.AutomationElementExtensions.TryById(window, value);
                        break;
                    case "name":
                        found = PilotUITesting.Uia.AutomationElementExtensions.TryByName(window, value);
                        break;
                    case "text":
                        found = PilotUITesting.Uia.AutomationElementExtensions.FindDescendantBfs(window, e => Safe(() => e.Name).IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0);
                        break;
                    default:
                        throw new ArgumentException("Селектор: id=..., name=... или text=...");
                }
                if (found != null)
                {
                    return found;
                }
            }
            throw new InvalidOperationException("Элемент не найден: " + selector);
        }

        private static void Click(string processName, string selector)
        {
            using (UIA3Automation automation = new UIA3Automation())
            {
                AutomationElement element = Find(automation, processName, selector);
                Console.WriteLine($"клик: [{element.ControlType}] name=\"{Safe(() => element.Name)}\" id=\"{Safe(() => element.AutomationId)}\" {element.BoundingRectangle}");
                if (element.Patterns.Invoke.IsSupported)
                {
                    element.Patterns.Invoke.Pattern.Invoke();
                }
                else if (element.Patterns.SelectionItem.IsSupported)
                {
                    element.Patterns.SelectionItem.Pattern.Select();
                }
                else
                {
                    element.Click();
                }
            }
        }

        private static void SelectRow(string processName, string text)
        {
            using (UIA3Automation automation = new UIA3Automation())
            {
                foreach (AutomationElement window in Enumerable.Reverse(ProcessWindows(automation, processName)))
                {
                    AutomationElement row = PilotUITesting.Uia.AutomationElementExtensions.TryRowContaining(window, text);
                    if (row != null)
                    {
                        Console.WriteLine($"строка: [{row.ControlType}] name=\"{Safe(() => row.Name)}\" {row.BoundingRectangle}");
                        PilotUITesting.Uia.AutomationElementExtensions.SelectItem(row);
                        return;
                    }
                }
                throw new InvalidOperationException("Строка не найдена: " + text);
            }
        }

        private static void TypeText(string processName, string selector, string text)
        {
            using (UIA3Automation automation = new UIA3Automation())
            {
                AutomationElement element = Find(automation, processName, selector);
                element.Focus();
                if (element.Patterns.Value.IsSupported)
                {
                    element.Patterns.Value.Pattern.SetValue(text);
                }
                else
                {
                    FlaUI.Core.Input.Keyboard.Type(text);
                }
            }
        }

        private static int Dump(string processName, string outDir, int maxDepth, bool screenshot)
        {
            Directory.CreateDirectory(outDir);
            using (UIA3Automation automation = new UIA3Automation())
            {
                AutomationElement[] windows = ProcessWindows(automation, processName);
                Console.WriteLine($"Процесс {processName}, окон верхнего уровня: {windows.Length}");
                int index = 0;
                foreach (AutomationElement window in windows)
                {
                    index++;
                    string stamp = DateTime.Now.ToString("HHmmss");
                    string baseName = $"{stamp}_{index}_{Sanitize(Safe(() => window.Name))}";
                    if (maxDepth > 0)
                    {
                        // подробный дамп библиотеки: HelpText, ItemStatus, ItemType, паттерны; SelectionPattern.Selection не читается
                        // (у служебного SilentInvoker в кнопках панелей Pilot его чтение выполняет команду кнопки)
                        StringBuilder sb = new StringBuilder(PilotUITesting.Uia.AutomationElementExtensions.DumpTree(window, maxDepth, true));
                        string treePath = Path.Combine(outDir, baseName + ".txt");
                        File.WriteAllText(treePath, sb.ToString(), Encoding.UTF8);
                        Console.WriteLine("дерево: " + treePath);
                    }
                    if (screenshot)
                    {
                        try
                        {
                            string shotPath = Path.Combine(outDir, baseName + ".png");
                            window.CaptureToFile(shotPath);
                            Console.WriteLine("скриншот: " + shotPath);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("скриншот не снят: " + ex.Message);
                        }
                    }
                }
            }
            return 0;
        }

        private static string Safe(Func<string> f)
        {
            try
            {
                return f() ?? "";
            }
            catch
            {
                return "";
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

        private static string Sanitize(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return "window";
            }
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                s = s.Replace(c, '_');
            }
            return s.Length > 40 ? s.Substring(0, 40) : s;
        }
    }
}
