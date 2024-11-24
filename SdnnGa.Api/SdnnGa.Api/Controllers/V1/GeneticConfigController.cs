using Microsoft.AspNetCore.Mvc;
using SdnnGa.Api.Controllers.V1.Models.GeneticConfig;
using SdnnGa.Model.Models;
using SdnnGa.Model.Services;
using System.Threading;
using System.Threading.Tasks;

namespace SdnnGa.Api.Controllers.V1;

[Route("api/[controller]")]
[ApiController]
public class GeneticConfigController : ControllerBase
{
    private readonly IGeneticConfigService _geneticConfigService;

    public GeneticConfigController(IGeneticConfigService geneticConfigService)
    {
        _geneticConfigService = geneticConfigService;
    }

    [HttpGet("BySession/{sessionId}")]
    public async Task<IActionResult> GetBySessionId(string sessionId, CancellationToken cancellationToken = default)
    {
        var sessionsResult = await _geneticConfigService.GetAllBySessionIdAsync(sessionId, cancellationToken);

        return Ok(sessionsResult);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken = default)
    {
        var sessionResult = await _geneticConfigService.GetByIdAsync(id, cancellationToken);
        
        return Ok(sessionResult);
    }

    [HttpPost("{sessionId}")]
    public async Task<IActionResult> Create(string sessionId, AddGeneticConfigRequest addGeneticConfigRequest, CancellationToken cancellationToken = default)
    {
        var geneticConfig = new GeneticConfig
        {
            Name = addGeneticConfigRequest.Name,
            MaxEpoches = addGeneticConfigRequest.MaxEpoches,
            MutationCof = addGeneticConfigRequest.MutationCof,
            SelectionCriterion = addGeneticConfigRequest.SelectionCriterion,
        };

        var sessionResult = await _geneticConfigService.AddGeneticConfigAsync(sessionId, geneticConfig, cancellationToken);

        return Ok(sessionResult);
    }
}
