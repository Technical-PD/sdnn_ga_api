using System;

namespace SdnnGa.Model.Models.Core.NNModel;

/// <summary>
/// Поточний стан навчання моделі (зберігається лише в пам'яті, поки модель навчається).
/// </summary>
public class TrainingProgress
{
    public string ModelId { get; set; }

    public string ModelName { get; set; }

    public string SessionId { get; set; }

    public string EpochId { get; set; }

    public int Epoch { get; set; }

    public int Epochs { get; set; }

    public float? Loss { get; set; }

    public float? Accuracy { get; set; }

    public float? ValLoss { get; set; }

    public float? ValAccuracy { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
