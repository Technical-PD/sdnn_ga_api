using Microsoft.AspNetCore.Mvc;
using SdnnGa.Api.Controllers.V1.Models.FitConfig;
using SdnnGa.Model.Models;
using SdnnGa.Model.Services;
using System.Threading;
using System.Threading.Tasks;

namespace SdnnGa.Api.Controllers.V1;

[Route("api/[controller]")]
[ApiController]
public class FitConfigController : ControllerBase
{
    private readonly IFitConfigService _fitConfigService;

    public FitConfigController(IFitConfigService fitConfigService)
    {
        _fitConfigService = fitConfigService;
    }

    [HttpPost("AddToSession/{sessionId}")]
    public async Task<IActionResult> Add(string sessionId, [FromBody] CreateFitConfigRequest createFitConfigRequest, CancellationToken cancellationToken = default)
    {
        var fitConfig = new FitConfig
        {
            SessionId = sessionId,
            LossFunc = createFitConfigRequest.LossFunc,
            MaxEpoches = createFitConfigRequest.MaxEpoches,
            FitMethod = createFitConfigRequest.FitMethod,
            Name = createFitConfigRequest.Name,
        };

        var epochResult = await _fitConfigService.AddFitConfigAssync(fitConfig, cancellationToken);

        return Ok(epochResult);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
    {
        var epochResult = await _fitConfigService.GetAllFitConfigsAsync(cancellationToken);

        return Ok(epochResult);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken = default)
    {
        var epochResult = await _fitConfigService.GetFitConfigAsync(id, cancellationToken);

        return Ok(epochResult);
    }

    [HttpGet("BySession/{sessionId}")]
    public async Task<IActionResult> GetAllBySessionId(string sessionId, CancellationToken cancellationToken = default)
    {
        var epochResult = await _fitConfigService.GetFitConfigBySessionAsync(sessionId, cancellationToken);

        return Ok(epochResult);
    }
}
