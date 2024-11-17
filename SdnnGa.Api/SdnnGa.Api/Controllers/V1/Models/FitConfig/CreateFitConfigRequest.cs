namespace SdnnGa.Api.Controllers.V1.Models.FitConfig;

public class CreateFitConfigRequest
{
    public int MaxEpoches { get; set; }

    public string FitMethod { get; set; }

    public string[] MetricFuncs { get; set; }

    public string LossFunc { get; set; }

    public float Alpha { get; set; }

    public string Name { get; set; }

}
