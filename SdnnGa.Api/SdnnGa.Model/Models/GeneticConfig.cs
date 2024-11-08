namespace SdnnGa.Model.Models;

public class GeneticConfig : BaseModel
{
    public int MaxEpoches { get; set; }

    public float MutationCof { get; set; }

    public string SessionId { get; set; }

    public Session Session { get; set; }
}