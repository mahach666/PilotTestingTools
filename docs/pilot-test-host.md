# PilotTestHost

Хост для тестов, выполняющихся **внутри клиента Pilot** (Pilot-ICE Enterprise / Pilot-BIM).
Аналог RevitTestHost: SDK-объекты Pilot живут только в процессе клиента, поэтому
тесты, трогающие `IObjectsRepository` и прочие сервисы, запускаются в самом клиенте.

## Состав

- **PilotTestHost.Sdk** - контракты для тестовых проектов: `[PilotFact]`, `ITestContext`, `IPilotTestLogger`.
- **PilotTestHost.Plugin** - плагин-хост (`PilotTestHost.ext2`): пункт меню «PilotTestHost -> Run Tests»,
  дискавери тестовых сборок, запуск, JSON-отчёт + окно-сводка.

## Как это работает

1. Плагин-хост деплоится в `%LocalAppData%\ASCON\<клиент>\Development\PilotTestHost\` (Debug-сборка делает это сама).
2. Тестовые сборки (`*Tests.dll`) кладутся в подпапку `Tests\` рядом с плагином - вместе со своими зависимостями
   (assert-библиотека, сборка тестируемого плагина).
3. В клиенте: меню **PilotTestHost -> Run Tests**. Результаты - окно-сводка + `%TEMP%\PilotTestHost\results_*.json`.

Тестовые dll грузятся через `Assembly.LoadFrom` в домен клиента: после пересборки тестов клиент нужно перезапустить.

## Как написать тест

```csharp
using PilotTestHost.Sdk;

public class MyPluginTests
{
    [PilotFact]
    public void Repository_Has_Types(ITestContext ctx)
    {
        var types = ctx.Repository.GetTypes().ToList();
        ctx.Logger.Info($"типов в базе: {types.Count}");
        if (types.Count == 0) throw new Exception("в базе нет типов");
    }

    [PilotFact(Skip = "пример пропуска")]
    public void Not_Ready_Yet() { }
}
```

Правила: публичный класс с публичным конструктором без параметров; публичный
экземплярный `void`-метод без параметров или с одним `ITestContext`. Async не поддерживается.
Assert-библиотека - любая (исключения с `Assert`/`Xunit`/`FluentAssertions` в имени типа
классифицируются как Failed, прочие - Error).

Тестовый проект ссылается на `PilotTestHost.Sdk` без копирования сборки в выходную папку -
обе стороны должны использовать один экземпляр SDK-сборки (её деплоит хост). Через пакет NuGet:

```xml
<PackageReference Include="PilotTestHost.Sdk" Version="0.2.0" ExcludeAssets="runtime" />
```

Через ссылку на проект (параметр `Private` работает только у `ProjectReference`):

```xml
<ProjectReference Include="<путь к PilotTestingTools>\src\PilotTestHost.Sdk\PilotTestHost.Sdk.csproj">
  <Private>false</Private>
</ProjectReference>
```

## Внешний запуск (CLI)

```
dotnet run --project src\PilotTestHost.Cli -- [--client <exe>] [--filter "Ns.Class.*"] [--output report.json] [--timeout-minutes 30]
```

CLI запускает клиент Pilot с переменными окружения `PILOTTESTHOST_CLI=1`,
`PILOTTESTHOST_OUTPUT`, `PILOTTESTHOST_FILTER`. Хост при старте видит CLI-режим,
дожидается готовности клиента (репозиторий отвечает - до 5 минут на автологин),
гоняет тесты, пишет отчёт и завершает процесс клиента.

Требование: у клиента должен быть **сохранён вход** (окно логина в headless-режиме - тупик,
прогон свалится по таймауту готовности с маркером ошибки в отчёте).

Коды возврата: `0` - все зелёные; `N` - количество failed+errored;
`101` - инфраструктура (клиент не найден, таймаут, нет отчёта); `102` - фатальная ошибка прогона.

По умолчанию ищутся `Ascon.Pilot.PilotEnterprise.exe` (ICE), затем `Ascon.Pilot.PilotBIM.exe`.

## Фазы

- [x] Фаза 1: UI-запуск, JSON-отчёт, окно-сводка.
- [x] Фаза 2: CLI (headless-запуск клиента, env-протокол, фильтр, коды возврата).
- [ ] Фаза 3: `[PilotTheory]`, фикстуры, TRX-отчёт.
