using Microsoft.AspNetCore.Mvc;
using SdnnGa.Model.Services;
using System.Threading;
using System.Threading.Tasks;

namespace SdnnGa.Api.Controllers.V1;

[Route("api/[controller]")]
[ApiController]
public class EpochesController : ControllerBase
{
    private readonly IEpochService _epochService;

    public EpochesController(IEpochService epochService)
    {
        _epochService = epochService;
    }

    [HttpPost("Add")]
    public async Task<IActionResult> Add([FromQuery]string sessionId, CancellationToken cancellationToken = default)
    {
        var epochResult = await _epochService.AddEpochAsync(sessionId, cancellationToken);

        return Ok(epochResult);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
    {
        var epochResult = await _epochService.GetAllAsync(cancellationToken);

        return Ok(epochResult);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken = default)
    {
        var epochResult = await _epochService.GetByIdAsync(id, cancellationToken);

        return Ok(epochResult);
    }

    [HttpGet("BySession/{sessionId}")]
    public async Task<IActionResult> GetAllBySessionId(string sessionId, CancellationToken cancellationToken = default)
    {
        var epochResult = await _epochService.GetAllBySessionIdAsync(sessionId, cancellationToken);

        return Ok(epochResult);
    }
}
