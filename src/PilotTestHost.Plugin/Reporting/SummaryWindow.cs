using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace PilotTestHost.Plugin.Reporting
{
    // Простое окно-сводка: моноширинный текст, копируется целиком
    public static class SummaryWindow
    {
        public static void Show(TestRunReport report, string jsonPath)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Пройдено: {report.Passed}   Упало: {report.Failed}   Ошибок: {report.Errored}   Пропущено: {report.Skipped}");
            sb.AppendLine($"Всего: {report.Total}   Время: {report.DurationMs} мс");
            sb.AppendLine($"Отчет: {jsonPath}");
            sb.AppendLine();

            if (report.Total == 0)
            {
                sb.AppendLine("Тесты не найдены. Положите *Tests.dll в папку Tests рядом с плагином.");
            }

            foreach (var result in report.Results)
            {
                sb.AppendLine($"[{result.Status.ToString().ToUpperInvariant()}] {result.ClassName}.{result.Method} ({result.DurationMs} мс)");

                if (result.Message != null)
                    sb.AppendLine("    " + result.Message);

                foreach (var line in result.Output)
                    sb.AppendLine("    " + line);
            }

            var window = new Window
            {
                Title = $"PilotTestHost - {report.Passed}/{report.Total} passed",
                Width = 720,
                Height = 480,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Content = new TextBox
                {
                    Text = sb.ToString(),
                    IsReadOnly = true,
                    FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                    TextWrapping = TextWrapping.NoWrap,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                    BorderThickness = new Thickness(0)
                }
            };

            window.Show();
        }
    }
}
