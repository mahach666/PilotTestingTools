using System;
using System.Linq;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using NUnit.Framework;
using PilotUITesting.Client;
using PilotUITesting.Data;
using PilotUITesting.NUnit;
using PilotUITesting.Uia;

namespace PilotUITesting.Tests.Contract
{
    /// <summary>
    /// Сверка каталога PilotIds с живым клиентом (стенд из pilot-ui-tests.json). Запускать при обновлении клиента Pilot:
    /// dotnet test --filter "Category=PilotContract". Отчёты CatalogReport (найдено / нет / условные / неизвестные) - в выводе теста.
    /// </summary>
    [TestFixture]
    [Explicit("живой клиент Pilot и стенд из pilot-ui-tests.json")]
    [Category("PilotContract")]
    [NonParallelizable]
    public class CatalogContractTests : PilotUiTestBase
    {
        [Test]
        public void Главное_окно_и_вкладки()
        {
            Window main = App.MainWindow;
            CatalogAssert.AllPresent(main, typeof(PilotIds.MainWindow));
            CatalogAssert.AllPresent(main, typeof(PilotIds.Tabs));
            CatalogAssert.AllPresent(main, typeof(PilotIds.MainMenu));
        }

        [Test]
        public void Подменю_главного_меню_открывается_и_закрывается()
        {
            Assert.That(App.MainMenu.IsOpen, Is.False, "строка главного меню на экране, но ни один пункт не раскрыт");
            PilotMenu service = App.MainMenu.Submenu(PilotIds.MainMenu.Service);
            Assert.That(service.IsOpen, Is.True);
            Assert.That(App.MainMenu.IsOpen, Is.True);
            Assert.That(service.Has(PilotIds.MainMenu.Options), Is.True);
            App.MainMenu.Close();
            Assert.That(App.MainMenu.IsOpen, Is.False);
        }

        [Test]
        public void Проводник_на_документе_песочницы()
        {
            ObjectRef document = TestData.CreateDocument(TestData.Unique("Контракт"), filePath: PilotTestFiles.Pdf("contract"));
            PilotExplorer explorer = OpenExplorer(TabLifetime.Test);
            explorer.GoTo(document);

            CatalogAssert.AllPresent(explorer.Root, typeof(PilotIds.Explorer.Navigation));
            CatalogAssert.AllPresent(explorer.Root, typeof(PilotIds.Explorer.List));
            CatalogAssert.AllPresent(explorer.Root, typeof(PilotIds.Explorer.Tree));
            CatalogAssert.AllPresent(explorer.List.Row(document.Title), typeof(PilotIds.Explorer.Row));
            CatalogAssert.AllPresent(explorer.Toolbar.Root, typeof(PilotIds.Explorer.Toolbar));

            PilotMenu menu = explorer.ContextMenu();
            try
            {
                CatalogAssert.AllPresent(menu.Root, typeof(PilotIds.Explorer.ListMenu));
                Assert.That(menu.Items().Select(i => i.Id), Does.Contain(PilotIds.Explorer.ListMenu.ShowProperties));
            }
            finally
            {
                menu.Close();
            }
        }

        [Test]
        public void Кнопка_меню_в_переполнении_открывается_подменю_многоточия()
        {
            ObjectRef document = TestData.CreateDocument(TestData.Unique("Переполнение"));
            PilotExplorer explorer = OpenExplorer(TabLifetime.Test);
            explorer.GoTo(document);
            Window main = App.MainWindow;
            main.Patterns.Window.Pattern.SetWindowVisualState(WindowVisualState.Normal);
            try
            {
                // кнопки-меню панели проводника справа налево: в «…» первыми уходят правые. Окно сужается по шагам: при слишком узком
                // окне в переполнение уходит и заголовок вкладки, и страница проводника теряет свою вкладку
                string[] menuButtons = { PilotIds.Explorer.Toolbar.AttachToWorkflow, PilotIds.Explorer.Toolbar.AttachToTask, PilotIds.Explorer.Toolbar.Create };
                string hidden = null;
                foreach (int width in new[] { 1600, 1400, 1200, 1000, 900, 800, 700 })
                {
                    main.Patterns.Transform.Pattern.Resize(width, 1000);
                    if (Wait.TryUntil(() => (hidden = menuButtons.FirstOrDefault(id => explorer.Toolbar.Has(id) && explorer.Toolbar.IsHidden(id))) != null, TimeSpan.FromSeconds(3)))
                    {
                        TestContext.Out.WriteLine($"ширина окна {main.BoundingRectangle.Width}; скрыты: " +
                            string.Join(", ", explorer.Toolbar.Buttons().Where(b => b.IsHidden).Select(b => b.Id)));
                        break;
                    }
                }
                if (hidden == null)
                {
                    Assert.Inconclusive("Ни одна кнопка-меню проводника не ушла в «…» при видимом заголовке вкладки: проверить переполнение не удалось.");
                }
                Assert.Throws<InvalidOperationException>(() => explorer.Toolbar.Click(hidden), "кнопку-меню в «…» не нажать - только OpenMenu");
                PilotMenu menu = explorer.Toolbar.OpenMenu(hidden);
                try
                {
                    TestContext.Out.WriteLine($"подменю {hidden} в «…»: " + string.Join("; ", menu.Items()));
                    Assert.That(menu.Items(), Is.Not.Empty, $"подменю {hidden} в «…» содержит пункты");
                }
                finally
                {
                    menu.Close();
                }
            }
            finally
            {
                // узкое окно клиент запоминает при выходе, и в следующих прогонах заголовки вкладок уходят в переполнение
                if (!Native.Maximize(main.Properties.NativeWindowHandle.Value))
                {
                    PilotLog.Warn("Главное окно клиента не развернулось после проверки переполнения: клиент запомнит узкое окно.");
                }
            }
        }
    }
}
