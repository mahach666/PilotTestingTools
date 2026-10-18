using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace PilotTestHost.Plugin.Reporting
{
    // JSON пишется вручную: хост не должен тащить свои зависимости
    // в процесс клиента (конфликты версий с другими плагинами)
    public static class JsonReporter
    {
        public static string Write(TestRunReport report)
        {
            var directory = Path.Combine(Path.GetTempPath(), "PilotTestHost");
            Directory.CreateDirectory(directory);

            var fileName = "results_" + report.StartedAtUtc.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture) + ".json";
            var path = Path.Combine(directory, fileName);

            File.WriteAllText(path, Serialize(report), new UTF8Encoding(false));
            return path;
        }

        private static string Serialize(TestRunReport report)
        {
            var sb = new StringBuilder();
            sb.Append("{\n  \"summary\": {\n");
            sb.Append("    \"total\": ").Append(report.Total).Append(",\n");
            sb.Append("    \"passed\": ").Append(report.Passed).Append(",\n");
            sb.Append("    \"failed\": ").Append(report.Failed).Append(",\n");
            sb.Append("    \"errored\": ").Append(report.Errored).Append(",\n");
            sb.Append("    \"skipped\": ").Append(report.Skipped).Append(",\n");
            sb.Append("    \"durationMs\": ").Append(report.DurationMs).Append(",\n");
            sb.Append("    \"host\": ").Append(Quote(report.Host)).Append(",\n");
            sb.Append("    \"startedAt\": ").Append(Quote(report.StartedAtUtc.ToString("o", CultureInfo.InvariantCulture)));
            sb.Append("\n  },\n  \"results\": [");

            for (var i = 0; i < report.Results.Count; i++)
            {
                var result = report.Results[i];
                sb.Append(i > 0 ? ",\n    {" : "\n    {");
                sb.Append(" \"class\": ").Append(Quote(result.ClassName));
                sb.Append(", \"method\": ").Append(Quote(result.Method));
                sb.Append(", \"status\": ").Append(Quote(result.Status.ToString().ToLowerInvariant()));
                sb.Append(", \"durationMs\": ").Append(result.DurationMs);

                if (result.Message != null)
                    sb.Append(", \"message\": ").Append(Quote(result.Message));
                if (result.StackTrace != null)
                    sb.Append(", \"stackTrace\": ").Append(Quote(result.StackTrace));

                if (result.Output.Count > 0)
                {
                    sb.Append(", \"output\": [");
                    for (var j = 0; j < result.Output.Count; j++)
                    {
                        if (j > 0) sb.Append(", ");
                        sb.Append(Quote(result.Output[j]));
                    }
                    sb.Append("]");
                }

                sb.Append(" }");
            }

            sb.Append("\n  ]\n}\n");
            return sb.ToString();
        }

        private static string Quote(string value)
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
