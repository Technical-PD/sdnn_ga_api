using Quartz;
using Quartz.Impl.Matchers;
using SdnnGa.Core.Listeners;
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
    private readonly ISchedulerService _schedulerService;
    private readonly IGeneticConfigService _geneticConfigService;

    private ModelRangeConfig _modelRangeConfig;
    private string _sessionId;
    private int _modelCount;

    public GeneticEpochJob(
        IJobScheduler jobScheduler,
        IEpochService epochService,
        ISessionService sessionService,
        IFitConfigService fitConfigService,
        INeuralNetworkModelService networkModelService,
        ISchedulerService schedulerService,
        IGeneticConfigService geneticConfigService)
    {
        _epochService = epochService;
        _sessionService = sessionService;
        _fitConfigService = fitConfigService;
        _networkModelService = networkModelService;
        _jobScheduler = jobScheduler;
        _schedulerService = schedulerService;
        _geneticConfigService = geneticConfigService;
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

        var geneticConfigResult = await _geneticConfigService.GetAllBySessionIdAsync(_sessionId);

        if (geneticConfigResult.IsError)
        {
            Console.WriteLine($"Error on obtaining genetic config in Genetic Flow. Message: '{geneticConfigResult.Message}'");
            return;
        }

        var currentGeneticConfig = geneticConfigResult.Entity;

        if (currentGeneticConfig == null)
        {
            Console.WriteLine($"Genetic Config does not exist for this session.");
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

            NeuralNetworkModel bestModel = null;

            if (currentGeneticConfig.SelectionCriterion == SelectionCriterion.ByAccuracy)
            {
                bestModel = epocheModelResult.Entity.OrderBy(md => md.LossValue).FirstOrDefault();
            }
            else
            {
                bestModel = epocheModelResult.Entity.OrderBy(md => md.AccuracyValue).LastOrDefault();
            }

            modelConfigNetPath = bestModel?.ModelConfigDotNetFileName;

            if (string.IsNullOrEmpty(modelConfigNetPath))
            {
                Console.WriteLine($"ModelConfigNetPath is null in Genetic Flow for session with id='{_sessionId}' and epochNo='{latestEpoch.EpochNo}'. Message: '{sessionEpochesResult.Message}'");
                return;
            }
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

        var scheduler = await _schedulerService.GetSchedulerAsync();

        var childJobs = new List<IJobDetail>();

        for (int i = 0; i < _modelCount; i++)
        {
            string modelConfigJson = string.Empty;

            if (_modelRangeConfig != null) 
            {
                modelConfigJson = JsonSerializer.Serialize(_modelRangeConfig);
            }
           
            var newModel = new NeuralNetworkModel
            {
                SessionId = _sessionId,
                EpocheId = currentEpochResult.Entity.Id,
                IsTrained = false,
                Name = $"Model_{currentEpochResult.Entity.EpochNo}_{i+1}",
            };

            var createdModelResult = await _networkModelService.CreateModelAsync(newModel);

            var settingModelCreateJob = new Dictionary<string, string>
            {
                { JobSettings.CreateModel.ModelRangeConfigSettingName, modelConfigJson },
                { JobSettings.CreateModel.ModelConfigNetPathSettingName, modelConfigNetPath },
                { JobSettings.CreateModel.SessionIdSettingName, _sessionId },
                { JobSettings.CreateModel.EpocheNoSettingName, currentEpochResult.Entity.EpochNo.ToString() },
                { JobSettings.CreateModel.ModelIdSettingName, createdModelResult.Entity.Id },
                { JobSettings.CreateModel.MutationCofSettingName, currentGeneticConfig.MutationCof.ToString() }
            };

            var settingModelFitJob = new Dictionary<string, string>
            {
                { JobSettings.FitModel.BatchSizeSettingName, "50" },
                { JobSettings.FitModel.EpocheNoSettingName, currentEpochResult.Entity.EpochNo.ToString() },
                { JobSettings.FitModel.EpochsSettingName, fitConfigResult.Entity.MaxEpoches.ToString() },
                { JobSettings.FitModel.IsLearnWithValidationSettingName, "True" },
                { JobSettings.FitModel.LossFuncSettingName, fitConfigResult.Entity.LossFunc },
                { JobSettings.FitModel.MinDeltaSettingName, "0.02" },
                { JobSettings.FitModel.ModelIdSettingName, createdModelResult.Entity.Id },
                { JobSettings.FitModel.OptimizerSettingName, fitConfigResult.Entity.FitMethod },
                { JobSettings.FitModel.PatienceSettingName, "True" },
                { JobSettings.FitModel.SessionIdSettingName, _sessionId },
                { JobSettings.FitModel.UseEarlyStoppingSettingName, "False" },
                { JobSettings.FitModel.XTrainPathSettingName, sessionResult.Entity.XTrainFileName },
                { JobSettings.FitModel.YTrainPathSettingName, sessionResult.Entity.YTrainFileName }
            };

            var createJob = JobBuilder.Create<ICreateModelJob>()
                .WithIdentity($"CreateModel-{currentEpochResult.Entity.EpochNo}-{Guid.NewGuid()}", $"Epoche-{currentEpochResult.Entity.EpochNo}-{_sessionId}")
                .UsingJobData(new JobDataMap(settingModelCreateJob))
                .StoreDurably()
                .Build();

            var fitJob = JobBuilder.Create<IFitModelJob>()
                .WithIdentity($"FitModel-{currentEpochResult.Entity.EpochNo}-{Guid.NewGuid()}", $"Epoche-{currentEpochResult.Entity.EpochNo}-{_sessionId}")
                .UsingJobData(new JobDataMap(settingModelFitJob))
                .StoreDurably()
                .Build();

            await scheduler.AddJob(createJob, true);
            await scheduler.AddJob(fitJob, true);

            var createTrigger = TriggerBuilder.Create()
                .ForJob(createJob)
                .StartNow()
                .Build();

            await scheduler.ScheduleJob(createTrigger);

            var nextFireTime = createTrigger.GetNextFireTimeUtc();

            if (nextFireTime.HasValue) 
            {
                var fitTrigger = TriggerBuilder.Create()
                    .ForJob(fitJob)
                    .StartAt(nextFireTime.Value.AddSeconds(10))
                    .Build();
                await scheduler.ScheduleJob(fitTrigger);
            }
            else
            {
                Console.WriteLine("Failed to determine the next fire time for the create trigger.");
                return;
            }

            childJobs.Add(createJob);
            childJobs.Add(fitJob);
        }

        var nextEpochIndex = currentEpochResult.Entity.EpochNo + 1;

        Console.WriteLine($"\n\n==============> CURRENT EPOCHE {nextEpochIndex}");

        
        Console.WriteLine($"\n\n############################## {nextEpochIndex} ##############################");

        var nextJobSettings = new Dictionary<string, string>
        {
            { JobSettings.GeneticEpoche.SessionIdSettingName, _sessionId },
            { JobSettings.GeneticEpoche.ModelCountSettingName, _modelCount.ToString() },
            { JobSettings.GeneticEpoche.ModelRangeConfigSettingName, JsonSerializer.Serialize(_modelRangeConfig) },
        };

        scheduler.ListenerManager.AddJobListener(
            jobListener: new ParentJobCompletionListener(
                scheduler: scheduler,
                jobScheduler: _jobScheduler,
                parentJobKey: context.JobDetail.Key,
                childJobs: childJobs,
                settings: nextJobSettings,
                epocheNo: currentEpochResult.Entity.EpochNo+1,
                maxEpochNo: currentGeneticConfig.MaxEpoches,
                sessionId: _sessionId),
            matchers: GroupMatcher<JobKey>.GroupEquals($"Epoche-{currentEpochResult.Entity.EpochNo}-{_sessionId}"));
    }

    private void InizializeSettingValues(IJobExecutionContext context)
    {
        var jobDataMap = context.JobDetail.JobDataMap;

        _sessionId = jobDataMap.GetString(JobSettings.GeneticEpoche.SessionIdSettingName);
        _modelCount = int.Parse(jobDataMap.GetString(JobSettings.GeneticEpoche.ModelCountSettingName));

        if (string.IsNullOrEmpty(_sessionId))
        {
            Console.WriteLine("Error: _sessionId is null or empty.");
            throw new ArgumentNullException(nameof(_sessionId), "Session ID cannot be null or empty.");
        }

        var modelRangeConfig = jobDataMap.GetString(JobSettings.GeneticEpoche.ModelRangeConfigSettingName);
        if (!string.IsNullOrEmpty(modelRangeConfig))
        {
            _modelRangeConfig = JsonSerializer.Deserialize<ModelRangeConfig>(modelRangeConfig);
        }

        Console.WriteLine("==============> InizializeSettingValues - All settings are initialized properly.");
    }
}
