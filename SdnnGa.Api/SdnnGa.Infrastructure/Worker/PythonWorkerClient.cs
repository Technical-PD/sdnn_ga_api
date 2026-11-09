using SdnnGa.Model.Infrastructure.Interfaces.Worker;
using SdnnGa.Model.Models.Core.NNModel;
using SdnnGa.Model.Models.Core.Worker;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SdnnGa.Infrastructure.Worker;

/// <summary>
/// Запускає Worker/sdnnga_worker.py як дочірній процес і спілкується з ним JSON-рядками через stdin/stdout.
/// Воркер обробляє запити послідовно, тому одночасно відправляється лише один запит,
/// а таймаут рахується з моменту відправки (а не з моменту постановки в чергу).
/// Якщо процес впав або завис — він перезапускається при наступному запиті.
/// </summary>
public sealed class PythonWorkerClient : IModelWorkerClient, IDisposable
{
    private const int StderrTailSize = 30;
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    private readonly WorkerOptions _options;
    private readonly SemaphoreSlim _requestLock = new(1, 1);
    private readonly SemaphoreSlim _startLock = new(1, 1);
    private readonly object _sync = new();
    private readonly Queue<string> _stderrTail = new();

    private Process _process;
    private TaskCompletionSource _readySignal;
    private PendingRequest _inflight;
    private volatile WorkerStatus _status = new(WorkerState.NotStarted, "Worker has not been started yet.");
    private volatile bool _disposed;

    public PythonWorkerClient(WorkerOptions options)
    {
        _options = options ?? new WorkerOptions();
    }

    public WorkerStatus Status => _status;

