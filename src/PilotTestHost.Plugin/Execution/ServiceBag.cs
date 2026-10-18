using Ascon.Pilot.SDK;
using System;
using System.Collections.Generic;

namespace PilotTestHost.Plugin.Execution
{
    // Сервисы клиента, захваченные при MEF-композиции хоста
    public sealed class ServiceBag
    {
        private readonly Dictionary<Type, object> _extras = new Dictionary<Type, object>();

        public ServiceBag(
            IObjectsRepository repository,
            ITabServiceProvider tabService,
            IPilotDialogService dialogService,
            IFileProvider fileProvider,
            ISearchService searchService)
        {
            Repository = repository;
            TabService = tabService;
            DialogService = dialogService;
            FileProvider = fileProvider;
            SearchService = searchService;
        }

        public IObjectsRepository Repository { get; }
        public ITabServiceProvider TabService { get; }
        public IPilotDialogService DialogService { get; }
        public IFileProvider FileProvider { get; }
        public ISearchService SearchService { get; }

        public void RegisterExtra(Type type, object service)
        {
            if (service != null)
                _extras[type] = service;
        }

        public object GetExtra(Type type) =>
            _extras.TryGetValue(type, out var service) ? service : null;
    }
}
