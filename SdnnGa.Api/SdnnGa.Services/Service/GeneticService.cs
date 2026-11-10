using SdnnGa.Model.Constants;
using SdnnGa.Model.Core.Interfaces;
using SdnnGa.Model.Infrastructure.Interfaces.Quartz.Scheduler;
using SdnnGa.Model.Infrastructure.Interfaces.Training;
using Quartz;
using Quartz.Impl.Matchers;
using System.Linq;
using SdnnGa.Model.Infrastructure.Interfaces.Worker;
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
    private readonly IModelWorkerClient _workerClient;
    private readonly ISessionService _sessionService;
    private readonly IFitConfigService _fitConfigService;
    private readonly IGeneticConfigService _geneticConfigService;
    private readonly ISchedulerService _schedulerService;
    private readonly IGeneticFlowStopRegistry _stopRegistry;
    private readonly ISessionOverviewService _sessionOverviewService;
    private readonly IEpochService _epochService;
    private readonly INeuralNetworkModelService _modelService;

    public GeneticService(
        IJobScheduler jobScheduler,
        IModelWorkerClient workerClient,
        ISessionService sessionService,
        IFitConfigService fitConfigService,
        IGeneticConfigService geneticConfigService,
        ISchedulerService schedulerService,
        IGeneticFlowStopRegistry stopRegistry,
        ISessionOverviewService sessionOverviewService,
        IEpochService epochService,
        INeuralNetworkModelService modelService)
    {
        _schedulerService = schedulerService;
        _stopRegistry = stopRegistry;
        _sessionOverviewService = sessionOverviewService;
        _epochService = epochService;
        _modelService = modelService;
        _jobScheduler = jobScheduler;
        _workerClient = workerClient;
        _sessionService = sessionService;
        _fitConfigService = fitConfigService;
        _geneticConfigService = geneticConfigService;
    }

    public async Task<ServiceResult> StartGeneticFlow(
        ModelRangeConfig modelRangeConfig,
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var validationError = await ValidateAsync(modelRangeConfig, sessionId, cancellationToken);

            if (validationError != null)
            {
                return ServiceResult.FromError(validationError);
            }

            if (await _sessionOverviewService.IsRunningAsync(sessionId, cancellationToken))
            {
                return ServiceResult.FromError("The search is already running for this session.");
            }

            try
            {
                await _workerClient.EnsureStartedAsync(cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                return ServiceResult.FromError($"The local training worker is not available: {ex.Message}");
            }

            // Новий запуск після зупинки.
            _stopRegistry.Clear(sessionId);

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

    public async Task<ServiceResult> StopGeneticFlow(string sessionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(sessionId))
        {
            return ServiceResult.FromError("Session id is required.");
        }

        try
        {
            // 1. Прапорець: GeneticEpochJob, CreateModelJob, FitModelJob і слухач перевіряють його.
            _stopRegistry.RequestStop(sessionId);

            var scheduler = await _schedulerService.GetSchedulerAsync();
            var groupSuffix = $"-{sessionId}";

            // 2. Скасовуємо джоби, що виконуються зараз (CancellationToken; воркер зупиняє поточне навчання).
            foreach (var executing in await scheduler.GetCurrentlyExecutingJobs(cancellationToken))
            {
                if (executing.JobDetail.Key.Group.EndsWith(groupSuffix, StringComparison.Ordinal))
                {
                    await scheduler.Interrupt(executing.JobDetail.Key, cancellationToken);
                }
            }

            // 3. Видаляємо ще не виконані джоби сесії.
            foreach (var group in await scheduler.GetJobGroupNames(cancellationToken))
            {
                if (!group.EndsWith(groupSuffix, StringComparison.Ordinal))
                {
                    continue;
                }

                foreach (var jobKey in await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(group), cancellationToken))
                {
                    await scheduler.DeleteJob(jobKey, cancellationToken);
                }
            }

            // 4. Слухачі завершення поколінь сесії більше не потрібні.
            var epochs = await _epochService.GetAllBySessionIdAsync(sessionId, cancellationToken);
            foreach (var epoch in epochs.Entity ?? Enumerable.Empty<Model.Models.Epoch>())
            {
                scheduler.ListenerManager.RemoveJobListener($"ParentJobCompletionListener-{epoch.Id}");
            }

            // 5. Моделі, що ще чекали на створення чи навчання, позначаємо як зупинені (навчені залишаються).
            var stopped = 0;
            foreach (var epoch in epochs.Entity ?? Enumerable.Empty<Model.Models.Epoch>())
            {
                var models = await _modelService.GetModelByEpochIdAsync(epoch.Id, false, cancellationToken);
                foreach (var model in (models.Entity ?? new List<Model.Models.NeuralNetworkModel>()).Where(model => !model.IsTrained && model.FitError == null))
                {
                    model.FitError = ModelStatusMessages.StoppedByUser;
                    await _modelService.UpdateModelAssync(model, cancellationToken);
                    stopped++;
                }
            }

            Console.WriteLine($"Genetic flow of session '{sessionId}' stopped by the user ({stopped} unfinished model(s) cancelled).");
            return ServiceResult.FromSuccess();
        }
        catch (Exception ex)
        {
            return ServiceResult.FromUnexpectedError($"Unexpected error occured on stopping GeneticFlow. Message: '{ex.Message}'");
        }
    }

    /// <returns>Текст помилки або <c>null</c>, якщо сесія готова до запуску.</returns>
    private async Task<string> ValidateAsync(ModelRangeConfig modelRangeConfig, string sessionId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(sessionId))
        {
            return "Session id is required.";
        }

        if (modelRangeConfig == null || modelRangeConfig.InputCount <= 0 || modelRangeConfig.OutputCount <= 0)
        {
            return "Input count and output count must be greater than zero.";
        }

        if (modelRangeConfig.NeuronsInLayerMin < 0 || modelRangeConfig.NeuronsInLayerMax < modelRangeConfig.NeuronsInLayerMin)
        {
            return "Neurons range is invalid (min must be ≥ 0 and max must be ≥ min).";
        }

        var session = await _sessionService.GetSessionAsync(sessionId, cancellationToken);

        if (session.IsError || session.Entity == null)
        {
            return $"Session '{sessionId}' was not found.";
        }

        if (string.IsNullOrEmpty(session.Entity.XTrainFileName) || string.IsNullOrEmpty(session.Entity.YTrainFileName))
        {
            return "Upload X and Y training data for this session first.";
        }

        var fitConfig = await _fitConfigService.GetFitConfigBySessionAsync(sessionId, cancellationToken);

        if (fitConfig.IsError || fitConfig.Entity == null)
        {
            return "Add a fit configuration for this session first.";
        }

        var geneticConfig = await _geneticConfigService.GetAllBySessionIdAsync(sessionId, cancellationToken);

        if (geneticConfig.IsError || geneticConfig.Entity == null)
        {
            return "Add a genetic configuration for this session first.";
        }

        return null;
    }
}
