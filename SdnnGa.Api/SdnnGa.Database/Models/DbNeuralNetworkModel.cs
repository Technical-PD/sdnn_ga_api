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

    public int Type { get; set; }

    public int[] Metric { get; set; }

    public int Loss { get; set; }

    public float LossValue { get; set; }

    public float AccuracyValue { get; set; }

    [Required]
    [ForeignKey("Session")]
    public string SessionId { get; set; }

    [Required]
    [ForeignKey("Epoch")]
    public string EpocheId { get; set; }

    public DbSession Session { get; set; }

    public DbEpoch Epoch { get; set; }
}