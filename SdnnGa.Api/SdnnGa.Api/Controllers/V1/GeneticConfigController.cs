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
        var sessionResult = await _geneticConfigService.GetAllBySessionIdAsync(sessionId, cancellationToken);

        if (sessionResult != null && sessionResult.IsSuccessful)
        {
            sessionResult.Entity.ActFuncMutationProb *= 100;
            sessionResult.Entity.CountOfNeuronMutationProb *= 100;
            sessionResult.Entity.CountOfInternalLayerMutationProb *= 100;
            sessionResult.Entity.BiasMutationProb *= 100;
            sessionResult.Entity.StopAccValue *= 100;
        }

        return Ok(sessionResult);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken = default)
    {
        var sessionResult = await _geneticConfigService.GetByIdAsync(id, cancellationToken);

        if (sessionResult != null && sessionResult.IsSuccessful)
        {
            sessionResult.Entity.ActFuncMutationProb *= 100;
            sessionResult.Entity.CountOfNeuronMutationProb *= 100;
            sessionResult.Entity.CountOfInternalLayerMutationProb *= 100;
            sessionResult.Entity.BiasMutationProb *= 100;
            sessionResult.Entity.StopAccValue *= 100;
        }
        
        return Ok(sessionResult);
    }

    [HttpPost("{sessionId}")]
    public async Task<IActionResult> Create(string sessionId, AddGeneticConfigRequest addGeneticConfigRequest, CancellationToken cancellationToken = default)
    {
        var geneticConfig = new GeneticConfig
        {
            Name = addGeneticConfigRequest.Name,
            MaxEpoches = addGeneticConfigRequest.MaxEpoches,
            ActFuncMutationProb = addGeneticConfigRequest.ActFuncMutationProb / 100,
            BiasMutationProb = addGeneticConfigRequest.BiasMutationProb / 100,
            CountOfInternalLayerMutationProb = addGeneticConfigRequest.CountOfInternalLayerMutationProb / 100,
            CountOfNeuronMutationProb = addGeneticConfigRequest.CountOfNeuronMutationProb / 100,
            StopAccValue = addGeneticConfigRequest.StopAccValue / 100,
            StopLossValue = addGeneticConfigRequest.StopLossValue,
            CountOfModelsInEpoch = addGeneticConfigRequest.CountOfModelsInEpoch,
            SelectionCriterion = addGeneticConfigRequest.SelectionCriterion,
        };

        var sessionResult = await _geneticConfigService.AddGeneticConfigAsync(sessionId, geneticConfig, cancellationToken);

        return Ok(sessionResult);
    }
}
