# PilotUITesting

Библиотека для сквозных UI-тестов модулей Pilot-ICE Enterprise / Pilot-BIM: тесты запускают (или подхватывают) настоящий клиент Pilot,
прокликивают интерфейс через UI Automation (FlaUI) как пользователь и проверяют результат в базе через Ascon.Pilot.Server.Api.
Дополняет [PilotTestHost](pilot-test-host.md): тот выполняет тесты внутри процесса клиента на уровне SDK, а эта библиотека работает снаружи.

В библиотеке уже заготовлено то, что раньше каждый проект тестов писал сам:

* **каталог идентификаторов клиента Pilot** `PilotIds` (около 600 констант с источником: кнопки панелей и пункты меню проводника, заданий,
  просмотрщика, окна, вкладки, стандартные диалоги) и подписи `PilotTitles`;
* **готовые компоненты клиента**: вкладки, проводник, панели команд (`ToolbarStripe` Pilot и модулей), меню, списки, карточка атрибутов,
  справочник, сообщения и подтверждения, окна ошибок, диалог файла, таблица (`GridPattern`, проверено на DevExpress);
* **надёжное ядро**: ожидания без передачи таймаута в каждый вызов, различие «элемента нет» и «клиент занят», нажатие без повторов,
  мышь и клавиатура только в окно клиента, переход к объекту по `piloturi`;
* **данные и уборку**: песочница через Server API с окончательным удалением, ожидания изменений с приёмом отказов сервера, тестовые PDF,
  резервная копия локальных файлов, журналы модулей;
* **базовый класс NUnit** с журналом шагов и артефактами падения (экран, дерево окон, окна ошибок Pilot, отказы сервера, журналы модулей).

## Состав

| Проект | Назначение |
|--------|-----------|
| `src/PilotUITesting` | библиотека (net48; FlaUI.UIA3 5.0, Ascon.Pilot.Server.Api 26.1, NUnit 4.3) |
| `tests/PilotUITesting.Tests` | модульные тесты библиотеки (без клиента Pilot) и снимок публичного API |
| `tools/PilotUITesting.Inspector` | консольный инспектор: подробный дамп дерева UI Automation окон клиента, клики, ввод |

## Быстрый старт

`pilot-ui-tests.json` рядом с тестовой сборкой (копируется из проекта тестов):

```jsonc
{
  "client": "PilotIce",                         // PilotIce | PilotBim
  "serverUrl": "http://host:5545/database",     // сервер и база
  "clientUser": "bot2 (bot)",                   // под кем вошёл клиент: подпись кнопки профиля (сверяется)
  "dpiAware": true,                             // экран больше 100 % - без этого клики мышью мимо
  "resetUiBeforeEachTest": true,                // закрывать окна и меню, оставшиеся от упавшего теста
  "restartAfterModuleView": true, "restartOnAttach": true,   // см. «Ловушки клиента»
  "pluginLogs": [ "Vendor.MyModule" ],
  "sandbox": { "rootType": "support", "rootTitleAttribute": "support_name",
               "documentType": "support_document", "documentTitleAttribute": "help_document_name", "namePrefix": "UI" }
}
```

Учётку Server API держите вне git - в `pilot-ui-tests.local.json` (`apiLogin`, `apiPassword`) или в переменных окружения.

Тест модуля, который открывается пунктом контекстного меню проводника:

```csharp
public class MyModuleTests : PilotUiTestBase
{
    [Test]
    public void Модуль_по_документу_без_данных_пуст()
    {
        ObjectRef document = TestData.CreateDocument(TestData.Unique("Документ"), filePath: PilotTestFiles.Pdf("doc"));

        PilotExplorer explorer = OpenExplorer(TabLifetime.Test);   // своя вкладка «Проекты и документы»
        explorer.GoTo(document);                                    // piloturi: строка выделена и в фокусе
        PilotModuleView view = Track(PilotModuleView.OpenedBy(App, "MyModuleView",
            () => explorer.ContextMenu().Click("MyModuleContextMenuItem")));

        Assert.That(view.WaitText(t => t.StartsWith("Всего - ")), Does.StartWith("Всего - 0"));
    }
}
```

