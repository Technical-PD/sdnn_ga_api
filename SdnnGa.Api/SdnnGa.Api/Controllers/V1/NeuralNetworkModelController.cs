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
    private readonly IRabbitMqClient _rebbitMqClient;
    private readonly INeuralNetworkModelService _neuralNetworkModelService;

    public NeuralNetworkModelController(
        IJobScheduler jobScheduler,
        IRabbitMqClient rebbitMqClient,
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

    [HttpGet("ModelCreateTest")]
    public async Task<IActionResult> TestPythonModelCreate(string message, CancellationToken cancellationToken = default)
    {
        var testModelConfig = new ModelConfig
        {
            InputShape = [2],
            InternalLayers = 
            [
                new Layer
                {
                    ActivationFunc = "relu",
                    NeuronsCount = 5
                },
                new Layer
                {
                    ActivationFunc = "sigmoid",
                    NeuronsCount = 10
                },
                new Layer
                {
                    ActivationFunc = "tanh",
                    NeuronsCount = 7
                },
            ],
            OutputLayer = new Layer
            {
                ActivationFunc = "tanh",
                NeuronsCount = 2
            }
        };

        var jsonData = JsonSerializer.Serialize(testModelConfig);

        await _jobScheduler.ScheduleJobAsync<CreateModelJob>("CreateModelTest", new Dictionary<string, string>
        {
            { "modelConfig", jsonData }
        });

        return Ok();
    }
}
