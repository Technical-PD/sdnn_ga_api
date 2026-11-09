# SDNN-GA

A desktop application that designs neural network architectures with a genetic algorithm.
It runs entirely on your computer with no internet connection: a local database, local file storage
and a local TensorFlow worker.

## How it works

```
SdnnGa.exe ─┬─ native window (Photino + WebView2) ── React UI (wwwroot)
            ├─ ASP.NET Core API on 127.0.0.1 (random port, not reachable from the network)
            ├─ Quartz scheduler ── genetic algorithm (epochs → create/fit jobs)
            ├─ SQLite database        %LOCALAPPDATA%\SdnnGa\sdnnga.db
            ├─ file storage           %LOCALAPPDATA%\SdnnGa\storage\   (datasets, model configs, weights)
            └─ Python worker process  worker\sdnnga_worker.py (TensorFlow), JSON over stdin/stdout
```

Logs are written to `%LOCALAPPDATA%\SdnnGa\logs`.

## Using the app

1. Create a session.
2. Upload the X and Y CSV files. There are samples in `samples\` (from `TestData/`), for example Iris `X.csv` + `y_one_hot.csv`.
3. Add a fit config and a genetic config.
4. Start the flow, then browse epochs and models. You can download the Keras config, the .NET config and the weights (`.h5`) for each model.

A banner at the top of the window shows whether the TensorFlow worker is ready. Loading TensorFlow takes 10–30 s after the app starts.

## Building the offline distribution

Requirements on the build machine (internet is needed only while building): .NET 8 SDK, Node.js, and Python with pip.

```powershell
.\build-desktop.ps1 -OutputDir C:\Builds\SdnnGa      # add -Zip to also produce C:\Builds\SdnnGa.zip
```

The output folder (~1.5 GB) is self-contained: a self-contained .NET runtime, the built UI, and an embeddable Python
with TensorFlow, pandas and numpy. Copy it to any Windows 10/11 x64 PC and run `SdnnGa.exe`. Nothing has to be installed.
The WebView2 runtime ships with Windows 11 and current Windows 10.

Use a short output path outside OneDrive: TensorFlow contains very long file paths, and Windows limits paths to 260 characters
unless `LongPathsEnabled` is turned on.

## Development

```powershell
# 1. Python environment for the worker (a short path avoids the 260-char limit)
py -3.13 -m venv C:\venvs\sdnnga
C:\venvs\sdnnga\Scripts\pip install -r Worker\requirements.txt

# 2. Point the app at it, either in SdnnGa.Api/SdnnGa.Api/appsettings.Development.json ("Worker": { "PythonPath": ... })
#    or through an environment variable:
$env:Worker__PythonPath = "C:\venvs\sdnnga\Scripts\python.exe"

# 3a. Run the desktop window (UI from the last `npm run build`)
cd SdnnGa.Client; npm install; npm run build; cd ..
dotnet run --project SdnnGa.Api\SdnnGa.Api

# 3b. ...or develop the UI with hot reload: API only on http://127.0.0.1:5080 (+ /swagger), with Vite proxying /api
dotnet run --project SdnnGa.Api\SdnnGa.Api -- --headless
cd SdnnGa.Client; npm run dev
```

### Configuration (`appsettings.json`)

| Setting | Default | Meaning |
|---|---|---|
| `Desktop:DataDirectory` | `%LOCALAPPDATA%\SdnnGa` | Database, files and logs |
| `Desktop:Port` | `0` (random) | Local API port (`5080` in Development) |
| `Desktop:Headless` | `false` | API only, no window (also `--headless`) |
| `Worker:PythonPath` | bundled `python\python.exe`, then `python` on PATH | Python interpreter with TensorFlow |
| `Worker:FitModelTimeoutSeconds` | `3600` | Maximum training time per model |

### Database migrations

```powershell
dotnet ef migrations add <Name> --project SdnnGa.Api\SdnnGa.Database --startup-project SdnnGa.Api\SdnnGa.Api
```

Migrations are applied automatically when the app starts.