Модуль с плиткой на странице «Новая вкладка»: `App.TabBar.OpenTile("Мой модуль")` (или прежний `ModuleButtonTitle` + `OpenModuleTab()`).

## Понятия

| Что | Где | Зачем |
|-----|-----|-------|
| `PilotApp` | `App` в базовом классе | клиент: `Windows`, `TabBar`, `MainMenu`, `State` (подключение, пользователь, последняя команда), `ShowObject(id)`, `Quit()`, `Restart()` |
| `PilotPage` | база page object | всё ищется внутри своего `Root`; `Find/TryFind/Has/AssertAbsent`, `Click/ClickText/MouseClick/RightClick`, `SetValue`, `WaitText`, фабрики `ToolbarById/ToolbarWith/Card/CardIn/ListById/GridById` |
| `PilotTabPage`, `PilotExplorer` | обычная вкладка, проводник | `GoTo(объект)`, `GoToPath(...)`, `OpenRow`, `ContextMenu()`, `CreateObject(тип)`, `OpenProperties()`, `DeleteSelected()` |
| `PilotModuleView` | вкладка модуля | представление модуля - прямой потомок главного окна (так клиент показывает вкладки `ITabServiceProvider`); закрывается Ctrl+W |
| `PilotDialog` | окно | окно верхнего уровня или вложенное; закрытие проверяется по HWND; `ClickAndWaitClosed`, `ClickTextAndWaitClosed` |
| `PilotToolbar` | панель команд | кнопка по id = имя из построителя (и у модулей), `IsEnabled/IsChecked/IsHidden` из ItemStatus, меню кнопки `<id>ContextMenu`, скрытые кнопки через «…» |
| `PilotMenu` | меню | контекстное (клавиша «Меню» или правый клик), кнопки панели, главное; `Click("id", "id подпункта")`, `ClickText(...)`, `Items()` |
| `PilotCard` | карточка атрибутов | в любом окне и области: `Set`, `Choose`, `SetDate`, `PickReference`, `AddOrgUnit`, `ObligatoryAttributes` - с проверкой введённого; в справочнике уже отмеченная строка не нажимается повторно (клик снял бы отметку), выбор без изменений закрывается «Отменой» (`PilotReferenceBook.Submit`) |
| `UiGrid` | таблица | `RowCount`, `Columns()`, `Cell(строка, колонка)`, `FindRow`, `SelectRow` (FieldName или подпись колонки) |
| `PilotMessageBox`, `PilotErrorWindow`, `WindowsFileDialog` | стандартные окна | `Answer(Yes/No/Cancel/Ok)` по фактическому набору кнопок; текст ошибки; открыть файл |
| `By`, `Wait`, `UiActions` | ядро | селекторы; ожидания `Wait.Until(() => …)` без таймаута (описание - из текста условия), `Wait.Stays`; действия над найденным элементом для page object'ов модулей |
| `PilotServerClient` | `Server` | прежнее + `…AndWait` (подтверждение чтением и мгновенный отказ, если сервер отклонил изменение), `DeletePermanently`, `WaitDescendant`, `TaskAttachments`, `Errors` |
| `PilotSandbox` | `TestData` | корень «{префикс} {класс} {время}» в корне базы, `CreateFolder/CreateDocument/Create`, `Unique(...)`, `Track(id)`; после класса удаляет окончательно и проверяет: поддерево корня, объекты из `Track` с их поддеревьями и объекты, которые тест удалил из песочницы в корзину (находятся по наборам изменений прогона и атрибуту `Delete_source`). Объект старше песочницы и его предки не удаляются (сервер удалил бы его вместе с родителем) - это отчёт об ошибке уборки. Удалённое в корзину через интерфейс под другим пользователем уборка видит, только если учётка Server API - администратор (иначе - предупреждение) |

