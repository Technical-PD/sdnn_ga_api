using Microsoft.AspNetCore.Mvc;
using SdnnGa.Model.Services;
using System.Threading.Tasks;
using System.Threading;

namespace SdnnGa.Api.Controllers.V1;

[Route("api/[controller]")]
[ApiController]
public class StatisticController : ControllerBase
{
    private readonly IStatisticService _statisticService;

    public StatisticController(IStatisticService statisticService)
    {
        _statisticService = statisticService;
    }

    [HttpGet("Models/BySession/{sessionId}/ByLoss")]
    public async Task<IActionResult> GetBestModelsBySessionIdByLossAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var epochResult = await _statisticService.GetBestModelInSessionByLossAsync(sessionId, cancellationToken);

        return Ok(epochResult);
    }

    [HttpGet("Models/BySession/{sessionId}/ByAccuracy")]
    public async Task<IActionResult> GetBestModelsBySessionIdByAccucacyAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var epochResult = await _statisticService.GetBestModelInSessionByAccuracyAsync(sessionId, cancellationToken);

        return Ok(epochResult);
    }
}
