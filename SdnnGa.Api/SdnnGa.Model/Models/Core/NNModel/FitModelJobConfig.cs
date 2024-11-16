namespace SdnnGa.Model.Models.Core.NNModel;

public class FitModelJobConfig
{
    public string XTrain { get; set; }
    public string YTrain { get; set; }
    public string ModelConfigJson { get; set; }
    public string CountOfLines { get; set; }
    public string CountOfInputs { get; set; }
    public string CountOfOutputs { get; set; }
    public string UseEarlyStopping { get; set; }
    public string MinDelta { get; set; }
    public string Patience { get; set; }
    public string IsLearnWithValidation { get; set; }
    public string Optimizer { get; set; }
    public string LossFunc { get; set; }
    public string Epochs { get; set; }
    public string BatchSize { get; set; }
}
