namespace SdnnGa.Model.Models.Core.NNModel;

public class FitResult
{
    public float Loss { get; set; }
    public float Accuracy { get; set; }
    public History History { get; set; }
}
