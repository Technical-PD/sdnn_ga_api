# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

SDNN-GA is a **Windows desktop app**, fully offline, that designs neural network architectures with a genetic algorithm.
One process (`SdnnGa.exe`) hosts an ASP.NET Core API on `127.0.0.1` and a Photino (WebView2) window showing the React UI.
Results live in SQLite, and files (datasets, model configs, weights) live in a local folder.
Models are built and trained by a local Python/TensorFlow child process.
Code comments and some log messages are in Ukrainian.

## Repository layout

- `SdnnGa.Api/` — .NET 8 solution (`SdnnGa.Api.sln`):
  - `SdnnGa.Api` — the desktop host (assembly `SdnnGa`). `Program.cs` builds the web app, runs migrations and starts Kestrel on loopback, then opens the Photino window (`[STAThread]` Main). `Desktop/` holds options, app paths, the file log tee and worker warm-up. Controllers live in `Controllers/V1` (incl. `SystemController` for worker status). The built React UI goes in `wwwroot/` (git-ignored, produced by `npm run build`).
  - `SdnnGa.Model` — interfaces, DTO/domain models, constants (`JobSettings`, `StoragePath`), `ServiceResult`. Interfaces live here, implementations elsewhere.
  - `SdnnGa.Services` — business services, AutoMapper `DtoProfile`
  - `SdnnGa.Core` — GA: Quartz jobs (`GeneticEpochJob`, `CreateModelJob`, `FitModelJob`), `ModelGenerator`, `NeuralNetworkMutation`, `BestModelSelector`, `ParentJobCompletionListener`
  - `SdnnGa.Database` — EF Core `ApiDbContext` (**SQLite**), `Db*` entities, `DbRepository<T>`, migrations, design-time factory
  - `SdnnGa.Infrastructure` — `Worker/PythonWorkerClient` (child-process RPC), `Storage/FileSystemStorage`, Quartz wrappers
- `Worker/sdnnga_worker.py` — the single Python worker (`create` → Keras JSON, `fit` → loss/accuracy/history/base64 weights). It is linked into the app output as `worker/sdnnga_worker.py`.
- `SdnnGa.Client/` — React 18 + Vite + Ant Design + Tailwind. It calls relative `/api/`, and the Vite dev server proxies to `127.0.0.1:5080`.
- `build-desktop.ps1` — produces the offline distribution (self-contained publish + embeddable Python with TF).
- `TestData/` — sample X/Y CSV datasets.

There are no automated tests.

## Commands

```
dotnet build SdnnGa.Api/SdnnGa.Api.sln
dotnet run --project SdnnGa.Api/SdnnGa.Api                  # desktop window
dotnet run --project SdnnGa.Api/SdnnGa.Api -- --headless    # API only on :5080 (Development) + /swagger
cd SdnnGa.Client && npm run dev | npm run build | npm run lint
dotnet ef migrations add <Name> --project SdnnGa.Api/SdnnGa.Database --startup-project SdnnGa.Api/SdnnGa.Api
.\build-desktop.ps1 -OutputDir C:\Builds\SdnnGa
```

The worker needs Python with `Worker/requirements.txt` (TensorFlow 2.20 → Python ≤ 3.13). Point to it with `Worker:PythonPath` / env `Worker__PythonPath`. Create venvs on a short path: TF install fails on paths over 260 chars when long paths are disabled.

## Runtime data

`%LOCALAPPDATA%\SdnnGa\` (override with `Desktop:DataDirectory`) holds `sdnnga.db` (WAL), `storage/` (keys from `StoragePath`, e.g. `v1/session-{id}/...`), `logs/` (all `Console` output is teed here; Release builds are WinExe with no console) and `webview/`.

## Worker protocol

JSON Lines over stdin/stdout. The worker sends `{"type":"ready"}` (or `"fatal"`) after importing TF. Requests are `{"id","type","payload"}` and responses are `{"id","ok","result"|"error"}`. Everything else the worker prints goes to stderr, which is logged as `[worker]`. `PythonWorkerClient` sends one request at a time: the timeout counts from send, and a crashed or hung worker is killed and restarted on the next call. Payload keys are the C# property names (`ModelConfig`, `FitModelJobConfig`) and must stay in sync with the Python dict keys. NaN/Inf are not valid JSON: the worker maps them to `null` (hence `History` uses `List<float?>`) and treats a non-finite final loss as an error.

## Genetic flow

1. The UI creates a session, uploads X/Y CSVs (uploaded sequentially, since each upload rewrites the whole session row), and adds a FitConfig and a GeneticConfig.
2. `GeneticService.StartGeneticFlow` validates the prerequisites and that the worker can start, then schedules a `GeneticEpochJob`.
3. `GeneticEpochJob` takes the best model of the previous epoch (`BestModelSelector`, which skips failed models) as the parent. It then creates the epoch and model rows and schedules a `CreateModelJob` and a `FitModelJob` (+10 s) per model in group `Epoche-{n}-{sessionId}`, plus a `ParentJobCompletionListener`.
4. `CreateModelJob` generates a random model (epoch 0) or mutates the parent, gets the Keras JSON from the worker and stores both configs.
5. `FitModelJob` **polls (no-tracking) until its model has a config**, because the worker queues requests. It then trains through the worker and stores the weights.
6. Any create/fit failure sets `NeuralNetworkModel.FitError`, and the model counts as finished (`IsFinished`). The listener waits until every model is finished, stops if none succeeded, and otherwise schedules the next epoch while `epochNo < MaxEpoches && bestAcc < StopAccValue && bestLoss > StopLossValue`.
7. Quartz is in-memory. At startup, models left unfinished by a previous run are marked as interrupted.

## Conventions

- Services return `ServiceResult` / `ServiceResult<T>` (`IsError`, `Message`, `Entity`) instead of throwing. Controllers return them with `200`, and the UI checks `isError`.
- Adding an entity touches `Db*` + `ApiDbContext` + a migration, a domain model, `DtoProfile`, the service interface and implementation, and DI in `Program.cs`.
- `FileSystemStorage` rejects keys that resolve outside the storage root (`Data/GetDataFile` takes a path from the client).
- Logging is via `Console.WriteLine`.
