<#
.SYNOPSIS
    Збирає повністю автономну (offline) десктопну версію SDNN-GA для Windows x64.

.DESCRIPTION
    Результат — тека, яку можна скопіювати на будь-який Windows 10/11 x64 і запускати без інтернету
    та без встановлених .NET / Python / Node:

        <OutputDir>\
            SdnnGa.exe          — застосунок (self-contained .NET 8 + Photino/WebView2)
            wwwroot\            — зібраний React-клієнт
            worker\             — Python-воркер (TensorFlow)
            python\             — вбудований Python (embeddable) з TensorFlow, pandas, numpy
            samples\            — приклади датасетів (TestData)

    Інтернет потрібен лише під час збирання (npm, NuGet, python.org, PyPI).
    Дані користувача зберігаються в %LOCALAPPDATA%\SdnnGa (база SQLite, файли, логи).

.PARAMETER OutputDir
    Куди покласти збірку. Краще короткий шлях поза OneDrive (TensorFlow містить дуже довгі шляхи файлів,
    а тека займає ~1.5 ГБ).

.PARAMETER PythonVersion
    Версія вбудованого Python (embeddable package з python.org). TensorFlow 2.20 підтримує 3.9–3.13.

.PARAMETER HostPython
    Python з pip, яким завантажуються пакети для вбудованого Python (його версія може відрізнятися).

.PARAMETER SkipPython
    Не вбудовувати Python (застосунок використає Worker:PythonPath або `python` з PATH).

.EXAMPLE
    .\build-desktop.ps1 -OutputDir C:\Builds\SdnnGa
