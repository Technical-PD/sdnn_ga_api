namespace SdnnGa.Api.Controllers.V1.Models.GeneticConfig;

public class AddGeneticConfigRequest
{
    public string Name { get; set; }

    public int MaxEpoches { get; set; }

    public int CountOfModelsInEpoch { get; set; }

    public float ActFuncMutationProb { get; set; }

    public float CountOfNeuronMutationProb { get; set; }

    public float CountOfInternalLayerMutationProb { get; set; }

    public float StopAccValue { get; set; }

    public float StopLossValue { get; set; }

    public float BiasMutationProb { get; set; }

    public string SelectionCriterion { get; set; }

    public string EvolutionStrategy { get; set; }

    public string CrossoverMethod { get; set; }

    public string FirstParentCriterion { get; set; }

    public string SecondParentCriterion { get; set; }
}
