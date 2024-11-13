using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Threading;
using SdnnGa.Model.Infrastructure.Interfaces.RabbitMq;
using SdnnGa.Model.Infrastructure.Interfaces.Quartz.Scheduler;
using SdnnGa.Model.Models.Core.NNModel;
using System.Text.Json;
using SdnnGa.Core.Jobs;
using System.Collections.Generic;

namespace SdnnGa.Api.Controllers.V1;

[Route("api/[controller]")]
[ApiController]
public class NeuralNetworkModelController : ControllerBase
{
    private readonly IJobScheduler _jobScheduler;
    private readonly IRabbitMqClient _rebbitMqClient;

    public NeuralNetworkModelController(
        IJobScheduler jobScheduler,
        IRabbitMqClient rebbitMqClient)
    {
        _jobScheduler = jobScheduler;
        _rebbitMqClient = rebbitMqClient;
    }

    [HttpGet("{message}")]
    public async Task<IActionResult> TestRebbitMq(string message, CancellationToken cancellationToken = default)
    {
        var response = await _rebbitMqClient.SendMessageAsync(message);
        _rebbitMqClient.Close();

        return Ok(response);
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
