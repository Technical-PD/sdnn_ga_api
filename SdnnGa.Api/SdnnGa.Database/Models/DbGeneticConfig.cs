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

    /// <summary><see cref="SdnnGa.Model.Constants.EvolutionStrategy"/>; <c>null</c> — мутація (конфіги до появи схрещування).</summary>
    public string EvolutionStrategy { get; set; }

    /// <summary><see cref="SdnnGa.Model.Constants.CrossoverMethod"/>.</summary>
    public string CrossoverMethod { get; set; }

    /// <summary>Критерій вибору першого батька для схрещування (<see cref="SdnnGa.Model.Constants.SelectionCriterion"/>).</summary>
    public string FirstParentCriterion { get; set; }

    /// <summary>Критерій вибору другого батька для схрещування (<see cref="SdnnGa.Model.Constants.SelectionCriterion"/>).</summary>
    public string SecondParentCriterion { get; set; }

    [Required]
    [ForeignKey("Session")]
    public string SessionId { get; set; }

    public DbSession Session { get; set; }
}