#>
[CmdletBinding()]
param(
    [string]$OutputDir = (Join-Path $PSScriptRoot "dist\SdnnGa"),
    [string]$PythonVersion = "3.13.15",
    [string]$HostPython = "python",
    [string]$Runtime = "win-x64",
    [switch]$SkipPython,
    [switch]$Zip
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
$OutputDir = [IO.Path]::GetFullPath($OutputDir)

function Step($text) { Write-Host "`n==> $text" -ForegroundColor Cyan }

function Invoke-Native {
    param([string]$File, [string[]]$Arguments)
    # У Windows PowerShell 5.1 будь-який рядок у stderr (навіть npm/pip warning) при "Stop" стає фатальною помилкою,
    # тому успіх визначаємо лише за кодом виходу.
    $previousPreference = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try { & $File @Arguments }
    finally { $ErrorActionPreference = $previousPreference }
    if ($LASTEXITCODE -ne 0) { throw "'$File $($Arguments -join ' ')' failed with exit code $LASTEXITCODE" }
}

# TensorFlow містить файли зі шляхами ~150 символів; без LongPathsEnabled ліміт Windows — 260.
$longPathsEnabled = (Get-ItemProperty "HKLM:\SYSTEM\CurrentControlSet\Control\FileSystem" -Name LongPathsEnabled -ErrorAction SilentlyContinue).LongPathsEnabled -eq 1
if (-not $SkipPython -and -not $longPathsEnabled -and $OutputDir.Length -gt 90) {
    throw "OutputDir '$OutputDir' is too long ($($OutputDir.Length) chars) for TensorFlow while Windows long paths are disabled. " +
          "Use a shorter path (e.g. -OutputDir C:\Builds\SdnnGa) or enable LongPathsEnabled."
}

if (Test-Path $OutputDir) {
    Step "Cleaning $OutputDir"
    Remove-Item -Recurse -Force $OutputDir
}
New-Item -ItemType Directory -Force $OutputDir | Out-Null

# 1. React-клієнт -> SdnnGa.Api/SdnnGa.Api/wwwroot
Step "Building client (SdnnGa.Client)"
Push-Location (Join-Path $root "SdnnGa.Client")
try {
    if (Test-Path "package-lock.json") { Invoke-Native "npm" @("ci", "--no-audit", "--no-fund") }
    else { Invoke-Native "npm" @("install", "--no-audit", "--no-fund") }
    Invoke-Native "npm" @("run", "build")
}
finally { Pop-Location }

# 2. .NET застосунок (self-contained — на цільовому ПК .NET не потрібен)
Step "Publishing desktop app ($Runtime, self-contained)"
Invoke-Native "dotnet" @(
    "publish", (Join-Path $root "SdnnGa.Api\SdnnGa.Api\SdnnGa.Api.csproj"),
    "-c", "Release", "-r", $Runtime, "--self-contained", "true",
    "-p:DebugType=None", "-o", $OutputDir, "--nologo")

# 3. Вбудований Python з TensorFlow
if (-not $SkipPython) {
    $pythonDir = Join-Path $OutputDir "python"
    $cacheDir = Join-Path $root ".build-cache"
    New-Item -ItemType Directory -Force $cacheDir | Out-Null

    $embedZip = Join-Path $cacheDir "python-$PythonVersion-embed-amd64.zip"
    if (-not (Test-Path $embedZip)) {
        Step "Downloading Python $PythonVersion (embeddable)"
        Invoke-WebRequest "https://www.python.org/ftp/python/$PythonVersion/python-$PythonVersion-embed-amd64.zip" -OutFile $embedZip
    }

    Step "Extracting embeddable Python"
    Expand-Archive $embedZip -DestinationPath $pythonDir -Force

    # Embeddable Python за замовчуванням ігнорує site-packages — вмикаємо.
    $pth = Get-ChildItem $pythonDir -Filter "python*._pth" | Select-Object -First 1
    $pthLines = (Get-Content $pth.FullName) -replace '^#\s*import site', 'import site'
    if ($pthLines -notcontains "Lib\site-packages") { $pthLines = @("Lib\site-packages") + $pthLines }
    Set-Content -Path $pth.FullName -Value $pthLines -Encoding ascii

    $sitePackages = Join-Path $pythonDir "Lib\site-packages"
    $shortVersion = ($PythonVersion -split '\.')[0..1] -join ''   # 3.13.15 -> 313

    Step "Installing worker requirements into embedded Python (this downloads TensorFlow, ~350 MB)"
    Invoke-Native $HostPython @(
        "-m", "pip", "install", "--disable-pip-version-check", "--no-warn-script-location",
        "--target", $sitePackages,
        "--platform", "win_amd64", "--python-version", $shortVersion, "--implementation", "cp", "--only-binary=:all:",
        "-r", (Join-Path $root "Worker\requirements.txt"))

    # Заголовки C++ потрібні лише для збирання власних операцій TF — у рантаймі не використовуються.
    $tfInclude = Join-Path $sitePackages "tensorflow\include"
    if (Test-Path $tfInclude) { Remove-Item -Recurse -Force $tfInclude }

    Step "Checking embedded Python can import TensorFlow"
    $env:PYTHONNOUSERSITE = "1"
    Invoke-Native (Join-Path $pythonDir "python.exe") @("-c", "import tensorflow as tf, pandas, numpy; print('TensorFlow', tf.__version__)")
    Remove-Item Env:PYTHONNOUSERSITE
}

# 4. Приклади датасетів
Step "Copying sample datasets"
Copy-Item -Recurse (Join-Path $root "TestData") (Join-Path $OutputDir "samples")

if ($Zip) {
    $zipPath = "$OutputDir.zip"
    Step "Creating $zipPath"
    if (Test-Path $zipPath) { Remove-Item $zipPath }
    Compress-Archive -Path (Join-Path $OutputDir "*") -DestinationPath $zipPath
}

$sizeMb = [math]::Round(((Get-ChildItem $OutputDir -Recurse -File | Measure-Object Length -Sum).Sum / 1MB))
Write-Host "`nDone: $OutputDir ($sizeMb MB). Run SdnnGa.exe" -ForegroundColor Green
