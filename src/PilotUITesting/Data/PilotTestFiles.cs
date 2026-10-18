using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace PilotUITesting.Data
{
    /// <summary>Файлы для тестов: минимальный PDF и ожидание файла, записанного клиентом.</summary>
    public static class PilotTestFiles
    {
        private static readonly string RunFolder = Path.Combine(Path.GetTempPath(), "PilotUITesting", "files", DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + Process.GetCurrentProcess().Id);

        /// <summary>Валидный PDF 1.4 с одной страницей и строкой текста (латиница, Helvetica) в папке прогона; возвращает путь.</summary>
        public static string Pdf(string text, string fileName = null)
        {
            Directory.CreateDirectory(RunFolder);
            string name = fileName ?? Sanitize(text) + ".pdf";
            string path = Path.Combine(RunFolder, name);
            File.WriteAllBytes(path, MinimalPdf(text));
            return path;
        }

        /// <summary>Байты минимального PDF.</summary>
        public static byte[] MinimalPdf(string text)
        {
            string safe = (text ?? "").Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
            string stream = $"BT /F1 18 Tf 50 750 Td ({safe}) Tj ET";
            List<string> objects = new List<string>
            {
                "<< /Type /Catalog /Pages 2 0 R >>",
                "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
                "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>",
                $"<< /Length {Encoding.ASCII.GetByteCount(stream)} >>\nstream\n{stream}\nendstream",
                "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
            };
            StringBuilder sb = new StringBuilder();
            sb.Append("%PDF-1.4\n");
            List<int> offsets = new List<int>();
            for (int i = 0; i < objects.Count; i++)
            {
                offsets.Add(Encoding.ASCII.GetByteCount(sb.ToString()));
                sb.Append($"{i + 1} 0 obj\n{objects[i]}\nendobj\n");
            }
            int xref = Encoding.ASCII.GetByteCount(sb.ToString());
            sb.Append($"xref\n0 {objects.Count + 1}\n0000000000 65535 f \n");
            foreach (int offset in offsets)
            {
                sb.Append(offset.ToString("D10") + " 00000 n \n");
            }
            sb.Append($"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n");
            return Encoding.ASCII.GetBytes(sb.ToString());
        }

        /// <summary>Ждать, что файл записан и закрыт писателем: есть, размер больше нуля, открывается монопольно.</summary>
        public static void WaitWritten(string path, TimeSpan? timeout = null)
        {
            TimeSpan limit = timeout ?? Uia.Wait.DefaultTimeout;
            Stopwatch sw = Stopwatch.StartNew();
            Exception last = null;
            while (true)
            {
                bool expired = sw.Elapsed >= limit;
                if (File.Exists(path) && new FileInfo(path).Length > 0)
                {
                    try
                    {
                        using (new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
                        {
                            return;
                        }
                    }
                    catch (IOException ex)
                    {
                        last = ex;
                    }
                }
                if (expired)
                {
                    throw new Uia.PilotWaitTimeoutException("файл записан: " + path, limit, sw.Elapsed, File.Exists(path) ? "размер " + new FileInfo(path).Length : "файла нет", last);
                }
                Thread.Sleep(Uia.Wait.PollInterval);
            }
        }

        private static string Sanitize(string text)
        {
            string s = new string((text ?? "file").Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c).ToArray()).Trim();
            if (s.Length == 0)
            {
                s = "file";
            }
            return s.Length > 60 ? s.Substring(0, 60) : s;
        }
    }

    /// <summary>
    /// Снимок локальных файлов (настройки модулей у пользователя клиента) и восстановление с побайтной проверкой.
    /// «Файла не было» тоже запоминается: при восстановлении такой файл удаляется.
    /// </summary>
    public sealed class LocalFileBackup
    {
        private readonly Dictionary<string, byte[]> _content;

        private LocalFileBackup(Dictionary<string, byte[]> content)
        {
            _content = content;
        }

        public IReadOnlyList<string> Paths => _content.Keys.ToList();

        public static LocalFileBackup Take(params string[] paths)
        {
            Dictionary<string, byte[]> content = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
            foreach (string path in paths)
            {
                string full = Path.GetFullPath(path);
                content[full] = File.Exists(full) ? File.ReadAllBytes(full) : null;
            }
            return new LocalFileBackup(content);
        }

        /// <summary>Вернуть файлы к снимку и сверить побайтно; расхождение - исключение.</summary>
        public void Restore()
        {
            List<string> problems = new List<string>();
            foreach (KeyValuePair<string, byte[]> pair in _content)
            {
                if (pair.Value == null)
                {
                    if (File.Exists(pair.Key))
                    {
                        File.Delete(pair.Key);
                    }
                    if (File.Exists(pair.Key))
                    {
                        problems.Add(pair.Key + ": не удалён");
                    }
                    continue;
                }
                Directory.CreateDirectory(Path.GetDirectoryName(pair.Key));
                File.WriteAllBytes(pair.Key, pair.Value);
                if (!File.ReadAllBytes(pair.Key).SequenceEqual(pair.Value))
                {
                    problems.Add(pair.Key + ": содержимое после восстановления отличается");
                }
            }
            if (problems.Count > 0)
            {
                throw new IOException("Локальные файлы не восстановлены: " + string.Join("; ", problems));
            }
        }
    }
}
