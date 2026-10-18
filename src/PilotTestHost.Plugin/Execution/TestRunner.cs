using PilotTestHost.Plugin.Discovery;
using PilotTestHost.Plugin.Reporting;
using System;
using System.Diagnostics;
using System.Reflection;

namespace PilotTestHost.Plugin.Execution
{
    public static class TestRunner
    {
        public static TestResult Run(TestDescriptor descriptor, ServiceBag services)
        {
            if (descriptor.SignatureError != null)
            {
                return new TestResult(descriptor.ClassFullName, descriptor.MethodName,
                    TestStatus.Error, 0, "Неверная сигнатура: " + descriptor.SignatureError);
            }

            if (!string.IsNullOrEmpty(descriptor.SkipReason))
            {
                return new TestResult(descriptor.ClassFullName, descriptor.MethodName,
                    TestStatus.Skipped, 0, descriptor.SkipReason);
            }

            var logger = new ReportLogger();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var instance = Activator.CreateInstance(descriptor.DeclaringType);
                var arguments = descriptor.TakesContext
                    ? new object[] { new TestContext(services, descriptor.MethodName, logger) }
                    : null;

                descriptor.Method.Invoke(instance, arguments);
                stopwatch.Stop();

                (instance as IDisposable)?.Dispose();

                return new TestResult(descriptor.ClassFullName, descriptor.MethodName,
                    TestStatus.Passed, stopwatch.ElapsedMilliseconds, output: logger.Lines);
            }
            catch (TargetInvocationException e) when (e.InnerException != null)
            {
                stopwatch.Stop();
                var inner = e.InnerException;

                // Исключения assert-библиотек считаем падением теста, прочее - ошибкой
                var typeName = inner.GetType().FullName ?? "";
                var status = typeName.Contains("Xunit") || typeName.Contains("Assert") || typeName.Contains("FluentAssertions")
                    ? TestStatus.Failed
                    : TestStatus.Error;

                return new TestResult(descriptor.ClassFullName, descriptor.MethodName,
                    status, stopwatch.ElapsedMilliseconds, inner.Message, inner.StackTrace, logger.Lines);
            }
            catch (Exception e)
            {
                stopwatch.Stop();
                return new TestResult(descriptor.ClassFullName, descriptor.MethodName,
                    TestStatus.Error, stopwatch.ElapsedMilliseconds, e.Message, e.StackTrace, logger.Lines);
            }
        }
    }
}
