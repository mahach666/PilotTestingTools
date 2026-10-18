using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Capturing;
using NUnit.Framework;
using PilotUITesting.Client;
using PilotUITesting.Uia;

namespace PilotUITesting.NUnit
{
    /// <summary>
    /// Папка артефактов теста «&lt;ArtifactsDir&gt;\&lt;yyyyMMdd_HHmmss_fff&gt;_&lt;Класс&gt;.&lt;Тест&gt;» с одной меткой времени на все файлы;
    /// файлы прикладываются к результату NUnit. Хранятся последние Config.ArtifactsKeep папок.
    /// </summary>
    public sealed class TestArtifacts
    {
        /// <summary>Имя папки артефактов теста (см. <see cref="For"/>): уборка трогает только такие папки.</summary>
        private static readonly System.Text.RegularExpressions.Regex FolderName =
            new System.Text.RegularExpressions.Regex(@"^\d{8}_\d{6}_\d{3}_", System.Text.RegularExpressions.RegexOptions.CultureInvariant);

        private TestArtifacts(string folder)
        {
            Folder = folder;
        }

        public string Folder { get; }

        /// <summary>Папка для текущего теста (создаётся при первом обращении к файлам).</summary>
        public static TestArtifacts For(PilotUiConfig config, string className, string testName, DateTime stamp)
        {
            string name = stamp.ToString("yyyyMMdd_HHmmss_fff") + "_" + Sanitize(className + "." + testName, 100);
            return new TestArtifacts(Path.Combine(config.ResolvedArtifactsDir, name));
        }

        /// <summary>Путь к файлу в папке теста (папка создаётся).</summary>
        public string PathOf(string fileName)
        {
            Directory.CreateDirectory(Folder);
            return Path.Combine(Folder, fileName);
        }

        public string WriteText(string fileName, string text, string description)
        {
            string path = PathOf(fileName);
            File.WriteAllText(path, text ?? "", new UTF8Encoding(true));
            TestContext.AddTestAttachment(path, description);
            return path;
        }

        /// <summary>Снимок всего экрана.</summary>
        public string Screen(string fileName = "screen.png")
        {
            string path = PathOf(fileName);
            Capture.MainScreen().ToFile(path);
            TestContext.AddTestAttachment(path, "Экран при падении");
            return path;
        }

        /// <summary>Подробный дамп всех окон процесса клиента (без чтения SelectionPattern.Selection).</summary>
        public string Windows(PilotApp app, string fileName = "windows.txt")
        {
            StringBuilder sb = new StringBuilder();
            foreach (AutomationElement window in app.TopLevelWindows())
            {
                sb.AppendLine(window.DumpTree(40, true));
                sb.AppendLine();
            }
            return WriteText(fileName, sb.ToString(), "Дерево UI Automation окон клиента");
        }

        /// <summary>
        /// Удалить старые папки артефактов тестов (только с именем вида «yyyyMMdd_HHmmss_fff_…» - прочие папки каталога не трогаются),
        /// оставив keep последних; удалённые перечисляются в журнале. Папку не удалось удалить (открыт файл, нет прав) - предупреждение
        /// в журнале, подготовка тестов продолжается: артефакты - диагностика.
        /// </summary>
        public static void Prune(PilotUiConfig config)
        {
            string root = config.ResolvedArtifactsDir;
            if (!Directory.Exists(root) || config.ArtifactsKeep <= 0)
            {
                return;
            }
            List<DirectoryInfo> folders = new DirectoryInfo(root).GetDirectories()
                .Where(d => FolderName.IsMatch(d.Name))
                .OrderByDescending(d => d.Name, StringComparer.Ordinal).ToList();
            foreach (DirectoryInfo old in folders.Skip(config.ArtifactsKeep))
            {
                try
                {
                    old.Delete(true);
                    PilotLog.Action("удалена старая папка артефактов " + old.FullName);
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    PilotLog.Warn("не удалось удалить старую папку артефактов " + old.FullName + ": " + ex.GetType().Name + ": " + ex.Message);
                }
            }
        }

        internal static string Sanitize(string s, int max)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                s = s.Replace(c, '_');
            }
            return s.Length > max ? s.Substring(0, max) : s;
        }
    }
}
