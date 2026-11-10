using Microsoft.Extensions.DependencyInjection;
using Quartz.Impl.Matchers;
using Quartz;
using SdnnGa.Core.Classes;
using SdnnGa.Core.Listeners;
using SdnnGa.Model.Constants;
using SdnnGa.Model.Core.Interfaces;
using SdnnGa.Model.Infrastructure.Interfaces.Quartz.Scheduler;
using SdnnGa.Model.Infrastructure.Interfaces.Training;
using SdnnGa.Model.Models.Core.GAConfigs;
using SdnnGa.Model.Models;
using SdnnGa.Model.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System;

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
    private readonly IServiceProvider _serviceProvider;
    private readonly IGeneticFlowStopRegistry _stopRegistry;

    private ModelRangeConfig _modelRangeConfig;
    private string _sessionId;

    public GeneticEpochJob(
        IJobScheduler jobScheduler,
        IEpochService epochService,
        ISessionService sessionService,
        IFitConfigService fitConfigService,
        INeuralNetworkModelService networkModelService,
        ISchedulerService schedulerService,
        IGeneticConfigService geneticConfigService,
        IServiceProvider serviceProvider,
        IGeneticFlowStopRegistry stopRegistry)
    {
        _stopRegistry = stopRegistry;
        _epochService = epochService;
        _sessionService = sessionService;
        _fitConfigService = fitConfigService;
        _networkModelService = networkModelService;
        _jobScheduler = jobScheduler;
        _schedulerService = schedulerService;
        _geneticConfigService = geneticConfigService;
        _serviceProvider = serviceProvider;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        InizializeSettingValues(context);

        if (_stopRegistry.IsStopRequested(_sessionId))
        {
            Console.WriteLine($"Genetic flow of session '{_sessionId}' was stopped: the next generation is not created.");
            return;
        }

        string modelConfigNetPath = string.Empty;
        string secondParentConfigNetPath = string.Empty;
        string operation = ModelOrigin.Random;
        string parentModelNames = null;

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

        // Батьків беремо з останнього покоління, де є успішно навчені моделі
        // (після зупинки чи збою в останньому поколінні може не бути жодної).
        var (latestEpoch, parentCandidates) = await FindParentEpochAsync(sessionEpochesResult.Entity);

        if (latestEpoch == null && sessionEpochesResult.Entity?.Any() == true)
        {
            if (_modelRangeConfig == null)
            {
                Console.WriteLine($"Session '{_sessionId}' has no trained models to breed from.");
                return;
            }

            Console.WriteLine($"Session '{_sessionId}' has no trained models yet: the new generation is generated randomly.");
        }

        if (latestEpoch != null)
        {

            var strategy = currentGeneticConfig.EvolutionStrategy ?? EvolutionStrategy.Mutation;
            NeuralNetworkModel firstParent;
            NeuralNetworkModel secondParent = null;

            if (EvolutionStrategy.UsesCrossover(strategy))
            {
                (firstParent, secondParent) = BestModelSelector.SelectPair(
                    parentCandidates,
                    currentGeneticConfig.FirstParentCriterion ?? SelectionCriterion.ByAccuracy,
                    currentGeneticConfig.SecondParentCriterion ?? SelectionCriterion.ByLoss);

                if (secondParent == null)
                {
                    // Для схрещування потрібно дві успішно навчені моделі.
                    Console.WriteLine($"Epoch {latestEpoch.EpochNo} of session '{_sessionId}' has fewer than two trained models: falling back to mutation.");
                    strategy = EvolutionStrategy.Mutation;
                    firstParent = BestModelSelector.Select(parentCandidates, currentGeneticConfig.SelectionCriterion);
                }
            }
            else
            {
                firstParent = BestModelSelector.Select(parentCandidates, currentGeneticConfig.SelectionCriterion);
            }

            modelConfigNetPath = firstParent?.ModelConfigDotNetFileName;
            secondParentConfigNetPath = secondParent?.ModelConfigDotNetFileName ?? string.Empty;
            operation = ModelOrigin.FromStrategy(strategy);
            parentModelNames = secondParent == null ? firstParent?.Name : $"{firstParent.Name}, {secondParent.Name}";

            Console.WriteLine($"Next generation of session '{_sessionId}': {operation} of {parentModelNames}.");

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

        for (int i = 0; i < currentGeneticConfig.CountOfModelsInEpoch; i++)
        {
            if (_stopRegistry.IsStopRequested(_sessionId))
            {
                Console.WriteLine($"Genetic flow of session '{_sessionId}' was stopped while generation was being created.");
                break;
            }

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
                Origin = operation,
                ParentModelNames = parentModelNames,
            };

            var createdModelResult = await _networkModelService.CreateModelAsync(newModel);

            var settingModelCreateJob = new Dictionary<string, string>
            {
                { JobSettings.CreateModel.ModelRangeConfigSettingName, modelConfigJson },
                { JobSettings.CreateModel.ModelConfigNetPathSettingName, modelConfigNetPath },
                { JobSettings.CreateModel.SessionIdSettingName, _sessionId },
                { JobSettings.CreateModel.EpocheNoSettingName, currentEpochResult.Entity.EpochNo.ToString() },
                { JobSettings.CreateModel.ModelIdSettingName, createdModelResult.Entity.Id },
                { JobSettings.CreateModel.ActFuncMutationProbSettingName, currentGeneticConfig.ActFuncMutationProb.ToString() },
                { JobSettings.CreateModel.CountOfInternalLayerMutationProbSettingName, currentGeneticConfig.CountOfInternalLayerMutationProb.ToString() },
                { JobSettings.CreateModel.CountOfNeuronMutationProbSettingName, currentGeneticConfig.CountOfNeuronMutationProb.ToString() },
                { JobSettings.CreateModel.BiasMutationProbSettingName, currentGeneticConfig.BiasMutationProb.ToString() },
                { JobSettings.CreateModel.ModelNoSettingName, i.ToString() },
                { JobSettings.CreateModel.OperationSettingName, operation },
                { JobSettings.CreateModel.SecondParentConfigNetPathSettingName, secondParentConfigNetPath },
                { JobSettings.CreateModel.CrossoverMethodSettingName, currentGeneticConfig.CrossoverMethod ?? CrossoverMethod.SinglePoint },
            };

            var settingModelFitJob = new Dictionary<string, string>
            {
                { JobSettings.FitModel.BatchSizeSettingName, "50" },
                { JobSettings.FitModel.EpocheNoSettingName, currentEpochResult.Entity.EpochNo.ToString() },
                { JobSettings.FitModel.EpochsSettingName, fitConfigResult.Entity.MaxEpoches.ToString() },
                { JobSettings.FitModel.IsLearnWithValidationSettingName, "True" },
                { JobSettings.FitModel.LossFuncSettingName, fitConfigResult.Entity.LossFunc },
                { JobSettings.FitModel.MinDeltaSettingName, "0.01" },
                { JobSettings.FitModel.ModelIdSettingName, createdModelResult.Entity.Id },
                { JobSettings.FitModel.OptimizerSettingName, fitConfigResult.Entity.FitMethod },
                { JobSettings.FitModel.PatienceSettingName, "10" },
                { JobSettings.FitModel.SessionIdSettingName, _sessionId },
                { JobSettings.FitModel.UseEarlyStoppingSettingName, "True" },
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
            { JobSettings.GeneticEpoche.ModelRangeConfigSettingName, JsonSerializer.Serialize(_modelRangeConfig) },
        };

        var listenerFactory = _serviceProvider.GetRequiredService<ParentJobCompletionListenerFactory>();

        var listener = listenerFactory.Create(
            scheduler: scheduler,
            jobScheduler: _jobScheduler,
            parentJobKey: context.JobDetail.Key,
            childJobs: childJobs,
            settings: nextJobSettings,
            epocheNo: currentEpochResult.Entity.EpochNo + 1,
            maxEpochNo: currentGeneticConfig.MaxEpoches,
            sessionId: _sessionId,
            epochId: currentEpochResult.Entity.Id,
            selectionCriterion: geneticConfigResult.Entity.SelectionCriterion,
            stopAccValue: geneticConfigResult.Entity.StopAccValue,
            stopLossValue: geneticConfigResult.Entity.StopLossValue);

        scheduler.ListenerManager.AddJobListener(
            jobListener: listener,
            matchers: GroupMatcher<JobKey>.GroupEquals($"Epoche-{currentEpochResult.Entity.EpochNo}-{_sessionId}"));
    }

    private async Task<(Epoch Epoch, List<NeuralNetworkModel> Models)> FindParentEpochAsync(IEnumerable<Epoch> epochs)
    {
        foreach (var epoch in (epochs ?? Enumerable.Empty<Epoch>()).OrderByDescending(epoch => epoch.EpochNo))
        {
            var modelsResult = await _networkModelService.GetModelByEpochIdAsync(epoch.Id, useTracking: false);

            if (!modelsResult.IsError && modelsResult.Entity.Any(model => model.IsTrained && model.FitError == null))
            {
                return (epoch, modelsResult.Entity);
            }
        }

        return (null, null);
    }

    private void InizializeSettingValues(IJobExecutionContext context)
    {
        var jobDataMap = context.JobDetail.JobDataMap;

        _sessionId = jobDataMap.GetString(JobSettings.GeneticEpoche.SessionIdSettingName);

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
