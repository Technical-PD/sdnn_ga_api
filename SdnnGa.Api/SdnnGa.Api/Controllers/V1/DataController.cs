using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SdnnGa.Api.Controllers.V1.Models.Data;
using SdnnGa.Model.Enums;
using SdnnGa.Model.Models;
using SdnnGa.Model.Services;
using SdnnGa.Model.Services.Models.ServiceResult;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SdnnGa.Api.Controllers.V1;

[Route("api/[controller]")]
[ApiController]
public class DataController : ControllerBase
{
    private readonly IDataService _dataService;

    public DataController(IDataService dataService)
    {
        _dataService = dataService;
    }

    [HttpPost("AddXData/{sessionId}")]
    public async Task<IActionResult> AddXData(
        [FromForm] IFormFile dataForm,
        string sessionId, 
        CancellationToken cancellationToken = default)
    {
        if (dataForm == null || dataForm.Length == 0)
        {
            return BadRequest("File is empty.");
        }

        var result = ServiceResult<Session>.FromSuccess(null);

        using (var memoryStream = new MemoryStream())
        {
            await dataForm.CopyToAsync(memoryStream);

            result = await _dataService.PutTrainDataAsync(sessionId, memoryStream, DataType.XData, cancellationToken);
        }

        return Ok(result);
    }

    [HttpPost("AddYData/{sessionId}")]
    public async Task<IActionResult> AddYData(
        [FromForm] IFormFile dataForm,
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        if (dataForm == null || dataForm.Length == 0)
        {
            return BadRequest("File is empty.");
        }

        var result = ServiceResult<Session>.FromSuccess(null);

        using (var memoryStream = new MemoryStream())
        {
            await dataForm.CopyToAsync(memoryStream);

            result = await _dataService.PutTrainDataAsync(sessionId, memoryStream, DataType.YData, cancellationToken);
        }

        return Ok(result);
    }

    [HttpPost("GetDataFile")]
    public async Task<IActionResult> GetDataSetAsync(GetDataRequest getDataRequest)
    {
        var datasetResult = await _dataService.GetDataFromStorageAsync(getDataRequest.DataStoragePath);

        if (datasetResult.Entity == null)
        {
            return Ok(datasetResult);
        }

        return File(datasetResult.Entity.OpenReadStream(), datasetResult.Entity.ContentType);
    }
}
