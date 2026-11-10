namespace SdnnGa.Model.Models.Core.NNModel;

/// <summary>
/// Прогрес навчання однієї моделі, який воркер надсилає після кожної епохи.
/// </summary>
public class FitProgress
{
    /// <summary>Скільки епох завершено (0 — навчання щойно почалося).</summary>
    public int Epoch { get; set; }

    /// <summary>Максимальна кількість епох (рання зупинка може завершити навчання раніше).</summary>
    public int Epochs { get; set; }

    public float? Loss { get; set; }

    public float? Accuracy { get; set; }

    public float? ValLoss { get; set; }

    public float? ValAccuracy { get; set; }
}
