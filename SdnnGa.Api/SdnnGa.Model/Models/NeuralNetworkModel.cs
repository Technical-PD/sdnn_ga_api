﻿using SdnnGa.Model.Database.Models;

namespace SdnnGa.Model.Models;

public class NeuralNetworkModel : BaseModel
{
    public bool IsTrained { get; set; }

    public string WeightsFileName { get; set; }

    public string ModelConfigFileName { get; set; }

    public string FitHistory { get; set; }

    public float LossValue { get; set; }

    public float AccuracyValue { get; set; }

    public string SessionId { get; set; }

    public string EpocheId { get; set; }

    public Session Session { get; set; }

    public Epoch Epoch { get; set; }
}