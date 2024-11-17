using Quartz;
using SdnnGa.Model.Constants;
using SdnnGa.Model.Core.Interfaces;
using SdnnGa.Model.Infrastructure.Interfaces.Quartz.Scheduler;
using SdnnGa.Model.Models;
using SdnnGa.Model.Models.Core.GAConfigs;
using SdnnGa.Model.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SdnnGa.Core.Jobs;

public class GeneticEpochJob : IGeneticEpochJob
{
    private readonly IJobScheduler _jobScheduler;
    private readonly IEpochService _epochService;
    private readonly ISessionService _sessionService;
    private readonly IFitConfigService _fitConfigService;
    private readonly INeuralNetworkModelService _networkModelService;

    private ModelRangeConfig _modelRangeConfig;
    private string _sessionId;

    public GeneticEpochJob(
        IJobScheduler jobScheduler,
        IEpochService epochService,
        ISessionService sessionService,
        IFitConfigService fitConfigService,
        INeuralNetworkModelService networkModelService)
    {
        _epochService = epochService;
        _sessionService = sessionService;
        _fitConfigService = fitConfigService;
        _networkModelService = networkModelService;
        _jobScheduler = jobScheduler;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        InizializeSettingValues(context);

        string modelConfigNetPath = string.Empty;

        var sessionEpochesResult = await _epochService.GetAllBySessionIdAsync(_sessionId);

        if (sessionEpochesResult.IsError)
        {
            Console.WriteLine($"Error on obtaining epoches in Genetic Flow for session with id='{_sessionId}'. Message: '{sessionEpochesResult.Message}'");
            return;
        }

        if (sessionEpochesResult.Entity != null && sessionEpochesResult.Entity.Any())
        {
            var latestEpoch = sessionEpochesResult.Entity.OrderBy(ep => ep.EpochNo).LastOrDefault();

            var epocheModelResult = await _networkModelService.GetModelByEpochIdAsync(latestEpoch.Id);

            if (epocheModelResult.IsError)
            {
                Console.WriteLine($"Error on obtaining models in Genetic Flow for session with id='{_sessionId}' and epochNo='{latestEpoch.EpochNo}'. Message: '{sessionEpochesResult.Message}'");
                return;
            }

            var bestModel = epocheModelResult.Entity.OrderBy(md => md.LossValue).FirstOrDefault();

            modelConfigNetPath = bestModel.ModelConfigDotNetFileName;
        }

        var currentEpochResult = await _epochService.AddEpochAsync(_sessionId);

        if (currentEpochResult.IsError)
        {
            Console.WriteLine($"Error on creating epoch in Genetic Flow. Message: '{currentEpochResult.Message}'");
            return;
        }

        var sessionResult = await _sessionService.GetSessionAsync(_sessionId);

        if (sessionResult.IsError)
        {
            Console.WriteLine($"Error on obtaining session in Genetic Flow. Message: '{sessionResult.Message}'");
            return;
        }

        var fitConfigResult = await _fitConfigService.GetFitConfigBySessionAsync(_sessionId);

        if (fitConfigResult.IsError)
        {
            Console.WriteLine($"Error on obtaining fit config in Genetic Flow. Message: '{fitConfigResult.Message}'");
            return;
        }

        for (int i = 0; i < _modelRangeConfig.CountOfModels; i++)
        {
            string modelConfigJson = JsonSerializer.Serialize(_modelRangeConfig);

            var newModel = new NeuralNetworkModel
            {
                SessionId = _sessionId,
                EpocheId = currentEpochResult.Entity.Id,
                IsTrained = false,
                Name = $"Model_{currentEpochResult.Entity.EpochNo}",
            };

            var createdModelResult = await _networkModelService.CreateModelAsync(newModel);

            var settingModelCreateJob = new Dictionary<string, string>
            {
                { JobSettings.CreateModel.ModelRangeConfigSettingName, modelConfigJson },
                { JobSettings.CreateModel.ModelConfigNetPathSettingName, modelConfigNetPath },
                { JobSettings.CreateModel.SessionIdSettingName, _sessionId },
                { JobSettings.CreateModel.EpocheNoSettingName, currentEpochResult.Entity.EpochNo.ToString() },
                { JobSettings.CreateModel.ModelIdSettingName, createdModelResult.Entity.Id }
            };

            var settingModelFitJob = new Dictionary<string, string>
            {
                { JobSettings.FitModel.BatchSizeSettingName, "50" },
                { JobSettings.FitModel.XTrainPathSettingName, sessionResult.Entity.XTrainFileName },
                { JobSettings.FitModel.YTrainPathSettingName, sessionResult.Entity.YTrainFileName },
                { JobSettings.FitModel.LossFuncSettingName, fitConfigResult.Entity.LossFunc },
                { JobSettings.FitModel.OptimizerSettingName, fitConfigResult.Entity.FitMethod },
                { JobSettings.FitModel.EpochsSettingName, fitConfigResult.Entity.MaxEpoches.ToString() },
                { JobSettings.FitModel.IsLearnWithValidationSettingName, "False" },
                { JobSettings.FitModel.UseEarlyStoppingSettingName, "False" },
                { JobSettings.FitModel.MinDeltaSettingName, "0.02" },
                { JobSettings.FitModel.PatienceSettingName, "True" },
                { JobSettings.FitModel.ModelIdSettingName, createdModelResult.Entity.Id },
                { JobSettings.FitModel.SessionIdSettingName, _sessionId },
                { JobSettings.FitModel.EpocheNoSettingName, currentEpochResult.Entity.EpochNo.ToString() },
            };

            await _jobScheduler.ScheduleSequentialJobsAsync<ICreateModelJob, IFitModelJob>(
                firstJobName: $"CreateModel-{Guid.NewGuid()}",
                secondJobName: $"FitModel-{Guid.NewGuid()}",
                firstJobSettings: settingModelCreateJob,
                secondJobSettings: settingModelFitJob);
        }
    }

    private void InizializeSettingValues(IJobExecutionContext context)
    {
        var jobDataMap = context.JobDetail.JobDataMap;

        _sessionId = jobDataMap.GetString(JobSettings.GeneticEpoche.SessionIdSettingName);

        var modelRangeConfig = jobDataMap.GetString(JobSettings.GeneticEpoche.ModelRangeConfigSettingName);
        _modelRangeConfig = JsonSerializer.Deserialize<ModelRangeConfig>(modelRangeConfig);
    }
}
