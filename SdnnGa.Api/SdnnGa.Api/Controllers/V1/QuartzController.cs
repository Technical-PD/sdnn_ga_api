using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Extensions;
using Quartz;
using Quartz.Impl.Matchers;
using SdnnGa.Api.Controllers.V1.Models.Session;
using SdnnGa.Model.Infrastructure.Interfaces.Quartz.Scheduler;
using SdnnGa.Model.Models;
using SdnnGa.Model.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SdnnGa.Api.Controllers.V1;

[Route("api/[controller]")]
[ApiController]
public class QuartzController : ControllerBase
{
    private readonly ISchedulerService _schedulerService;

    public QuartzController(ISchedulerService schedulerService)
    {
        _schedulerService = schedulerService;
    }

    /// <summary>
    /// Gets all.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpGet("Triggers")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
    {
        var scheduler = await _schedulerService.GetSchedulerAsync();

        var triggers = await scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup());

        var triggerDict = new Dictionary<string, string>();

        foreach (var triggerKey in triggers)
        {
            var triggerState = await scheduler.GetTriggerState(triggerKey, cancellationToken);
            triggerDict.Add(triggerKey.Name, triggerState.GetDisplayName());
        }

        return Ok(triggerDict);
    }
}
