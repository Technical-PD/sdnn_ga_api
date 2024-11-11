namespace SdnnGa.Model.Models.Core.NNModel;

public class Layer
{
    public int NeuronsCount { get; set; }

    public string ActivationFunc { get; set; }

    public bool UseBias { get; set; } = false;
}
