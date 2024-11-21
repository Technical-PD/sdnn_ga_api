using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Threading;
using SdnnGa.Model.Infrastructure.Interfaces.RabbitMq;
using SdnnGa.Model.Infrastructure.Interfaces.Quartz.Scheduler;
using SdnnGa.Model.Models.Core.NNModel;
using System.Text.Json;
using SdnnGa.Core.Jobs;
using System.Collections.Generic;
using SdnnGa.Model.Services;

namespace SdnnGa.Api.Controllers.V1;

[Route("api/[controller]")]
[ApiController]
public class NeuralNetworkModelController : ControllerBase
{
    private readonly IJobScheduler _jobScheduler;
    private readonly IRabbitMqClientCreateModelService _rebbitMqClient;
    private readonly INeuralNetworkModelService _neuralNetworkModelService;

    public NeuralNetworkModelController(
        IJobScheduler jobScheduler,
        IRabbitMqClientCreateModelService rebbitMqClient,
        INeuralNetworkModelService neuralNetworkModelService)
    {
        _jobScheduler = jobScheduler;
        _rebbitMqClient = rebbitMqClient;
        _neuralNetworkModelService = neuralNetworkModelService;
    }

    [HttpGet("ByEpoche/{epocheId}")]
    public async Task<IActionResult> GetByEpocheAsync(string epocheId, CancellationToken cancellationToken = default)
    {
        var result = await _neuralNetworkModelService.GetModelByEpochIdAsync(epocheId, cancellationToken);

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken = default)
    {
        var result = await _neuralNetworkModelService.GetAllModelsAsync(cancellationToken);

        return Ok(result);
    }
}
