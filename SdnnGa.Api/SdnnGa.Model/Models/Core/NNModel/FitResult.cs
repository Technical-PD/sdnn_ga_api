namespace SdnnGa.Model.Models.Core.NNModel;

public class FitResult
{
    public float Loss { get; set; }

    public float Accuracy { get; set; }

    public History History { get; set; }

    /// <summary>Ваги навченої моделі (.weights.h5), закодовані в base64.</summary>
    public string Weights { get; set; }
}
