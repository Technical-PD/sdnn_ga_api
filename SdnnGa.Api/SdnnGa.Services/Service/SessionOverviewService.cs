using Microsoft.EntityFrameworkCore;
using SdnnGa.Database;
using SdnnGa.Model.Constants;
using SdnnGa.Model.Infrastructure.Interfaces.Quartz.Scheduler;
using SdnnGa.Model.Models;
using SdnnGa.Model.Services;
using SdnnGa.Model.Services.Models.ServiceResult;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SdnnGa.Services.Service;

/// <summary>
/// Агрегує стан сесій для UI. Читає базу напряму (проєкції без FitHistory), щоб не тягнути
/// всі дані моделей через репозиторії.
/// </summary>
public class SessionOverviewService : ISessionOverviewService
{
    private readonly ApiDbContext _dbContext;
    private readonly ISchedulerService _schedulerService;

    public SessionOverviewService(ApiDbContext dbContext, ISchedulerService schedulerService)
    {
        _dbContext = dbContext;
        _schedulerService = schedulerService;
    }

    public async Task<ServiceResult<List<SessionOverview>>> GetOverviewsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var overviews = await BuildAsync(sessionId: null, includeEpochs: false, cancellationToken);
            return ServiceResult<List<SessionOverview>>.FromSuccess(overviews);
        }
        catch (Exception ex)
        {
            return ServiceResult<List<SessionOverview>>.FromUnexpectedError($"Unexpected error occured on building sessions overview. Message: '{ex.Message}'");
        }
    }

    public async Task<ServiceResult<SessionOverview>> GetOverviewAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(sessionId))
        {
            return ServiceResult<SessionOverview>.FromError("Session id is required.");
        }

        try
        {
            var overview = (await BuildAsync(sessionId, includeEpochs: true, cancellationToken)).FirstOrDefault();

            return overview == null
                ? ServiceResult<SessionOverview>.FromNotFound($"Session '{sessionId}' was not found.")
                : ServiceResult<SessionOverview>.FromSuccess(overview);
        }
        catch (Exception ex)
        {
            return ServiceResult<SessionOverview>.FromUnexpectedError($"Unexpected error occured on building session overview. Message: '{ex.Message}'");
        }
    }

    public async Task<bool> IsRunningAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var hasPendingModels = await _dbContext.NeuralNetworkModels
            .AnyAsync(model => model.SessionId == sessionId && !model.IsTrained && model.FitError == null, cancellationToken);

        return hasPendingModels || (await GetSessionsWithScheduledJobsAsync()).Contains(sessionId);
    }

    private async Task<List<SessionOverview>> BuildAsync(string sessionId, bool includeEpochs, CancellationToken cancellationToken)
    {
        var sessions = await _dbContext.Sessions.AsNoTracking()
            .Where(session => sessionId == null || session.Id == sessionId)
            .Select(session => new { session.Id, session.Name, session.Description, session.RecCreated, session.XTrainFileName, session.YTrainFileName })
            .ToListAsync(cancellationToken);

        if (sessions.Count == 0)
        {
            return new List<SessionOverview>();
        }

        var sessionsWithFitConfig = (await _dbContext.FitConfigs.AsNoTracking()
            .Where(config => sessionId == null || config.SessionId == sessionId)
            .Select(config => config.SessionId)
            .ToListAsync(cancellationToken))
            .ToHashSet();

        var geneticConfigs = (await _dbContext.DbGeneticConfigs.AsNoTracking()
            .Where(config => sessionId == null || config.SessionId == sessionId)
            .Select(config => new { config.SessionId, config.MaxEpoches, config.SelectionCriterion })
            .ToListAsync(cancellationToken))
            .GroupBy(config => config.SessionId)
            .ToDictionary(group => group.Key, group => group.First());

        var epochs = (await _dbContext.Epochs.AsNoTracking()
            .Where(epoch => sessionId == null || epoch.SessionId == sessionId)
            .Select(epoch => new { epoch.Id, epoch.SessionId, epoch.EpochNo, epoch.RecCreated })
            .ToListAsync(cancellationToken))
            .ToLookup(epoch => epoch.SessionId);

        var models = (await _dbContext.NeuralNetworkModels.AsNoTracking()
            .Where(model => sessionId == null || model.SessionId == sessionId)
            .Select(model => new ModelRow(model.Id, model.Name, model.SessionId, model.EpocheId, model.IsTrained, model.FitError, model.LossValue, model.AccuracyValue))
            .ToListAsync(cancellationToken))
            .ToLookup(model => model.SessionId);

        var sessionsWithJobs = await GetSessionsWithScheduledJobsAsync();

        var result = new List<SessionOverview>();

        foreach (var session in sessions.OrderByDescending(session => session.RecCreated))
        {
            var sessionModels = models[session.Id].ToList();
            geneticConfigs.TryGetValue(session.Id, out var geneticConfig);
            var selectionCriterion = geneticConfig?.SelectionCriterion ?? SelectionCriterion.ByLoss;

            var bestModel = SelectBest(sessionModels, selectionCriterion);

            var overview = new SessionOverview
            {
                Id = session.Id,
                Name = session.Name,
                Description = session.Description,
                RecCreated = session.RecCreated,
                HasData = !string.IsNullOrEmpty(session.XTrainFileName) && !string.IsNullOrEmpty(session.YTrainFileName),
                HasFitConfig = sessionsWithFitConfig.Contains(session.Id),
                HasGeneticConfig = geneticConfig != null,
                EpochCount = epochs[session.Id].Count(),
                MaxEpoches = geneticConfig?.MaxEpoches,
                SelectionCriterion = selectionCriterion,
                ModelCount = sessionModels.Count,
                TrainedModelCount = sessionModels.Count(IsSuccessful),
                FailedModelCount = sessionModels.Count(IsFailed),
                StoppedModelCount = sessionModels.Count(IsStopped),
                PendingModelCount = sessionModels.Count(IsPending),
                BestModelId = bestModel?.Id,
                BestModelName = bestModel?.Name,
                BestModelEpochId = bestModel?.EpochId,
                BestModelAccuracy = bestModel?.Accuracy,
                BestModelLoss = bestModel?.Loss,
            };

            overview.IsRunning = overview.PendingModelCount > 0 || sessionsWithJobs.Contains(session.Id);
            // Зупинена — якщо в останньому поколінні є моделі, скасовані користувачем.
            var lastEpochId = epochs[session.Id].OrderByDescending(epoch => epoch.EpochNo).FirstOrDefault()?.Id;
            var lastEpochStopped = lastEpochId != null && sessionModels.Any(model => model.EpochId == lastEpochId && IsStopped(model));

            overview.Status = overview.IsRunning ? SessionStatus.Running
                : lastEpochStopped ? SessionStatus.Stopped
                : overview.EpochCount > 0 ? SessionStatus.Finished
                : overview.HasData && overview.HasFitConfig && overview.HasGeneticConfig ? SessionStatus.Ready
                : SessionStatus.NeedsSetup;

            if (includeEpochs)
            {
                var modelsByEpoch = sessionModels.ToLookup(model => model.EpochId);

                overview.Epochs = epochs[session.Id]
                    .OrderBy(epoch => epoch.EpochNo)
                    .Select(epoch =>
                    {
                        var epochModels = modelsByEpoch[epoch.Id].ToList();
                        var successful = epochModels.Where(IsSuccessful).ToList();

                        return new EpochOverview
                        {
                            Id = epoch.Id,
                            EpochNo = epoch.EpochNo,
                            RecCreated = epoch.RecCreated,
                            ModelCount = epochModels.Count,
                            TrainedModelCount = successful.Count,
                            FailedModelCount = epochModels.Count(IsFailed),
                            StoppedModelCount = epochModels.Count(IsStopped),
                            PendingModelCount = epochModels.Count(IsPending),
                            BestAccuracy = successful.Count > 0 ? successful.Max(model => model.Accuracy) : null,
                            BestLoss = successful.Count > 0 ? successful.Min(model => model.Loss) : null,
                        };
                    })
                    .ToList();
            }

            result.Add(overview);
        }

        return result;
    }

    /// <summary>
    /// Сесії, для яких у Quartz є заплановані джоби (групи мають вигляд <c>Epoche-{n}-{sessionId}</c>).
    /// Покриває проміжки, коли нове покоління вже заплановано, а моделі ще не створені.
    /// </summary>
    private async Task<HashSet<string>> GetSessionsWithScheduledJobsAsync()
    {
        var scheduler = await _schedulerService.GetSchedulerAsync();
        var groupNames = await scheduler.GetJobGroupNames();

        // GUID сесії має фіксовану довжину (36 символів) і стоїть у кінці назви групи.
        return groupNames
            .Where(group => group.StartsWith("Epoche-", StringComparison.Ordinal) && group.Length > 36)
            .Select(group => group[^36..])
            .ToHashSet();
    }

    private static bool IsSuccessful(ModelRow model) => model.IsTrained && model.FitError == null;

    private static bool IsPending(ModelRow model) => !model.IsTrained && model.FitError == null;

    private static bool IsStopped(ModelRow model) =>
        model.FitError is ModelStatusMessages.StoppedByUser or ModelStatusMessages.Interrupted;

    private static bool IsFailed(ModelRow model) => model.FitError != null && !IsStopped(model);

    private static ModelRow SelectBest(IEnumerable<ModelRow> models, string selectionCriterion)
    {
        var successful = models.Where(IsSuccessful);

        return selectionCriterion == SelectionCriterion.ByAccuracy
            ? successful.OrderByDescending(model => model.Accuracy).ThenBy(model => model.Loss).FirstOrDefault()
            : successful.OrderBy(model => model.Loss).ThenByDescending(model => model.Accuracy).FirstOrDefault();
    }

    private sealed record ModelRow(string Id, string Name, string SessionId, string EpochId, bool IsTrained, string FitError, float Loss, float Accuracy);
}
