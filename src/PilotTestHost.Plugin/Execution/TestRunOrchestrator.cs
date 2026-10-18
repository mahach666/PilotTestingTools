using PilotTestHost.Plugin.Discovery;
using PilotTestHost.Plugin.Reporting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace PilotTestHost.Plugin.Execution
{
    public static class TestRunOrchestrator
    {
        // filter: маски "Namespace.Class.Method" через ';', поддерживается *
        public static TestRunReport Run(ServiceBag services, string filter = null)
        {
            var report = new TestRunReport();
            var stopwatch = Stopwatch.StartNew();

            var descriptors = TestDiscoverer.DiscoverAll();
            var patterns = ParseFilter(filter);
            if (patterns.Count > 0)
            {
                descriptors = descriptors
                    .Where(d => patterns.Any(p => p.IsMatch(d.ClassFullName + "." + d.MethodName)))
                    .ToList();
            }

            foreach (var descriptor in descriptors)
                report.Results.Add(TestRunner.Run(descriptor, services));

            stopwatch.Stop();
            report.DurationMs = stopwatch.ElapsedMilliseconds;
            return report;
        }

        private static List<Regex> ParseFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return new List<Regex>();

            return filter.Split(';')
                .Select(p => p.Trim())
                .Where(p => p.Length > 0)
                .Select(p => new Regex(
                    "^" + Regex.Escape(p).Replace("\\*", ".*") + "$",
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                .ToList();
        }
    }
}
