using Microsoft.Extensions.Hosting;
using SdnnGa.Model.Infrastructure.Interfaces.Worker;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SdnnGa.Api.Desktop;

/// <summary>
/// Запускає Python-воркер одразу після старту застосунку: імпорт TensorFlow займає десятки секунд,
/// тож краще зробити це, поки користувач налаштовує сесію.
/// </summary>
public class WorkerWarmupService : BackgroundService
{
    private readonly IModelWorkerClient _workerClient;

    public WorkerWarmupService(IModelWorkerClient workerClient)
    {
        _workerClient = workerClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _workerClient.EnsureStartedAsync(stoppingToken);
            var status = _workerClient.Status;
            Console.WriteLine($"[worker] Ready: Python {status.PythonVersion}, TensorFlow {status.TensorFlowVersion}");
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[worker] Worker is not available: {ex.Message}");
        }
    }
}
