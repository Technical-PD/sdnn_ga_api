﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SdnnGa.Model.Enums;
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

        var result = ServiceResult.FromSuccess();

        using (var memoryStream = new MemoryStream())
        {
            await dataForm.CopyToAsync(memoryStream);

            result = await _dataService.PutTrainDataAsync(sessionId, memoryStream, DataType.XData);
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

        var result = ServiceResult.FromSuccess();

        using (var memoryStream = new MemoryStream())
        {
            await dataForm.CopyToAsync(memoryStream);

            result = await _dataService.PutTrainDataAsync(sessionId, memoryStream, DataType.YData);
        }

        return Ok(result);
    }
}
