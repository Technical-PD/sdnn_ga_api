using SdnnGa.Model.Database.Models;
using System.ComponentModel.DataAnnotations;

namespace SdnnGa.Model.Models;

public class GeneticConfig : BaseModel
{
    public int MaxEpoches { get; set; }

    public int CountOfModelsInEpoch { get; set; }

    public float ActFuncMutationProb { get; set; }

    public float CountOfNeuronMutationProb { get; set; }

    public float CountOfInternalLayerMutationProb { get; set; }

    public float BiasMutationProb { get; set; }

    public float StopAccValue { get; set; }

    public float StopLossValue { get; set; }

    public string SelectionCriterion { get; set; }

    /// <summary><see cref="Constants.EvolutionStrategy"/>; <c>null</c> — мутація (конфіги до появи схрещування).</summary>
    public string EvolutionStrategy { get; set; }

    /// <summary><see cref="Constants.CrossoverMethod"/>.</summary>
    public string CrossoverMethod { get; set; }

    /// <summary>Критерій вибору першого батька для схрещування (<see cref="Constants.SelectionCriterion"/>).</summary>
    public string FirstParentCriterion { get; set; }

    /// <summary>Критерій вибору другого батька для схрещування (<see cref="Constants.SelectionCriterion"/>).</summary>
    public string SecondParentCriterion { get; set; }

    public string SessionId { get; set; }
}