Базовый класс `PilotUiTestBase` дополнительно даёт `Step(...)` (журнал шагов), `RequireTypes(...)`, `OpenExplorer(...)`, `Track(...)`
(вкладка или представление модуля закроется после теста/класса), `RestoreFileAfterTests(путь)`, `AssertNoPilotErrors()`,
`AssertNoPluginErrors()`, `NewOutputPath(имя)`.

## Каталог идентификаторов

`PilotIds.<Раздел>.<Контейнер>.<Элемент>` - точный AutomationId: `PilotIds.Explorer.Toolbar.Delete`, `PilotIds.Explorer.ListMenu.AttachToSimpleTask`,
`PilotIds.Tasks.Toolbar.CreateTask`, `PilotIds.Viewer.Annotations.Menu.Container`, `PilotIds.Card.Container`, `PilotIds.MessageBox.Yes`.
Составные id - методы: `PilotIds.Explorer.CreateItem("support_document")` -> `miCreatesupportdocument`, `PilotIds.Card.Field("name")` -> `AUIDname`,
`PilotIds.ToolbarStripe.MenuOf("Create")` -> `CreateContextMenu`. Подписи (UIA Name) - `PilotTitles`.

* Правило, на котором стоит каталог (проверено по коду клиента 26.11 и на живом клиенте): **AutomationId пункта меню и кнопки панели - это
  имя, переданное в построитель, без преобразований** - и для встроенных команд, и для команд модулей SDK (`IMenuBuilder.AddItem(name)`,
  `IToolbarBuilder.AddButtonItem(name)`). Поэтому свои пункты модуль описывает так же - строкой имени.
* У каждой константы - «подпись», тип элемента, уровень доказательства: **[Р]** проверено на живом клиенте, **[Д]** встречалось в реальных
  дампах прогонов, **[К]** только код клиента (ссылка «Файл.cs:строка» - декомпиляция клиента 26.11). `[ShownWhen]` - элемент есть не всегда.
* Одна команда может иметь разные id на панели и в меню (`Toolbar.AttachToTask` = `miAttachToTask`, `ListMenu.AttachToSimpleTask` =
  `miAttachToSimpleTask`). Не всё в UIA имеет Name: у команд с подписью из `Command.Caption` Name пустой - ищите по id.
* При обновлении клиента: `CatalogAssert.Check(область, typeof(PilotIds.Explorer.Toolbar))` покажет, каких id нет и какие новые появились;
  `CatalogAssert.AllPresent(...)` - то же с провалом теста (годится и для своих id модуля).

## Конфигурация

Источник (по убыванию приоритета): переменные `PILOT_UI_*` -> `pilot-ui-tests.local.json` -> `pilot-ui-tests.json` -> значения по умолчанию.
Неизвестные ключи не ломают загрузку, но выводятся предупреждением (`Config.UnknownKeys`).

| Ключ | По умолчанию | Переменная | Смысл |
|------|--------------|------------|-------|
| `client`, `exePath`, `serverUrl`, `login`, `password`, `apiLogin`, `apiPassword`, `keepClientOpen`, `attachToRunning`, `artifactsDir`, `defaultTimeoutSeconds`, `clientStartTimeoutSeconds` | как в 0.1 | `PILOT_UI_CLIENT`, `PILOT_UI_EXE`, `PILOT_UI_SERVER_URL`, `PILOT_UI_LOGIN`, `PILOT_UI_PASSWORD`, `PILOT_UI_API_LOGIN`, `PILOT_UI_API_PASSWORD`, `PILOT_UI_KEEP_CLIENT`, `PILOT_UI_ATTACH`, `PILOT_UI_ARTIFACTS` | прежние ключи |
| `dataTimeoutSeconds` | 90 | `PILOT_UI_DATA_TIMEOUT` | данные сервера в интерфейсе, загрузка модулей |
| `clientUser` | - | `PILOT_UI_CLIENT_USER` | под кем должен быть клиент: подпись кнопки профиля «отображаемое имя (должность)», сверяется точно; расхождение - отказ |
| `dpiAware` | false | `PILOT_UI_DPI_AWARE` | Per-Monitor V2 до создания UI Automation |
| `resetUiBeforeEachTest` | false | `PILOT_UI_RESET_UI` | уборка окон и меню перед тестом и после него |
| `unsentChanges` | `Fail` | `PILOT_UI_UNSENT_CHANGES` | окно «Отправка изменений»: остановиться или `Discard` (стирает ВСЕ неотправленные изменения) |
| `restartAfterModuleView` | true | - | перезапуск клиента перед следующим тестом и в конце класса, если открывалось представление модуля (только новое API) |
| `restartOnAttach` | false | `PILOT_UI_RESTART_ON_ATTACH` | перезапустить уже запущенный клиент при подключении |
| `pluginLogs` | [] | - | журналы модулей (`Units Logs`) для `AssertNoPluginErrors` и артефактов |
| `artifactsKeep` | 50 | - | сколько папок артефактов хранить; удаляются только папки вида `yyyyMMdd_HHmmss_fff_*`, сбой удаления - предупреждение в журнале |
| `sandbox` | - | - | типы песочницы: `rootType`, `rootTitleAttribute`, `folderType`, `folderTitleAttribute`, `documentType`, `documentTitleAttribute`, `namePrefix`, `cleanup` (`DeletePermanently` / `RecycleBin` / `Keep`) |

