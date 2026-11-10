using SdnnGa.Model.Database.Models;

namespace SdnnGa.Model.Models;

public class NeuralNetworkModel : BaseModel
{
    public bool IsTrained { get; set; }

    public string WeightsFileName { get; set; }

    public string ModelConfigFileName { get; set; }

    public string ModelConfigDotNetFileName { get; set; }

    public string FitHistory { get; set; }

    public float LossValue { get; set; }

    public float AccuracyValue { get; set; }

    /// <summary>Помилка створення/навчання моделі; <c>null</c>, якщо все пройшло успішно.</summary>
    public string FitError { get; set; }

    /// <summary>Як отримана модель: <see cref="SdnnGa.Model.Constants.ModelOrigin"/>.</summary>
    public string Origin { get; set; }

    /// <summary>Назви батьківських моделей (через кому).</summary>
    public string ParentModelNames { get; set; }

    /// <summary>Модель або успішно навчена, або завершилась з помилкою — більше не очікує обробки.</summary>
    public bool IsFinished => IsTrained || FitError != null;

    public string SessionId { get; set; }

    public string EpocheId { get; set; }
}