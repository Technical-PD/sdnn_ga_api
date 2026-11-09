namespace SdnnGa.Model.Models.Core.NNModel;

/// <summary>
/// Запит на навчання моделі. Назви властивостей — це ключі, які читає Worker/sdnnga_worker.py.
/// </summary>
public class FitModelJobConfig
{
    public bool IsLearnWithValidation { get; set; }

    public int Patience { get; set; }

    public bool UseEarlyStopping { get; set; }

    public float MinDelta { get; set; }

    public int BatchSize { get; set; }

    public int Epochs { get; set; }

    public string LossFunc { get; set; }

    public string ModelConfigJson { get; set; }

    public string Optimizer { get; set; }

    public string XTrain { get; set; }

    public string YTrain { get; set; }
}
