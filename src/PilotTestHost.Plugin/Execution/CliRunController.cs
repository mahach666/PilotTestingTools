using PilotTestHost.Plugin.Reporting;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PilotTestHost.Plugin.Execution
{
    // Headless-прогон: клиент запущен внешним раннером с PILOTTESTHOST_CLI=1.
    // Ждем готовности клиента, гоняем тесты на UI-потоке, пишем отчет и гасим процесс
    public static class CliRunController
    {
        private const int ReadyPollDelayMs = 2000;
        private const int ReadyPollAttempts = 150;        // ~5 минут на логин/подключение
        private const int ShutdownGraceMs = 10000;

        public static void StartIfRequested(ServiceBag services)
        {
            if (Environment.GetEnvironmentVariable(CliConstants.CliModeVariable) != "1")
                return;

            // Из MEF-конструктора запускать нельзя - клиент еще не готов
            Task.Run(() => WaitAndRunAsync(services));
        }

        private static async Task WaitAndRunAsync(ServiceBag services)
        {
            var output = Environment.GetEnvironmentVariable(CliConstants.OutputVariable);

            try
            {
                await WaitForClientReadyAsync(services);

                var filter = Environment.GetEnvironmentVariable(CliConstants.FilterVariable);

                TestRunReport report = null;
                await RunOnUiThreadAsync(() => report = TestRunOrchestrator.Run(services, filter));

                var jsonPath = JsonReporter.Write(report);

                if (!string.IsNullOrWhiteSpace(output))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(output) ?? ".");
                    File.Copy(jsonPath, output, overwrite: true);
                }
            }
            catch (Exception e)
            {
                WriteErrorMarker(output, e);
            }
            finally
            {
                await ShutdownClientAsync();
            }
        }

        // Готовность = репозиторий отвечает (логин прошел, подключение установлено)
        private static async Task WaitForClientReadyAsync(ServiceBag services)
        {
            for (var attempt = 0; attempt < ReadyPollAttempts; attempt++)
            {
                await Task.Delay(ReadyPollDelayMs);

                try
                {
                    var dispatcher = Application.Current?.Dispatcher;
                    if (dispatcher == null)
                        continue;

                    var ready = await dispatcher.InvokeAsync(() =>
                    {
                        try
                        {
                            return services.Repository.GetDatabaseId() != Guid.Empty;
                        }
                        catch
                        {
                            return false;
                        }
                    });

                    if (ready)
                        return;
                }
                catch
                {
                    // Клиент еще поднимается - пробуем дальше
                }
            }

            throw new TimeoutException("Клиент Pilot не стал готов за отведенное время (логин не пройден?)");
        }

        private static async Task RunOnUiThreadAsync(Action action)
        {
            var dispatcher = Application.Current?.Dispatcher
                ?? throw new InvalidOperationException("UI-диспетчер клиента недоступен");
            await dispatcher.InvokeAsync(action);
        }

        private static void WriteErrorMarker(string output, Exception e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(output))
                    return;

                Directory.CreateDirectory(Path.GetDirectoryName(output) ?? ".");
                File.WriteAllText(output, "{ \"error\": " + JsonQuote(e.ToString()) + " }", new UTF8Encoding(false));
            }
            catch
            {
                // Отчет об ошибке - best effort
            }
        }

        // Штатного API выхода из клиента нет: пробуем мягкий Shutdown,
        // через паузу гарантированно убиваем процесс (отчет уже на диске)
        private static async Task ShutdownClientAsync()
        {
            try
            {
                var dispatcher = Application.Current?.Dispatcher;
                if (dispatcher != null)
                    await dispatcher.InvokeAsync(() => Application.Current.Shutdown());
            }
            catch
            {
            }

            await Task.Delay(ShutdownGraceMs);
            Process.GetCurrentProcess().Kill();
        }

        private static string JsonQuote(string value)
        {
            var sb = new StringBuilder(value.Length + 2);
            sb.Append('"');
            foreach (var c in value)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < ' ')
                            sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        else
                            sb.Append(c);
                        break;
                }
            }
            sb.Append('"');
            return sb.ToString();
        }
    }
}
