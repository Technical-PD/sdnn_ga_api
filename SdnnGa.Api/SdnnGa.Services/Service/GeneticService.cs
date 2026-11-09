using SdnnGa.Model.Constants;
using SdnnGa.Model.Core.Interfaces;
using SdnnGa.Model.Infrastructure.Interfaces.Quartz.Scheduler;
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

    public GeneticService(
        IJobScheduler jobScheduler,
        IModelWorkerClient workerClient,
        ISessionService sessionService,
        IFitConfigService fitConfigService,
        IGeneticConfigService geneticConfigService)
    {
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

            try
            {
                await _workerClient.EnsureStartedAsync(cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                return ServiceResult.FromError($"The local training worker is not available: {ex.Message}");
            }

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
