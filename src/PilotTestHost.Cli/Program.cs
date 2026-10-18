using System.Diagnostics;
using System.Text.Json;

namespace PilotTestHost.Cli;

// Внешний раннер: запускает клиент Pilot с env-протоколом, ждет отчет, печатает сводку.
// Коды возврата: 0 - все зеленые; N - количество failed+errored;
// 101 - инфраструктура (клиент не найден, таймаут, нет отчета); 102 - фатальная ошибка в прогоне
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
        var timeoutMinutes = 30;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--client": clientPath = Next(args, ref i); break;
                case "--filter": filter = Next(args, ref i); break;
                case "--output": output = Next(args, ref i); break;
                case "--timeout-minutes": timeoutMinutes = int.Parse(Next(args, ref i)); break;
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

        output ??= Path.Combine(Path.GetTempPath(), "PilotTestHost",
            $"cli_results_{DateTime.Now:yyyyMMdd_HHmmss}.json");
        Directory.CreateDirectory(Path.GetDirectoryName(output)!);

        // Существование файла после прогона - признак успеха, старый удаляем
        File.Delete(output);

        Console.WriteLine($"Клиент : {clientPath}");
        Console.WriteLine($"Отчет  : {output}");
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

        if (!process.WaitForExit((int)TimeSpan.FromMinutes(timeoutMinutes).TotalMilliseconds))
        {
            Console.Error.WriteLine("Таймаут: клиент не завершился, процесс будет убит.");
            try { process.Kill(entireProcessTree: true); } catch { }
            return 101;
        }

        if (!File.Exists(output))
        {
            Console.Error.WriteLine("Отчет не появился. Вероятные причины:");
            Console.Error.WriteLine(" - плагин PilotTestHost не установлен в Development-папку этого клиента;");
            Console.Error.WriteLine(" - клиент показал окно логина (нужен сохраненный вход);");
            Console.Error.WriteLine(" - клиент упал при старте.");
            return 101;
        }

        return PrintReport(output);
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
        Console.WriteLine();
        Console.WriteLine("Требования: плагин PilotTestHost установлен в Development-папку клиента,");
        Console.WriteLine("тестовые *Tests.dll лежат в его подпапке Tests, у клиента сохранен вход.");
    }
}