    public async Task EnsureStartedAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        await _startLock.WaitAsync(cancellationToken);
        try
        {
            if (IsAlive(_process) && _status.State == WorkerState.Ready)
            {
                return;
            }

            if (!IsAlive(_process))
            {
                StartProcess();
            }

            try
            {
                await _readySignal.Task.WaitAsync(TimeSpan.FromSeconds(_options.StartupTimeoutSeconds), cancellationToken);
            }
            catch (TimeoutException)
            {
                var message = $"Worker did not become ready within {_options.StartupTimeoutSeconds}s. {StderrTail()}";
                SetStatus(new WorkerStatus(WorkerState.Failed, message));
                KillProcess(_process);
                throw new WorkerException(message);
            }
        }
        finally
        {
            _startLock.Release();
        }
    }

    public async Task<string> CreateModelAsync(ModelConfig modelConfig, CancellationToken cancellationToken = default)
    {
        var result = await SendAsync("create", modelConfig, _options.CreateModelTimeoutSeconds, cancellationToken);
        return result.GetString();
    }

    public async Task<FitResult> FitModelAsync(FitModelJobConfig fitConfig, CancellationToken cancellationToken = default)
    {
        var result = await SendAsync("fit", fitConfig, _options.FitModelTimeoutSeconds, cancellationToken);
        return result.Deserialize<FitResult>();
    }

    public void Dispose()
    {
        _disposed = true;
        KillProcess(_process);
        FailInflight("Application is shutting down.");
    }

    private async Task<JsonElement> SendAsync<TPayload>(string type, TPayload payload, int timeoutSeconds, CancellationToken cancellationToken)
    {
        await _requestLock.WaitAsync(cancellationToken);
        try
        {
            await EnsureStartedAsync(cancellationToken);

            var process = _process;
            var pending = new PendingRequest(Guid.NewGuid().ToString("N"));
            lock (_sync)
            {
                _inflight = pending;
            }

            var line = JsonSerializer.Serialize(new { id = pending.Id, type, payload });

            try
            {
                await process.StandardInput.WriteLineAsync(line);
                await process.StandardInput.FlushAsync();
            }
            catch (Exception ex) when (ex is IOException or InvalidOperationException)
            {
                ClearInflight(pending);
                throw new WorkerException($"Failed to send '{type}' request to the worker: {ex.Message}", ex);
            }

            try
            {
                return await pending.Completion.Task.WaitAsync(TimeSpan.FromSeconds(timeoutSeconds), cancellationToken);
            }
            catch (TimeoutException)
            {
                ClearInflight(pending);
                var message = $"Worker did not finish '{type}' within {timeoutSeconds}s; it will be restarted.";
                SetStatus(new WorkerStatus(WorkerState.Failed, message));
                KillProcess(process);
                throw new WorkerException(message);
            }
            catch (OperationCanceledException)
            {
                // Воркер усе ще виконує скасований запит — зупиняємо його, щоб відповідь не прийшла наступному запиту.
                ClearInflight(pending);
                KillProcess(process);
                throw;
            }
        }
        finally
        {
            _requestLock.Release();
        }
    }

    private void StartProcess()
    {
        var (pythonPath, scriptPath, isBundled) = ResolveCommand();

        _readySignal = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var ready = _readySignal;

        if (!File.Exists(scriptPath))
        {
            var message = $"Worker script not found at '{scriptPath}'.";
            SetStatus(new WorkerStatus(WorkerState.Failed, message));
            throw new WorkerException(message);
        }

        var startInfo = new ProcessStartInfo(pythonPath)
        {
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardInputEncoding = Utf8NoBom,
            StandardOutputEncoding = Utf8NoBom,
            StandardErrorEncoding = Utf8NoBom,
            WorkingDirectory = Path.GetDirectoryName(scriptPath),
        };
        startInfo.ArgumentList.Add("-u");
        startInfo.ArgumentList.Add(scriptPath);
        startInfo.Environment["PYTHONIOENCODING"] = "utf-8";
        startInfo.Environment["PYTHONUNBUFFERED"] = "1";
        startInfo.Environment["TF_CPP_MIN_LOG_LEVEL"] = "2";
        if (isBundled)
        {
            // Вбудований Python не повинен підхоплювати пакети з профілю користувача.
            startInfo.Environment["PYTHONNOUSERSITE"] = "1";
        }

        var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data == null)
            {
                return;
            }

            Console.WriteLine($"[worker] {e.Data}");
            lock (_stderrTail)
            {
                _stderrTail.Enqueue(e.Data);
                while (_stderrTail.Count > StderrTailSize)
                {
                    _stderrTail.Dequeue();
                }
            }
        };

        lock (_stderrTail)
        {
            _stderrTail.Clear();
        }

        SetStatus(new WorkerStatus(WorkerState.Starting, $"Starting worker: {pythonPath} {scriptPath}"));
        Console.WriteLine($"[worker] Starting: \"{pythonPath}\" -u \"{scriptPath}\"");

        try
        {
            process.Start();
        }
        catch (Win32Exception ex)
        {
            var message = $"Could not start Python ('{pythonPath}'): {ex.Message}. " +
                "Install Python with the packages from Worker/requirements.txt or set 'Worker:PythonPath' in appsettings.json.";
            SetStatus(new WorkerStatus(WorkerState.Failed, message));
            ready.TrySetException(new WorkerException(message, ex));
            throw new WorkerException(message, ex);
        }

        process.BeginErrorReadLine();
        _process = process;

        _ = Task.Run(() => ReadOutputLoopAsync(process, ready));
    }

    private async Task ReadOutputLoopAsync(Process process, TaskCompletionSource ready)
    {
        try
        {
            string line;
            while ((line = await process.StandardOutput.ReadLineAsync()) != null)
            {
                HandleLine(line, ready);
            }

            await process.WaitForExitAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[worker] Error while reading worker output: {ex.Message}");
        }

        if (_disposed)
        {
            return;
        }

        var exitCode = process.HasExited ? process.ExitCode.ToString() : "unknown";
        var reason = $"Worker process exited (code {exitCode}). {StderrTail()}";
        Console.WriteLine($"[worker] {reason}");

        if (ReferenceEquals(_process, process))
        {
            // Не перетираємо більш інформативне повідомлення (fatal / timeout).
            if (_status.State != WorkerState.Failed)
            {
                SetStatus(new WorkerStatus(WorkerState.Failed, reason));
            }

            FailInflight(reason);
        }

        ready.TrySetException(new WorkerException(_status.Message ?? reason));
    }

    private void HandleLine(string line, TaskCompletionSource ready)
    {
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(line);
        }
        catch (JsonException)
        {
            // Якась бібліотека надрукувала щось напряму в stdout — це не повідомлення протоколу.
            Console.WriteLine($"[worker:stdout] {line}");
            return;
        }

        using (document)
        {
            var root = document.RootElement;

            if (root.TryGetProperty("type", out var typeElement))
            {
                switch (typeElement.GetString())
                {
                    case "ready":
                        SetStatus(new WorkerStatus(
                            WorkerState.Ready,
                            "Worker is ready.",
                            PythonVersion: GetString(root, "python"),
                            TensorFlowVersion: GetString(root, "tensorflow")));
                        ready.TrySetResult();
                        break;

                    case "fatal":
                        var error = GetString(root, "error") ?? "Worker failed to start.";
                        SetStatus(new WorkerStatus(WorkerState.Failed, error));
                        ready.TrySetException(new WorkerException(error));
                        break;
                }

                return;
            }

            var id = GetString(root, "id");
            PendingRequest pending;
            lock (_sync)
            {
                pending = _inflight?.Id == id ? _inflight : null;
                if (pending != null)
                {
                    _inflight = null;
                }
            }

            if (pending == null)
            {
                Console.WriteLine($"[worker] Ignoring response for unknown request '{id}'.");
                return;
            }

            if (root.TryGetProperty("ok", out var ok) && ok.GetBoolean())
            {
                pending.Completion.TrySetResult(root.GetProperty("result").Clone());
            }
            else
            {
                pending.Completion.TrySetException(new WorkerException(GetString(root, "error") ?? "Unknown worker error."));
            }
        }
    }

    private (string PythonPath, string ScriptPath, bool IsBundled) ResolveCommand()
    {
        var baseDirectory = AppContext.BaseDirectory;

        var pythonPath = _options.PythonPath;
        var isBundled = false;

        if (string.IsNullOrWhiteSpace(pythonPath))
        {
            var bundledPython = Path.Combine(baseDirectory, "python", "python.exe");
            isBundled = File.Exists(bundledPython);
            pythonPath = isBundled ? bundledPython : "python";
        }

        var scriptPath = string.IsNullOrWhiteSpace(_options.ScriptPath)
            ? Path.Combine(baseDirectory, "worker", "sdnnga_worker.py")
            : Path.GetFullPath(_options.ScriptPath);

        return (pythonPath, scriptPath, isBundled);
    }

    private void SetStatus(WorkerStatus status) => _status = status;

    private void ClearInflight(PendingRequest pending)
    {
        lock (_sync)
        {
            if (ReferenceEquals(_inflight, pending))
            {
                _inflight = null;
            }
        }
    }

    private void FailInflight(string reason)
    {
        PendingRequest pending;
        lock (_sync)
        {
            pending = _inflight;
            _inflight = null;
        }

        pending?.Completion.TrySetException(new WorkerException(reason));
    }

    private string StderrTail()
    {
        lock (_stderrTail)
        {
            return _stderrTail.Count == 0 ? string.Empty : "Last worker output: " + string.Join(Environment.NewLine, _stderrTail);
        }
    }

    private static bool IsAlive(Process process)
    {
        try
        {
            return process is { HasExited: false };
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    private static void KillProcess(Process process)
    {
        if (!IsAlive(process))
        {
            return;
        }

        try
        {
            process.Kill(entireProcessTree: true);
            process.WaitForExit(5000);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[worker] Failed to stop worker process: {ex.Message}");
        }
    }

    private static string GetString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private sealed class PendingRequest
    {
        public PendingRequest(string id) => Id = id;

        public string Id { get; }

        public TaskCompletionSource<JsonElement> Completion { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}

public class WorkerException : Exception
{
    public WorkerException(string message, Exception innerException = null)
        : base(message, innerException)
    {
    }
}
