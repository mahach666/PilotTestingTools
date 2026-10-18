#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace PilotTestingTools.Shared
{
    /// <summary>
    /// Журнал клиента Pilot (%APPDATA%\ASCON\&lt;продукт&gt;\Logs\client.log), дочитываемый с запомненного места:
    /// причина, по которой клиент не вошёл в базу, и последние строки для сообщения об ошибке.
    /// Общий исходник библиотеки PilotUITesting (net48) и pilottest (net8).
    /// </summary>
    internal sealed class ClientLogFile
    {
        /// <summary>
        /// Эту запись клиент делает перед исключением, после которого перестаёт подключаться к серверу
        /// (VersionMatchException, PilotSecurityAccessDeniedException; проверено на Pilot-BIM и Pilot-ICE Enterprise 26.x).
        /// Опечатка «иключение» - как в клиенте, поэтому ищется только хвост фразы.
        /// </summary>
        internal const string BlockingMarker = "блокирующее попытки переподключения к серверу";

        private const int TailSize = 30;
        private const int HeadSize = 64;
        private const string TimestampFormat = "yyyy-MM-dd HH:mm:ss,fff";

        private readonly DateTime _notBefore;
        private readonly Queue<string> _tail = new Queue<string>();
        private long _offset;
        // начало файла (первая запись с меткой времени до миллисекунд): другое начало - журнал начат заново
        private byte[] _head;
        private bool _inRange;
        private bool _afterMarker;

        private ClientLogFile(string path, long offset, byte[] head, DateTime notBefore)
        {
            Path = path;
            _offset = offset;
            _head = head;
            _notBefore = notBefore;
            _inRange = notBefore == DateTime.MinValue;
        }

        public string Path { get; }

        /// <summary>Исключение из журнала («Тип: сообщение»), после которого клиент не подключается; null - такого не было.</summary>
        public string? Failure { get; private set; }

        /// <summary>Последняя ошибка чтения журнала (файл занят, нет прав); null - читался без ошибок.</summary>
        public string? ReadError { get; private set; }

        /// <summary>Последние прочитанные строки журнала (не больше 30).</summary>
        public IReadOnlyCollection<string> Tail => _tail;

        /// <summary>Папка продукта в %APPDATA%\ASCON по имени exe клиента; null - exe не известен.</summary>
        public static string? ProductFolder(string clientExePath)
        {
            string name = System.IO.Path.GetFileName(clientExePath);
            if (string.Equals(name, "Ascon.Pilot.PilotEnterprise.exe", StringComparison.OrdinalIgnoreCase))
            {
                return "Pilot-ICE Enterprise";
            }
            if (string.Equals(name, "Ascon.Pilot.PilotBIM.exe", StringComparison.OrdinalIgnoreCase))
            {
                return "Pilot-BIM";
            }
            return null;
        }

        public static string DefaultPath(string productFolder)
        {
            return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ASCON", productFolder, "Logs", "client.log");
        }

        /// <summary>Только записи, сделанные после этого вызова: создавать до запуска клиента.</summary>
        public static ClientLogFile FromNow(string path)
        {
            if (!File.Exists(path))
            {
                return new ClientLogFile(path, 0, new byte[0], DateTime.MinValue);
            }
            using (FileStream stream = Open(path))
            {
                return new ClientLogFile(path, stream.Length, ReadHead(stream), DateTime.MinValue);
            }
        }

        /// <summary>Записи не раньше notBefore (клиент уже запущен: время старта его процесса).</summary>
        public static ClientLogFile Since(string path, DateTime notBefore)
        {
            return new ClientLogFile(path, 0, new byte[0], notBefore);
        }

        /// <summary>Пояснение к <see cref="Failure"/> для сообщения об ошибке.</summary>
        public static string Explain(string failure)
        {
            if (failure.IndexOf("VersionMatchException", StringComparison.Ordinal) >= 0)
            {
                return "версии клиента и сервера Pilot не совпадают (" + failure + ")";
            }
            if (failure.IndexOf("PilotSecurityAccessDeniedException", StringComparison.Ordinal) >= 0)
            {
                return "сервер отклонил сохранённый вход (" + failure + "). Сохраните вход заново: pilottest --login <сервер> <пользователь> <пароль> или PilotClientLogin.Save";
            }
            return "клиент прекратил попытки подключения к серверу (" + failure + ")";
        }

        /// <summary>Дочитать новые полные строки журнала. Возвращает <see cref="Failure"/>.</summary>
        public string? Poll()
        {
            if (Failure != null)
            {
                return Failure;
            }
            byte[] chunk;
            try
            {
                using (FileStream stream = Open(Path))
                {
                    byte[] head = ReadHead(stream);
                    if (stream.Length < _offset || !StartsWith(head, _head))
                    {
                        // журнал начат заново (client.log переименован в client.log.1)
                        _offset = 0;
                    }
                    if (head.Length > _head.Length)
                    {
                        _head = head;
                    }
                    chunk = ReadFrom(stream, _offset);
                }
                ReadError = null;
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            catch (DirectoryNotFoundException)
            {
                return null;
            }
            catch (IOException ex)
            {
                ReadError = ex.Message;
                return null;
            }
            catch (UnauthorizedAccessException ex)
            {
                ReadError = ex.Message;
                return null;
            }
            int end = Array.LastIndexOf(chunk, (byte)'\n');
            if (end < 0)
            {
                return null;
            }
            // только до последнего перевода строки: недописанная строка (и разрезанный символ UTF-8) дочитается в следующий раз
            _offset += end + 1;
            foreach (string line in Encoding.UTF8.GetString(chunk, 0, end).Split('\n'))
            {
                Feed(line);
            }
            return Failure;
        }

        private static FileStream Open(string path)
        {
            return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        }

        private static byte[] ReadHead(FileStream stream)
        {
            return ReadFrom(stream, 0, HeadSize);
        }

        private static byte[] ReadFrom(FileStream stream, long offset, int? limit = null)
        {
            byte[] buffer = new byte[Math.Max(0, Math.Min(stream.Length - offset, limit ?? int.MaxValue))];
            stream.Position = offset;
            int read = 0;
            while (read < buffer.Length)
            {
                int n = stream.Read(buffer, read, buffer.Length - read);
                if (n == 0)
                {
                    break;
                }
                read += n;
            }
            if (read < buffer.Length)
            {
                Array.Resize(ref buffer, read);
            }
            return buffer;
        }

        private static bool StartsWith(byte[] data, byte[] prefix)
        {
            if (data.Length < prefix.Length)
            {
                return false;
            }
            for (int i = 0; i < prefix.Length; i++)
            {
                if (data[i] != prefix[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>Разобрать одну строку журнала (без перевода строки).</summary>
        internal void Feed(string line)
        {
            line = line.TrimEnd('\r').TrimStart('﻿');
            if (line.Length >= TimestampFormat.Length
                && DateTime.TryParseExact(line.Substring(0, TimestampFormat.Length), TimestampFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime time))
            {
                _inRange = time >= _notBefore;
                _afterMarker = _inRange && line.IndexOf(BlockingMarker, StringComparison.Ordinal) >= 0;
            }
            else if (_afterMarker && line.Trim().Length > 0)
            {
                // следующая строка записи - «Тип исключения: сообщение»
                Failure = Failure ?? line.Trim();
                _afterMarker = false;
            }
            if (_inRange)
            {
                _tail.Enqueue(line);
                if (_tail.Count > TailSize)
                {
                    _tail.Dequeue();
                }
            }
        }
    }
}
