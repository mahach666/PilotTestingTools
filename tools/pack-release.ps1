<#
.SYNOPSIS
    Собирает файлы релиза в папку artifacts: архив плагина-хоста, CLI, инспектор, пакеты NuGet и SHA256SUMS.txt.

.DESCRIPTION
    Версия берётся из Directory.Build.props. Файлы в Output перезаписываются, ничего не удаляется.

    Состав архива плагина: содержимое распаковывается в
    %LocalAppData%\ASCON\<клиент>\Development\PilotTestHost\ (PilotTestHost.ext2.dll и зависимости).
    Каталог со сборкой Release копируется как есть, без .pdb: так же, как это делает Debug-сборка плагина.
#>
param(
    [string]$Output = (Join-Path $PSScriptRoot '..\artifacts')
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$props = [xml](Get-Content -LiteralPath (Join-Path $root 'Directory.Build.props') -Raw)
$versionNode = $props.SelectSingleNode('/Project/PropertyGroup/Version')
$version = if ($versionNode) { $versionNode.InnerText } else { $null }
if (-not $version) { throw 'В Directory.Build.props не найдено свойство Version.' }

New-Item -ItemType Directory -Force -Path $Output | Out-Null
$Output = (Resolve-Path $Output).Path
$stage = Join-Path $Output 'stage'
if (Test-Path -LiteralPath $stage) { Remove-Item -LiteralPath $stage -Recurse -Force }
New-Item -ItemType Directory -Force -Path $stage | Out-Null

function Invoke-DotNet {
    & dotnet @args
    if ($LASTEXITCODE -ne 0) { throw "dotnet $($args -join ' ') завершился с кодом $LASTEXITCODE" }
}

function New-Zip([string]$SourceDir, [string]$ZipPath) {
    if (Test-Path -LiteralPath $ZipPath) { Remove-Item -LiteralPath $ZipPath }
    Compress-Archive -Path (Join-Path $SourceDir '*') -DestinationPath $ZipPath
}

# 1. Плагин-хост (Release-сборка не копирует файлы в папки клиента Pilot)
$pluginStage = Join-Path $stage 'plugin'
Invoke-DotNet build (Join-Path $root 'src\PilotTestHost.Plugin\PilotTestHost.Plugin.csproj') -c Release -o $pluginStage
Get-ChildItem -LiteralPath $pluginStage -Recurse -File | Where-Object { $_.Extension -eq '.pdb' -or $_.Name -like '*.deps.json' } | Remove-Item
New-Zip $pluginStage (Join-Path $Output "PilotTestHost-Plugin-$version.zip")

# 2. CLI (нужен .NET 8 Runtime)
$cliStage = Join-Path $stage 'cli'
Invoke-DotNet publish (Join-Path $root 'src\PilotTestHost.Cli\PilotTestHost.Cli.csproj') -c Release -o $cliStage
New-Zip $cliStage (Join-Path $Output "PilotTestHost-Cli-$version.zip")

# 3. Инспектор
$inspectorStage = Join-Path $stage 'inspector'
Invoke-DotNet publish (Join-Path $root 'tools\PilotUITesting.Inspector\PilotUITesting.Inspector.csproj') -c Release -o $inspectorStage
New-Zip $inspectorStage (Join-Path $Output "PilotUITesting-Inspector-$version.zip")

# 4. Пакеты NuGet
Invoke-DotNet pack (Join-Path $root 'src\PilotTestHost.Sdk\PilotTestHost.Sdk.csproj') -c Release -o $Output
Invoke-DotNet pack (Join-Path $root 'src\PilotUITesting\PilotUITesting.csproj') -c Release -o $Output

Remove-Item -LiteralPath $stage -Recurse -Force

# 5. Контрольные суммы
$files = @(Get-ChildItem -LiteralPath $Output -File | Where-Object { $_.Extension -in '.zip', '.nupkg', '.snupkg' } | Sort-Object Name)
$sums = $files | ForEach-Object { '{0}  {1}' -f (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant(), $_.Name }
Set-Content -LiteralPath (Join-Path $Output 'SHA256SUMS.txt') -Value $sums -Encoding Ascii
$sums
