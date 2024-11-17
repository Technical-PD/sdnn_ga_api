using Quartz;
using SdnnGa.Model.Constants;
using SdnnGa.Model.Core.Interfaces;
using SdnnGa.Model.Infrastructure.Interfaces.Quartz.Scheduler;
using SdnnGa.Model.Models;
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
        var currentEpochResult = await _epochService.AddEpochAsync(sessionId, cancellationToken);

        if (currentEpochResult.IsError)
        {
            ServiceResult.FromError($"Error on creating epoch in Genetic Flow. Message: '{currentEpochResult.Message}'");
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

        for (int i = 0; i < modelRangeConfig.CountOfModels; i++)
        {
            string modelConfigJson = JsonSerializer.Serialize(modelRangeConfig);

            var newModel = new NeuralNetworkModel
            {
                SessionId = sessionId,
                EpocheId = currentEpochResult.Entity.Id,
                IsTrained = false,
                Name = $"Model_{currentEpochResult.Entity.EpochNo}",
            };

            var createdModelResult = await _networkModelService.CreateModelAsync(newModel);

            //await _jobScheduler.ScheduleJobAsync<ICreateModelJob>("CreateModel", new Dictionary<string, string>
            //{
            //    { JobSettings.CreateModel.ModelConfigSettingName, modelConfigJson },
            //    { JobSettings.CreateModel.SessionIdSettingName, sessionId },
            //    { JobSettings.CreateModel.EpocheNoSettingName, currentEpochResult.Entity.EpochNo.ToString() },
            //    { JobSettings.CreateModel.ModelIdSettingName, createdModelResult.Entity.Id }
            //});
            
            var settingModelCreateJob = new Dictionary<string, string>
            {
                { JobSettings.CreateModel.ModelConfigSettingName, modelConfigJson },
                { JobSettings.CreateModel.SessionIdSettingName, sessionId },
                { JobSettings.CreateModel.EpocheNoSettingName, currentEpochResult.Entity.EpochNo.ToString() },
                { JobSettings.CreateModel.ModelIdSettingName, createdModelResult.Entity.Id }
            };

            var settingModelFitJob = new Dictionary<string, string>
            {
                { JobSettings.FitModelJob.BatchSizeSettingName, "50" },
                { JobSettings.FitModelJob.XTrainPathSettingName, sessionResult.Entity.XTrainFileName },
                { JobSettings.FitModelJob.YTrainPathSettingName, sessionResult.Entity.YTrainFileName },
                { JobSettings.FitModelJob.LossFuncSettingName, fitConfigResult.Entity.LossFunc },
                { JobSettings.FitModelJob.OptimizerSettingName, fitConfigResult.Entity.FitMethod },
                { JobSettings.FitModelJob.EpochsSettingName, fitConfigResult.Entity.MaxEpoches.ToString() },
                { JobSettings.FitModelJob.IsLearnWithValidationSettingName, "False" },
                { JobSettings.FitModelJob.UseEarlyStoppingSettingName, "False" },
                { JobSettings.FitModelJob.MinDeltaSettingName, "0.02" },
                { JobSettings.FitModelJob.PatienceSettingName, "True" },
                { JobSettings.FitModelJob.ModelIdSettingName, createdModelResult.Entity.Id },
                { JobSettings.FitModelJob.SessionIdSettingName, sessionId },
                { JobSettings.FitModelJob.EpocheNoSettingName, currentEpochResult.Entity.EpochNo.ToString() },
            };

            await _jobScheduler.ScheduleSequentialJobsAsync<ICreateModelJob, IFitModelJob>(
                firstJobName: $"CreateModel-{Guid.NewGuid()}",
                secondJobName: $"FitModel-{Guid.NewGuid()}",
                firstJobSettings: settingModelCreateJob,
                secondJobSettings: settingModelFitJob);
        }

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
            { JobSettings.FitModelJob.BatchSizeSettingName, "50" },
            //{ JobSettings.FitModelJob.ModelConfigJsonPathSettingName, modelResult.Entity.ModelConfigFileName },
            { JobSettings.FitModelJob.XTrainPathSettingName, sessionResult.Entity.XTrainFileName },
            { JobSettings.FitModelJob.YTrainPathSettingName, sessionResult.Entity.YTrainFileName },
            { JobSettings.FitModelJob.LossFuncSettingName, fitConfigResult.Entity.LossFunc },
            { JobSettings.FitModelJob.OptimizerSettingName, fitConfigResult.Entity.FitMethod },
            { JobSettings.FitModelJob.EpochsSettingName, fitConfigResult.Entity.MaxEpoches.ToString() },
            { JobSettings.FitModelJob.IsLearnWithValidationSettingName, "False" },
            { JobSettings.FitModelJob.UseEarlyStoppingSettingName, "False" },
            { JobSettings.FitModelJob.MinDeltaSettingName, "0.02" },
            { JobSettings.FitModelJob.PatienceSettingName, "True" },
            //{ JobSettings.FitModelJob.WeightPathSettingName, weightPath },
            { JobSettings.FitModelJob.ModelIdSettingName, modelId },
        };

        await _jobScheduler.ScheduleJobAsync<IFitModelJob>("FitModel", settingDict);

        return ServiceResult.FromSuccess();
    }
}
