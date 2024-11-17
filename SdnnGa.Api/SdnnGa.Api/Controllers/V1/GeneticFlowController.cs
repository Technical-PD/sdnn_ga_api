using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Threading;
using SdnnGa.Api.Controllers.V1.Models.GeneticFlow;
using SdnnGa.Model.Services;

namespace SdnnGa.Api.Controllers.V1;

[Route("api/[controller]")]
[ApiController]
public class GeneticFlowController : ControllerBase
{
    private readonly IGeneticService _geneticService;

    public GeneticFlowController(IGeneticService geneticService)
    {
        _geneticService = geneticService;
    }

    [HttpPost("StartFlow")]
    public async Task<IActionResult> StartFlow([FromBody]GeneticConfigRequest geneticConfigRequest, [FromQuery]string sessionId, CancellationToken cancellationToken = default)
    {
        var result = await _geneticService.StartGeneticFlow(
            modelRangeConfig: geneticConfigRequest.ModelRangeConfig,
            compileConfig: geneticConfigRequest.CompileConfig,
            trainConfig: geneticConfigRequest.TrainConfig,
            separator: geneticConfigRequest.DataSeparator,
            sessionId: sessionId,
            cancellationToken: cancellationToken);

        return Ok(result);
    }

    [HttpPost("TestFitModel/{modelId}")]
    public async Task<IActionResult> TestFitting([FromQuery] string sessionId, string modelId, CancellationToken cancellationToken = default)
    {
        var result = await _geneticService.TestLearningAsync(sessionId, modelId);

        return Ok(result);
    }
}
