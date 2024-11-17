using Microsoft.AspNetCore.Mvc;
using SdnnGa.Model.Infrastructure.Interfaces.RabbitMq;
using System.Threading;
using System.Threading.Tasks;

namespace SdnnGa.Api.Controllers.V1;

[Route("api/[controller]")]
[ApiController]
public class RabbitMqController : ControllerBase
{
    private readonly IRabbitMqClientCreateModelService _rabbitMqClientCreateModelService;
    private readonly IRabbitMqClientFitModelService _rabbitMqClientFitModelService;

    public RabbitMqController(
        IRabbitMqClientCreateModelService rabbitMqClientCreateModelService,
        IRabbitMqClientFitModelService rabbitMqClientFitModelService)
    {
        _rabbitMqClientCreateModelService = rabbitMqClientCreateModelService;
        _rabbitMqClientFitModelService = rabbitMqClientFitModelService;
    }

    [HttpGet("Queues/Clean")]
    public async Task<IActionResult> CleanRabbitMqQueues(CancellationToken cancellationToken = default)
    {
        _rabbitMqClientCreateModelService.PurgeRequestQueue();
        _rabbitMqClientCreateModelService.PurgeResponseQueue();

        _rabbitMqClientFitModelService.PurgeRequestQueue();
        _rabbitMqClientFitModelService.PurgeResponseQueue();

        return Ok();
    }
}
