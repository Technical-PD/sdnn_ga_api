using Microsoft.AspNetCore.Mvc;
using SdnnGa.Api.Desktop;
using SdnnGa.Model.Infrastructure.Interfaces.Worker;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SdnnGa.Api.Controllers.V1;

[Route("api/[controller]")]
[ApiController]
public class SystemController : ControllerBase
{
    private readonly IModelWorkerClient _workerClient;
    private readonly AppPaths _appPaths;

    public SystemController(IModelWorkerClient workerClient, AppPaths appPaths)
    {
        _workerClient = workerClient;
        _appPaths = appPaths;
    }

    /// <summary>
    /// Стан локального Python-воркера та розташування даних застосунку.
    /// </summary>
    [HttpGet("Status")]
    public IActionResult GetStatus() => Ok(BuildStatus());

    /// <summary>
    /// Повторний запуск воркера (наприклад, після встановлення TensorFlow).
    /// </summary>
    [HttpPost("Worker/Start")]
    public async Task<IActionResult> StartWorker(CancellationToken cancellationToken = default)
    {
        try
        {
            await _workerClient.EnsureStartedAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Деталі вже записані в Status.
        }

        return Ok(BuildStatus());
    }

    private object BuildStatus() => new
    {
        Worker = _workerClient.Status,
        _appPaths.DataDirectory,
        LogFile = FileLog.CurrentLogFile,
        Version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString(),
    };
}
