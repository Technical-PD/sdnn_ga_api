using SdnnGa.Model.Infrastructure.Interfaces.Training;
using SdnnGa.Model.Models.Core.NNModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SdnnGa.Infrastructure.Training;

/// <summary>
/// Прогрес моделей, що навчаються, у пам'яті. Запис з'являється, коли воркер починає навчання,
/// і зникає, коли FitModelJob завершується (успішно чи ні).
/// </summary>
public class TrainingProgressTracker : ITrainingProgressTracker
{
    private readonly ConcurrentDictionary<string, TrainingProgress> _progress = new();

    public void Report(string modelId, string modelName, string sessionId, string epochId, FitProgress progress)
    {
        var now = DateTime.UtcNow;

        _progress.AddOrUpdate(
            modelId,
            _ => Create(modelId, modelName, sessionId, epochId, progress, now, now),
            (_, existing) => Create(modelId, modelName, sessionId, epochId, progress, existing.StartedAt, now));
    }

    public void Complete(string modelId) => _progress.TryRemove(modelId, out _);

    public IReadOnlyList<TrainingProgress> GetBySession(string sessionId) =>
        _progress.Values.Where(item => item.SessionId == sessionId).ToList();

    private static TrainingProgress Create(string modelId, string modelName, string sessionId, string epochId, FitProgress progress, DateTime startedAt, DateTime updatedAt) => new()
    {
        ModelId = modelId,
        ModelName = modelName,
        SessionId = sessionId,
        EpochId = epochId,
        Epoch = progress.Epoch,
        Epochs = progress.Epochs,
        Loss = progress.Loss,
        Accuracy = progress.Accuracy,
        ValLoss = progress.ValLoss,
        ValAccuracy = progress.ValAccuracy,
        StartedAt = startedAt,
        UpdatedAt = updatedAt,
    };
}