Новые ключи по умолчанию не меняют поведение прежних проектов.

## Ловушки клиента Pilot (проверено на Pilot-ICE Enterprise 26.11)

* **DPI.** На экране больше 100 % без `dpiAware` клики мышью попадают мимо. С ним работают клик, двойной и правый клик
  (правый клик по строке проводника и ячейке таблицы открывает контекстное меню).
* **Вкладка модуля** (`ITabServiceProvider.OpenTabPage`) - не под `TabView`, а прямой потомок главного окна с классом представления;
  заголовка вкладки в UIA нет, `DocumentCloseButton` недоступна - `PilotModuleView`, закрытие Ctrl+W.
* **После открытия вкладки модуля дерево UI Automation главного окна перестаёт обновляться** до перезапуска клиента: новые вкладки,
  строки проводника, повторно открытые меню кнопок, редактор ячейки таблицы видны на экране, но не в дереве. Первое открытие каждого
  всплывающего меню или списка в сеансе видно. Отсюда `restartAfterModuleView` / `restartOnAttach` и сообщения об ошибках с подсказкой.
* **Окна**: диалоги DialogService и модулей - дочерние окна главного (не верхнего уровня); у окон модулей AutomationId пуст, Name = заголовок;
  карточка объекта, окна задания и процесса - верхнего уровня (у задания и процесса Name = `AUIDNextTasksEdit`/`AUIDWorkflowEditView`,
  а id = заголовок). Всплывающие меню и списки - окна класса `Popup`. `App.Windows` ищет везде.
* **Состояние клиента** читается из HelpText/ItemStatus: `tabHostView` - `ConnectionState:Online`, `IsLoadingTabs:False`, ItemType -
  последняя команда; заголовок вкладки - `IsActive:True; Content:<тип>`; кнопки панелей - `IsHidden`/`IsChecked`. Текст «ПОДКЛЮЧЕНО»
  в шапке есть только в развёрнутом окне.
* **Переход к объекту**: `exe клиента "piloturi://<id>"` (не через оболочку - протокол может быть зарегистрирован на другой клиент);
  строка объекта в проводнике выделяется и получает фокус. `-quit` штатно закрывает клиент.
* **Новая вкладка проводника** открывается в последнем месте, а не в «Начале»; у вкладок, восстановленных при запуске клиента, нет кнопок
  панелей от модулей - тесту нужна своя вкладка (`OpenExplorer`).
* **Переполнение панели** (узкое окно): кнопки уходят в «…» (`PART_MoreButton` -> `moreButtonMenu`, id пунктов те же). Обычная кнопка
  нажимается пунктом «…» (`Click`), кнопка-меню там - заголовок подменю: её пункты открывает `OpenMenu`/`ClickMenuItem`, а `Click`
  бросает исключение с подсказкой (проверено контрактным тестом на «Вложить в процесс» при ширине окна 1600). В ещё более узком окне
  в переполнение уходят и заголовки вкладок - страницы вкладок их не находят.
