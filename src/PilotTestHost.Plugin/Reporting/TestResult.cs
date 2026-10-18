using System.Collections.Generic;

namespace PilotTestHost.Plugin.Reporting
{
    public enum TestStatus
    {
        Passed,
        Failed,
        Skipped,
        Error
    }

    public sealed class TestResult
    {
        public TestResult(string className, string method, TestStatus status, long durationMs,
            string message = null, string stackTrace = null, IReadOnlyList<string> output = null)
        {
            ClassName = className;
            Method = method;
            Status = status;
            DurationMs = durationMs;
            Message = message;
            StackTrace = stackTrace;
            Output = output ?? new List<string>();
        }

        public string ClassName { get; }
        public string Method { get; }
        public TestStatus Status { get; }
        public long DurationMs { get; }
        public string Message { get; }
        public string StackTrace { get; }
        public IReadOnlyList<string> Output { get; }
    }
}
