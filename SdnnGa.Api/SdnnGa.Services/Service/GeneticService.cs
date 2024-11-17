using SdnnGa.Model.Constants;
using SdnnGa.Model.Core.Interfaces;
using SdnnGa.Model.Infrastructure.Interfaces.Quartz.Scheduler;
using SdnnGa.Model.Models.Core.GAConfigs;
using SdnnGa.Model.Services;
using SdnnGa.Model.Services.Models.ServiceResult;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SdnnGa.Services.Service;

public class GeneticService : IGeneticService
{
    private readonly IEpochService _epochService;
    private readonly ISessionService _sessionService;
    private readonly IJobScheduler _jobScheduler;
    private readonly IFitConfigService _fitConfigService;
    private readonly INeuralNetworkModelService _networkModelService;

    public GeneticService(
        IEpochService epochService,
        IJobScheduler jobScheduler,
        ISessionService sessionService,
        INeuralNetworkModelService networkModelService,
        IFitConfigService fitConfigService)
    {
        _epochService = epochService;
        _jobScheduler = jobScheduler;
        _sessionService = sessionService;
        _networkModelService = networkModelService;
        _fitConfigService = fitConfigService;
    }

    public async Task<ServiceResult> StartGeneticFlow(
        ModelRangeConfig modelRangeConfig, 
        CompileConfig compileConfig, 
        TrainConfig trainConfig, 
        char separator, 
        string sessionId, 
        CancellationToken cancellationToken = default)
    {
        string modelRangeConfigJson = JsonSerializer.Serialize(modelRangeConfig);

        var geneticJobSettngs = new Dictionary<string, string>
        {
            { JobSettings.GeneticEpoche.SessionIdSettingName, sessionId },
            { JobSettings.GeneticEpoche.ModelRangeConfigSettingName, modelRangeConfigJson },
        };

        await _jobScheduler.ScheduleJobAsync<IGeneticEpochJob>($"GeneticEpochRun-{Guid.NewGuid()}", geneticJobSettngs);

        return ServiceResult.FromSuccess();
    }

    public async Task<ServiceResult> TestLearningAsync(string sessionId, string modelId, CancellationToken cancellationToken = default)
    {
        var modelResult = await _networkModelService.GetModelByIdAsync(modelId, cancellationToken);

        if (modelResult.IsError)
        {
            ServiceResult.FromError($"Error on obtaining models in Genetic Flow. Message: '{modelResult.Message}'");
        }

        var sessionResult = await _sessionService.GetSessionAsync(sessionId, cancellationToken);

        if (sessionResult.IsError)
        {
            ServiceResult.FromError($"Error on obtaining session in Genetic Flow. Message: '{sessionResult.Message}'");
        }

        var fitConfigResult = await _fitConfigService.GetFitConfigBySessionAsync(sessionId, cancellationToken);

        if (fitConfigResult.IsError)
        {
            ServiceResult.FromError($"Error on obtaining fit config in Genetic Flow. Message: '{fitConfigResult.Message}'");
        }

        var weightPath = string.Format(StoragePath.WeightPath, sessionId, "0", Guid.NewGuid());

        var settingDict = new Dictionary<string, string>
        {
            { JobSettings.FitModel.BatchSizeSettingName, "50" },
            //{ JobSettings.FitModelJob.ModelConfigJsonPathSettingName, modelResult.Entity.ModelConfigFileName },
            { JobSettings.FitModel.XTrainPathSettingName, sessionResult.Entity.XTrainFileName },
            { JobSettings.FitModel.YTrainPathSettingName, sessionResult.Entity.YTrainFileName },
            { JobSettings.FitModel.LossFuncSettingName, fitConfigResult.Entity.LossFunc },
            { JobSettings.FitModel.OptimizerSettingName, fitConfigResult.Entity.FitMethod },
            { JobSettings.FitModel.EpochsSettingName, fitConfigResult.Entity.MaxEpoches.ToString() },
            { JobSettings.FitModel.IsLearnWithValidationSettingName, "False" },
            { JobSettings.FitModel.UseEarlyStoppingSettingName, "False" },
            { JobSettings.FitModel.MinDeltaSettingName, "0.02" },
            { JobSettings.FitModel.PatienceSettingName, "True" },
            //{ JobSettings.FitModelJob.WeightPathSettingName, weightPath },
            { JobSettings.FitModel.ModelIdSettingName, modelId },
        };

        await _jobScheduler.ScheduleJobAsync<IFitModelJob>("FitModel", settingDict);

        return ServiceResult.FromSuccess();
    }
}
