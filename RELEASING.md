# Выпуск версии

1. Поднимите `Version` в [Directory.Build.props](Directory.Build.props). Версия общая для всех проектов.
2. Перенесите записи из раздела «Не выпущено» в [CHANGELOG.md](CHANGELOG.md) под заголовок новой версии с датой и поправьте ссылки внизу файла.
3. Проверьте локально:

   ```powershell
   dotnet build PilotTestingTools.sln -c Release
   dotnet test PilotTestingTools.sln -c Release --no-build
   powershell -ExecutionPolicy Bypass -File tools/pack-release.ps1
   ```

   Файлы релиза появятся в `artifacts/`: архив плагина-хоста, CLI, инспектор, два пакета `.nupkg` с символами `.snupkg` и `SHA256SUMS.txt`.
4. Закоммитьте изменения, поставьте тег и отправьте его:

   ```powershell
   git tag -a v0.2.0 -m "v0.2.0"
   git push origin main v0.2.0
   ```

5. Workflow [release.yml](.github/workflows/release.yml) по тегу `v*` проверит, что версия тега совпадает с `Version`, соберёт и проверит решение, подготовит те же файлы и создаст на GitHub черновик релиза с пометкой pre-release. Описание берётся из раздела версии в `CHANGELOG.md`. Проверьте черновик и опубликуйте его.
6. Пакеты в nuget.org отправляются вручную (нужен ключ API):

   ```powershell
   dotnet nuget push artifacts/PilotTestHost.Sdk.0.2.0.nupkg --source https://api.nuget.org/v3/index.json --api-key <ключ>
   dotnet nuget push artifacts/PilotUITesting.0.2.0.nupkg --source https://api.nuget.org/v3/index.json --api-key <ключ>
   ```

   Файлы `.snupkg` с символами лежат рядом, `dotnet nuget push` отправляет их вместе с пакетом.

Публикация пакета в nuget.org необратима: версию можно только скрыть, но не переиспользовать. Перед выпуском на новую версию клиента Pilot проверьте, что плагин-хост в ней загружается, и повторите контрактные тесты каталога (см. [docs/pilot-ui-testing.md](docs/pilot-ui-testing.md)).
