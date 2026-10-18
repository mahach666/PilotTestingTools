using System.Diagnostics;
using System.Text.Json;
using PilotTestingTools.Shared;

namespace PilotTestHost.Cli;

// Внешний раннер: запускает клиент Pilot с env-протоколом, ждет отчет, печатает сводку.
// Коды возврата: 0 - все зеленые; N - количество failed+errored;
// 101 - инфраструктура (клиент не найден, уже запущен, не вошел в базу, таймаут, нет отчета); 102 - фатальная ошибка в прогоне
internal static class Program
{
    // Дублируют CliConstants хоста (net8 не может ссылаться на net48) - менять синхронно
    private const string CliModeVariable = "PILOTTESTHOST_CLI";
    private const string FilterVariable = "PILOTTESTHOST_FILTER";
    private const string OutputVariable = "PILOTTESTHOST_OUTPUT";

    private static readonly string[] DefaultClientPaths =
    {
        @"C:\Program Files\ASCON\Pilot-ICE Enterprise\Ascon.Pilot.PilotEnterprise.exe",
        @"C:\Program Files\ASCON\Pilot-BIM\Ascon.Pilot.PilotBIM.exe"
    };

    private static int Main(string[] args)
    {
        string? clientPath = null;
        string? filter = null;
        string? output = null;
        string[]? login = null;
        var timeoutMinutes = 30;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--client": clientPath = Next(args, ref i); break;
                case "--filter": filter = Next(args, ref i); break;
                case "--output": output = Next(args, ref i); break;
                case "--timeout-minutes": timeoutMinutes = int.Parse(Next(args, ref i)); break;
                case "--login": login = new[] { Next(args, ref i), Next(args, ref i), Next(args, ref i) }; break;
                case "--help" or "-h" or "/?": PrintUsage(); return 0;
                default:
                    Console.Error.WriteLine($"Неизвестный аргумент: {args[i]}");
                    PrintUsage();
                    return 101;
            }
        }

        clientPath ??= DefaultClientPaths.FirstOrDefault(File.Exists);
        if (clientPath == null || !File.Exists(clientPath))
        {
            Console.Error.WriteLine("Клиент Pilot не найден. Укажите путь через --client <exe>.");
            return 101;
        }

        var product = ClientLogFile.ProductFolder(clientPath);
        if (login != null)
            return SaveLogin(clientPath, product, login[0], login[1], login[2]);

        var running = RunningClient(clientPath);
        if (running != null)
        {
            Console.Error.WriteLine($"Клиент уже запущен (PID {running.Id}). Второй экземпляр сразу закрывается, тесты не выполнятся. Закройте клиент.");
            return 101;
        }

        output ??= Path.Combine(Path.GetTempPath(), "PilotTestHost",
            $"cli_results_{DateTime.Now:yyyyMMdd_HHmmss}.json");
        Directory.CreateDirectory(Path.GetDirectoryName(output)!);

        // Существование файла после прогона - признак успеха, старый удаляем
        File.Delete(output);

        // Журнал клиента читается с текущего конца: в нем ищется причина, по которой клиент не вошел в базу
        var log = product == null ? null : ClientLogFile.FromNow(ClientLogFile.DefaultPath(product));

        Console.WriteLine($"Клиент : {clientPath}");
        Console.WriteLine($"Отчет  : {output}");
        Console.WriteLine($"Журнал : {log?.Path ?? "не проверяется (неизвестный exe клиента)"}");
        if (filter != null)
            Console.WriteLine($"Фильтр : {filter}");
        Console.WriteLine($"Таймаут: {timeoutMinutes} мин");
        Console.WriteLine();

        var psi = new ProcessStartInfo
        {
            FileName = clientPath,
            UseShellExecute = false
        };
        psi.Environment[CliModeVariable] = "1";
        psi.Environment[OutputVariable] = output;
        if (!string.IsNullOrWhiteSpace(filter))
            psi.Environment[FilterVariable] = filter;

        using var process = Process.Start(psi);
        if (process == null)
        {
            Console.Error.WriteLine("Не удалось запустить клиент.");
            return 101;
        }

        Console.WriteLine($"Клиент запущен (PID {process.Id}), ожидание завершения прогона...");

        var timeout = TimeSpan.FromMinutes(timeoutMinutes);
        var stopwatch = Stopwatch.StartNew();
        while (!process.WaitForExit(1000))
        {
            var failure = log?.Poll();
            if (failure != null)
            {
                Console.Error.WriteLine("Клиент не вошел в базу: " + ClientLogFile.Explain(failure));
                Console.Error.WriteLine("Журнал клиента: " + log!.Path);
                Kill(process);
                return 101;
            }
            if (stopwatch.Elapsed >= timeout)
            {
                Console.Error.WriteLine("Таймаут: клиент не завершился, процесс будет убит.");
                Kill(process);
                PrintLogTail(log);
                return 101;
            }
        }

        if (!File.Exists(output))
        {
            Console.Error.WriteLine($"Отчет не появился (клиент завершился с кодом {process.ExitCode}).");
            var failure = log?.Poll();
            if (failure != null)
            {
                Console.Error.WriteLine("Клиент не вошел в базу: " + ClientLogFile.Explain(failure));
                return 101;
            }
            Console.Error.WriteLine("Вероятные причины:");
            Console.Error.WriteLine(" - плагин PilotTestHost не установлен в Development-папку этого клиента;");
            Console.Error.WriteLine(" - у клиента нет сохраненного входа (pilottest --login <сервер> <пользователь> <пароль>);");
            Console.Error.WriteLine(" - клиент упал при старте.");
            PrintLogTail(log);
            return 101;
        }

        return PrintReport(output);
    }

    private static int SaveLogin(string clientPath, string? product, string server, string user, string password)
    {
        if (product == null)
        {
            Console.Error.WriteLine($"Не определить продукт по exe клиента: {Path.GetFileName(clientPath)}. Ожидается Ascon.Pilot.PilotEnterprise.exe или Ascon.Pilot.PilotBIM.exe.");
            return 101;
        }
        if (password == "-")
            password = Console.ReadLine() ?? "";
        try
        {
            ClientLoginFile.Save(clientPath, product, server, user, password);
        }
        catch (Exception e) when (e is InvalidOperationException or IOException or ArgumentException or InvalidDataException or UnauthorizedAccessException or System.Xml.XmlException)
        {
            Console.Error.WriteLine("Вход не сохранен: " + e.Message);
            return 101;
        }
        Console.WriteLine($"Сохранен вход {user} в {server} для {product} ({ClientLoginFile.SettingsPath(product)})");
        return 0;
    }

    private static Process? RunningClient(string clientPath)
    {
        var session = Process.GetCurrentProcess().SessionId;
        return Process.GetProcessesByName(Path.GetFileNameWithoutExtension(clientPath)).FirstOrDefault(p => p.SessionId == session);
    }

    private static void Kill(Process process)
    {
        try
        {
            process.Kill(entireProcessTree: true);
        }
        catch (InvalidOperationException)
        {
            // процесс уже завершился
        }
    }

    private static void PrintLogTail(ClientLogFile? log)
    {
        if (log == null)
            return;
        log.Poll();
        if (log.ReadError != null)
            Console.Error.WriteLine($"Журнал клиента не прочитан ({log.Path}): {log.ReadError}");
        if (log.Tail.Count == 0)
            return;
        Console.Error.WriteLine();
        Console.Error.WriteLine($"Последние строки журнала клиента ({log.Path}):");
        foreach (var line in log.Tail)
            Console.Error.WriteLine("  " + line);
    }

    private static int PrintReport(string path)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        var root = doc.RootElement;

        if (root.TryGetProperty("error", out var error))
        {
            Console.Error.WriteLine("Фатальная ошибка в прогоне:");
            Console.Error.WriteLine(error.GetString());
            return 102;
        }

        var summary = root.GetProperty("summary");
        var passed = summary.GetProperty("passed").GetInt32();
        var failed = summary.GetProperty("failed").GetInt32();
        var errored = summary.GetProperty("errored").GetInt32();
        var skipped = summary.GetProperty("skipped").GetInt32();
        var total = summary.GetProperty("total").GetInt32();
        var duration = summary.GetProperty("durationMs").GetInt64();

        Console.WriteLine($"Итог: {passed} passed, {failed} failed, {errored} errored, {skipped} skipped (всего {total}, {duration} мс)");

        if (failed + errored > 0)
        {
            Console.WriteLine();
            foreach (var result in root.GetProperty("results").EnumerateArray())
            {
                var status = result.GetProperty("status").GetString();
                if (status is not ("failed" or "error"))
                    continue;

                var cls = result.GetProperty("class").GetString();
                var method = result.GetProperty("method").GetString();
                var message = result.TryGetProperty("message", out var m) ? m.GetString() : null;
                Console.WriteLine($"  [{status?.ToUpperInvariant()}] {cls}.{method}: {message}");
            }
        }

        return failed + errored;
    }

    private static string Next(string[] args, ref int i)
    {
        if (i + 1 >= args.Length)
            throw new ArgumentException($"Аргумент {args[i]} требует значения");
        return args[++i];
    }

    private static void PrintUsage()
    {
        Console.WriteLine("pilottest [--client <путь к exe клиента>] [--filter \"Ns.Class.Method;Ns.Other.*\"]");
        Console.WriteLine("          [--output <путь.json>] [--timeout-minutes <n>]");
        Console.WriteLine("pilottest [--client <путь к exe клиента>] --login <сервер> <пользователь> <пароль | ->");
        Console.WriteLine();
        Console.WriteLine("Требования: плагин PilotTestHost установлен в Development-папку клиента,");
        Console.WriteLine("тестовые *Tests.dll лежат в его подпапке Tests, у клиента сохранен вход.");
        Console.WriteLine();
        Console.WriteLine("--login сохраняет вход клиента (клиент должен быть закрыт) и завершается, тесты не запускаются.");
        Console.WriteLine("Сервер - адрес с базой, например http://host:5545/database. Пароль \"-\" читается из stdin.");
    }
}
