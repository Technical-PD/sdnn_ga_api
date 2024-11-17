﻿namespace SdnnGa.Model.Models.Core.NNModel;

public class FitModelJobConfig
{
    public string XTrain { get; set; }
    public string YTrain { get; set; }
    public string ModelConfigJson { get; set; }
    public bool UseEarlyStopping { get; set; }
    public float MinDelta { get; set; }
    public bool Patience { get; set; }
    public bool IsLearnWithValidation { get; set; }
    public string Optimizer { get; set; }
    public string LossFunc { get; set; }
    public int Epochs { get; set; }
    public int BatchSize { get; set; }
    public string WeigthPath { get; set; }
}
