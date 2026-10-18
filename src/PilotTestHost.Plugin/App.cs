using Ascon.Pilot.SDK;
using Ascon.Pilot.SDK.Menu;
using PilotTestHost.Plugin.Discovery;
using PilotTestHost.Plugin.Execution;
using PilotTestHost.Plugin.Reporting;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace PilotTestHost.Plugin
{
    // MEF-точка входа хоста: пункт меню «Run Tests», сервисы клиента
    // захватываются при композиции и отдаются тестам через ITestContext
    [Export(typeof(IMenu<MainViewContext>))]
    public class App : IMenu<MainViewContext>
    {
        // Имя попадает в WPF FrameworkElement.Name - допустимы только буквы/цифры/подчёркивание (без точек)
        private const string RunAllItem = "PilotTestHostRunAll";

        private readonly ServiceBag _services;

        [ImportingConstructor]
        public App(IObjectsRepository objectsRepository,
            ITabServiceProvider tabServiceProvider,
            IPilotDialogService pilotDialogService,
            IPilotServiceProvider serviceProvider,
            IFileProvider fileProvider,
            [Import(AllowDefault = true)] ISearchService searchService,
            // Модификатор нужен тестам, которые проверяют реакцию клиента на изменения:
            // его Apply создаёт настоящий changeset и проходит через обработчики всех расширений.
            // Экспорт помечен RegisterAsNew, поэтому хост получает собственный экземпляр.
            [Import(AllowDefault = true)] IObjectModifier objectModifier,
            // Переходы состояний ограничены правами: сервер отклоняет недопустимый переход
            // уже после применения changeset. Менеджер отдаёт только разрешённые переходы.
            [Import(AllowDefault = true)] ITransitionManager transitionManager,
            // Личные настройки нужны тестам, которые проверяют поведение расширений,
            // настраиваемых через страницу настроек клиента: тест кладёт нужное значение
            // тем же путём, что и страница настроек (IPersonalSettings.ChangeSettingValue).
            [Import(AllowDefault = true)] IPersonalSettings personalSettings)
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveFromPluginDirectory;

            _services = new ServiceBag(objectsRepository, tabServiceProvider,
                pilotDialogService, fileProvider, searchService);
            _services.RegisterExtra(typeof(IObjectModifier), objectModifier);
            _services.RegisterExtra(typeof(ITransitionManager), transitionManager);
            _services.RegisterExtra(typeof(IPersonalSettings), personalSettings);

            // Внешний запуск: раннер стартовал клиент с PILOTTESTHOST_CLI=1
            CliRunController.StartIfRequested(_services);
        }

        public void Build(IMenuBuilder builder, MainViewContext context)
        {
            // Индекс не должен превышать текущее число пунктов меню:
            // жёсткий индекс (раньше было 100) роняет клиент ArgumentOutOfRangeException
            // в ArrayList.Insert при открытии главного меню.
            var item = builder.AddItem("PilotTestHost", builder.Count).WithHeader("PilotTestHost");
            item.WithSubmenu().AddItem(RunAllItem, 0).WithHeader("Run Tests");
        }

        public void OnMenuItemClick(string name, MainViewContext context)
        {
            if (name != RunAllItem)
                return;

            TestRunReport report;
            try
            {
                report = TestRunOrchestrator.Run(_services);
            }
            catch (Exception e)
            {
                MessageBox.Show("Запуск тестов прерван: " + e, "PilotTestHost");
                return;
            }

            string jsonPath;
            try
            {
                jsonPath = JsonReporter.Write(report);
            }
            catch (Exception e)
            {
                jsonPath = "(не удалось записать JSON: " + e.Message + ")";
            }

            SummaryWindow.Show(report, jsonPath);
        }

        // Зависимости хоста и тестовых сборок ищутся рядом с плагином и в Tests\
        private static Assembly ResolveFromPluginDirectory(object sender, ResolveEventArgs args)
        {
            try
            {
                var pluginDir = Path.GetDirectoryName(typeof(App).Assembly.Location);
                if (pluginDir == null)
                    return null;

                var name = new AssemblyName(args.Name).Name + ".dll";
                var candidates = new[]
                {
                    Path.Combine(pluginDir, name),
                    Path.Combine(pluginDir, TestAssemblyLoader.TestsFolderName, name)
                };

                var path = candidates.FirstOrDefault(File.Exists);
                return path != null ? Assembly.LoadFrom(path) : null;
            }
            catch
            {
                return null;
            }
        }
    }
}
