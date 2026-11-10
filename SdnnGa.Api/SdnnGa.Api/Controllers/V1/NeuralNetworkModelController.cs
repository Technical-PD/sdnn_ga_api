using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Threading;
using SdnnGa.Model.Services;
using SdnnGa.Model.Infrastructure.Interfaces.Training;
using SdnnGa.Model.Models.Core.NNModel;
using SdnnGa.Model.Services.Models.ServiceResult;
using System.Collections.Generic;

namespace SdnnGa.Api.Controllers.V1;

[Route("api/[controller]")]
[ApiController]
public class NeuralNetworkModelController : ControllerBase
{
    private readonly INeuralNetworkModelService _neuralNetworkModelService;
    private readonly ITrainingProgressTracker _progressTracker;

    public NeuralNetworkModelController(
        INeuralNetworkModelService neuralNetworkModelService,
        ITrainingProgressTracker progressTracker)
    {
        _neuralNetworkModelService = neuralNetworkModelService;
        _progressTracker = progressTracker;
    }

    /// <summary>
    /// Прогрес моделей сесії, які навчаються зараз (епоха, метрики).
    /// </summary>
    [HttpGet("Progress/BySession/{sessionId}")]
    public IActionResult GetTrainingProgress(string sessionId)
    {
        return Ok(ServiceResult<IReadOnlyList<TrainingProgress>>.FromSuccess(_progressTracker.GetBySession(sessionId)));
    }

    [HttpGet("ByEpoche/{epocheId}")]
    public async Task<IActionResult> GetByEpocheAsync(string epocheId, CancellationToken cancellationToken = default)
    {
        var result = await _neuralNetworkModelService.GetModelByEpochIdAsync(epocheId, true, cancellationToken);

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken = default)
    {
        var result = await _neuralNetworkModelService.GetAllModelsAsync(cancellationToken);

        return Ok(result);
    }
}
