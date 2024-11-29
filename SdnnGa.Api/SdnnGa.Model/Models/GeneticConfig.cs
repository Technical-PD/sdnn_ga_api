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

    public string SessionId { get; set; }
}