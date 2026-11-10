using SdnnGa.Model.Database.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SdnnGa.Database.Models;

[Table("NeuralNetworkModel")]
public class DbNeuralNetworkModel : BaseModel
{
    [Required]
    public bool IsTrained { get; set; }

    [MaxLength(255)]
    public string WeightsFileName { get; set; }

    [MaxLength(255)]
    public string ModelConfigFileName { get; set; }

    [MaxLength(255)]
    public string ModelConfigDotNetFileName { get; set; }

    public string FitHistory {  get; set; }

    public float LossValue { get; set; }

    public float AccuracyValue { get; set; }

    /// <summary>Помилка створення/навчання моделі; <c>null</c>, якщо все пройшло успішно.</summary>
    public string FitError { get; set; }

    /// <summary>Як отримана модель: <see cref="SdnnGa.Model.Constants.ModelOrigin"/>.</summary>
    public string Origin { get; set; }

    /// <summary>Назви батьківських моделей (через кому).</summary>
    public string ParentModelNames { get; set; }

    [Required]
    [ForeignKey("Session")]
    public string SessionId { get; set; }

    [Required]
    [ForeignKey("Epoch")]
    public string EpocheId { get; set; }

    public DbSession Session { get; set; }

    public DbEpoch Epoch { get; set; }
}