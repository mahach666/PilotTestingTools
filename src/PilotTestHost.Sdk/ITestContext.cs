using Ascon.Pilot.SDK;
using System;

namespace PilotTestHost.Sdk
{
    // Контекст теста: сервисы клиента Pilot, полученные хостом через MEF
    public interface ITestContext
    {
        IObjectsRepository Repository { get; }
        ITabServiceProvider TabService { get; }
        IPilotDialogService DialogService { get; }
        IFileProvider FileProvider { get; }

        // null, если клиент не предоставил сервис
        ISearchService SearchService { get; }

        // Доступ к прочим захваченным сервисам (например, BIM) по типу; null - недоступен
        T GetService<T>() where T : class;

        // Вывод попадает в отчет теста
        IPilotTestLogger Logger { get; }

        string TestName { get; }
    }
}
