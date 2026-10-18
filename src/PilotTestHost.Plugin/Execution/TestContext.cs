using Ascon.Pilot.SDK;
using PilotTestHost.Sdk;
using System.Collections.Generic;

namespace PilotTestHost.Plugin.Execution
{
    internal sealed class TestContext : ITestContext
    {
        private readonly ServiceBag _services;

        public TestContext(ServiceBag services, string testName, ReportLogger logger)
        {
            _services = services;
            TestName = testName;
            Logger = logger;
        }

        public IObjectsRepository Repository => _services.Repository;
        public ITabServiceProvider TabService => _services.TabService;
        public IPilotDialogService DialogService => _services.DialogService;
        public IFileProvider FileProvider => _services.FileProvider;
        public ISearchService SearchService => _services.SearchService;

        public IPilotTestLogger Logger { get; }
        public string TestName { get; }

        public T GetService<T>() where T : class
        {
            if (typeof(T) == typeof(IObjectsRepository)) return Repository as T;
            if (typeof(T) == typeof(ITabServiceProvider)) return TabService as T;
            if (typeof(T) == typeof(IPilotDialogService)) return DialogService as T;
            if (typeof(T) == typeof(IFileProvider)) return FileProvider as T;
            if (typeof(T) == typeof(ISearchService)) return SearchService as T;

            return _services.GetExtra(typeof(T)) as T;
        }
    }

    // Вывод тестов собирается в отчет
    internal sealed class ReportLogger : IPilotTestLogger
    {
        public List<string> Lines { get; } = new List<string>();

        public void Info(string message) => Lines.Add("[INFO] " + message);
        public void Warn(string message) => Lines.Add("[WARN] " + message);
        public void Error(string message) => Lines.Add("[ERROR] " + message);
    }
}
