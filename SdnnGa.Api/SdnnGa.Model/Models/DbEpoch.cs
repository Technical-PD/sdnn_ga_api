using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SdnnGa.Model.Models;

[Table("Epochs")]
public class DbEpoch : BaseModel
{
    [Required]
    public int ModelCount { get; set; }

    [Required]
    public bool IsTrained { get; set; } = false;

    [Required]
    public int EpochNo { get; set; }

    [Required]
    [ForeignKey("Session")]
    public string SessionId { get; set; }

    public ICollection<DbNeuralNetworkModel> NeuralNetworkModel { get; set; }

    public DbSession Session { get; set; }
}