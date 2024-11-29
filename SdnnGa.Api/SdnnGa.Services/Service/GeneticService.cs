using SdnnGa.Model.Constants;
using SdnnGa.Model.Core.Interfaces;
using SdnnGa.Model.Infrastructure.Interfaces.Quartz.Scheduler;
using SdnnGa.Model.Models.Core.GAConfigs;
using SdnnGa.Model.Services.Models.ServiceResult;
using SdnnGa.Model.Services;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace SdnnGa.Services.Service;

public class GeneticService : IGeneticService
{
    private readonly IJobScheduler _jobScheduler;

    public GeneticService(IJobScheduler jobScheduler)
    {
        _jobScheduler = jobScheduler;
    }

    public async Task<ServiceResult> StartGeneticFlow(
        ModelRangeConfig modelRangeConfig,
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string modelRangeConfigJson = JsonSerializer.Serialize(modelRangeConfig);

            var geneticJobSettngs = new Dictionary<string, string>
            {
                { JobSettings.GeneticEpoche.SessionIdSettingName, sessionId },
                { JobSettings.GeneticEpoche.ModelRangeConfigSettingName, modelRangeConfigJson },
            };

            await _jobScheduler.ScheduleJobAsync<IGeneticEpochJob>($"GeneticEpochRun-{Guid.NewGuid()}", $"Epoche-0-{sessionId}", geneticJobSettngs);

            return ServiceResult.FromSuccess();
        }
        catch (Exception e)
        {
            return ServiceResult.FromUnexpectedError($"Unexpected error occured on starting GeneticFlow. Message: '{e.Message}'");
        }
    }
}
