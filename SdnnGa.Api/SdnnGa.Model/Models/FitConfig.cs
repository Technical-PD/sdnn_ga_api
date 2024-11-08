﻿namespace SdnnGa.Model.Models;

public class DbFitConfig 
{
    public int MaxEpoches { get; set; }

    public int FitMethod { get; set; }

    public int[] MetricFuncs { get; set; }

    public int LossFunc { get; set; }

    public float Alpha { get; set; }

    public string XDataFileName { get; set; }

    public string YDataFileName { get; set; }

    public string SessionId { get; set; }

    public Session Session { get; set; }
}