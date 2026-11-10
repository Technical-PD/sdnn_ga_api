using SdnnGa.Model.Models.Core.NNModel;
using System.Collections.Generic;

namespace SdnnGa.Model.Infrastructure.Interfaces.Training;

/// <summary>
/// Прогрес моделей, які навчаються зараз (у пам'яті, для індикаторів в UI).
/// </summary>
public interface ITrainingProgressTracker
{
    void Report(string modelId, string modelName, string sessionId, string epochId, FitProgress progress);

    void Complete(string modelId);

    IReadOnlyList<TrainingProgress> GetBySession(string sessionId);
}
