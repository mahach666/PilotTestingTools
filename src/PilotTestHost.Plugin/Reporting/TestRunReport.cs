using System;
using System.Collections.Generic;
using System.Linq;

namespace PilotTestHost.Plugin.Reporting
{
    public sealed class TestRunReport
    {
        public DateTime StartedAtUtc { get; } = DateTime.UtcNow;
        public string Host { get; } = "PilotTestHost 0.1.0";
        public List<TestResult> Results { get; } = new List<TestResult>();

        public int Total => Results.Count;
        public int Passed => Results.Count(r => r.Status == TestStatus.Passed);
        public int Failed => Results.Count(r => r.Status == TestStatus.Failed);
        public int Errored => Results.Count(r => r.Status == TestStatus.Error);
        public int Skipped => Results.Count(r => r.Status == TestStatus.Skipped);
        public long DurationMs { get; set; }
    }
}
