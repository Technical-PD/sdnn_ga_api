using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using SdnnGa.Model.Database.Models;

namespace SdnnGa.Database.Models;

[Table("GeneticConfigs")]
public class DbGeneticConfig : BaseModel
{
    [Required]
    public int MaxEpoches { get; set; }

    [Required]
    public int CountOfModelsInEpoch { get; set; }

    [Required]
    public float ActFuncMutationProb { get; set; }

    [Required]
    public float CountOfNeuronMutationProb { get; set; }

    [Required]
    public float CountOfInternalLayerMutationProb { get; set; }

    [Required]
    public float StopAccValue { get; set; }

    [Required]
    public float StopLossValue { get; set; }

    [Required]
    public float BiasMutationProb { get; set; }

    [Required]
    public string SelectionCriterion { get; set; }

    [Required]
    [ForeignKey("Session")]
    public string SessionId { get; set; }

    public DbSession Session { get; set; }
}