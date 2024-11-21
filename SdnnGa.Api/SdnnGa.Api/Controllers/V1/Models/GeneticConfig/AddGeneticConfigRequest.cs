namespace SdnnGa.Api.Controllers.V1.Models.GeneticConfig;

public class AddGeneticConfigRequest
{
    public string Name { get; set; }
    public int MaxEpoches { get; set; }
    public float MutationCof { get; set; }
}
