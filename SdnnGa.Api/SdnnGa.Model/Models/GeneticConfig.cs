using SdnnGa.Model.Database.Models;

namespace SdnnGa.Model.Models;

public class GeneticConfig : BaseModel
{
    public int MaxEpoches { get; set; }

    public float MutationCof { get; set; }

    public string SelectionCriterion { get; set; }

    public string SessionId { get; set; }
}