* **`fallIntoButton`** реагирует только на мышь (Invoke не работает); переход внутрь - Enter или двойной клик.
* **`obligatoryTxb`** - знак обязательности атрибута, а не «поле пустое» (виден и после заполнения).
* **SilentInvoker**: чтение `SelectionPattern.Selection` у служебного списка внутри кнопки панели **выполняет команду кнопки**.
  Дампы библиотеки и инспектора `Selection` не читают.
* Окно «Отправка изменений» и отказы сервера: изменения Server API подтверждаются чтением, отказ (`NotifyChangeAsyncError` с id изменения)
  сразу даёт `ServerChangeRejectedException` с текстом сервера (например, «нет права на переход состояния»).

## Совместимость

Публичный API прежних версий сохранён: `tests/PilotUITesting.Tests/Unit/PublicApiTests` сверяет его со снимком `PublicApi.approved.txt`
(из снимка ничего не должно пропадать; обновить снимок - `PILOT_UI_UPDATE_API=1`). Существующие UI-тесты модулей собираются без изменений.
Исправлено в старых членах: гонка и тихие ошибки в `CloseDialogs`/`CloseClientErrorWindows`, ложный успех `CloseTab`, зависание ожидания
подключения (признак «ПОДКЛЮЧЕНО» -> `ConnectionState:Online`), выбор процесса из чужого сеанса Windows, `Wait.Until` бросает
`PilotWaitTimeoutException` (наследник `TimeoutException`) и проверяет условие после дедлайна, `IsUiBusy` не считает собственный таймаут
занятостью клиента, `OnAfterTests` не вызывается, если подготовка класса упала. `ModuleButtonTitle` стал необязательным.
`PickReference`/`PickReferenceByPath` из 0.1 больше не снимают отметку с уже отмеченной строки справочника элементов (клик по флажку
очищал атрибут): такая строка не нажимается, окно закрывается OK, если он доступен, иначе «Отменой».

## Тесты библиотеки

```
dotnet test tests\PilotUITesting.Tests\PilotUITesting.Tests.csproj
```

Проверяют без клиента: разбор состояний Pilot, классификацию сбоев UI Automation, ожидания, обход дерева при сбоях «занят»/«исчез»
(поддельные узлы FlaUI - `Unit/FakeNode.cs`), выбор кнопок сообщений, конфигурацию, наличие объекта на сервере, сверку дат в UTC,
журналы модулей, уборку артефактов, резервные копии файлов, снимок API.

## Инспектор

```
PilotUITesting.Inspector dump  [процесс=Ascon.Pilot.PilotBIM] [папка] [глубина]
PilotUITesting.Inspector start "C:\Program Files\ASCON\Pilot-BIM\Ascon.Pilot.PilotBIM.exe" [папка]
PilotUITesting.Inspector click Ascon.Pilot.PilotEnterprise "id=AddTabButton|name=Кнопка|text=часть" [папка] [ждать мс]
PilotUITesting.Inspector row   Ascon.Pilot.PilotEnterprise "текст строки" [папка]
PilotUITesting.Inspector type  Ascon.Pilot.PilotEnterprise "id=AUIDname" "текст" [папка]
PilotUITesting.Inspector keys  Ascon.Pilot.PilotEnterprise "{ESC}" [папка]
PilotUITesting.Inspector cancel Ascon.Pilot.PilotEnterprise [папка] [id1,id2]
```

Инспектор включает осведомлённость о DPI, дамп подробный (класс, прямоугольник, HelpText, ItemStatus, ItemType, значение, паттерны).

## Что не проверено

* Вертикальная виртуализация строк таблицы DevExpress (проверено на небольшом числе строк), выделение нескольких строк проводника,
  нажатие скрытой при переполнении кнопки через «…» вживую.
* Форматы HelpText/ItemStatus в Pilot-BIM (код клиента общий с ICE, вживую проверен ICE). Путь журналов модулей для BIM.
* Причина устаревания дерева UI Automation после вкладки модуля (обход - перезапуск клиента).
