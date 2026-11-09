using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Threading;
using SdnnGa.Model.Services;

namespace SdnnGa.Api.Controllers.V1;

[Route("api/[controller]")]
[ApiController]
public class NeuralNetworkModelController : ControllerBase
{
    private readonly INeuralNetworkModelService _neuralNetworkModelService;

    public NeuralNetworkModelController(
        INeuralNetworkModelService neuralNetworkModelService)
    {
        _neuralNetworkModelService = neuralNetworkModelService;
    }

    [HttpGet("ByEpoche/{epocheId}")]
    public async Task<IActionResult> GetByEpocheAsync(string epocheId, CancellationToken cancellationToken = default)
    {
        var result = await _neuralNetworkModelService.GetModelByEpochIdAsync(epocheId, true, cancellationToken);

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken = default)
    {
        var result = await _neuralNetworkModelService.GetAllModelsAsync(cancellationToken);

        return Ok(result);
    }
}
