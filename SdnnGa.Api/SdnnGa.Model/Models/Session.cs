﻿using SdnnGa.Model.Database.Models;
using System.Collections.Generic;

namespace SdnnGa.Model.Models;

public class Session : BaseModel
{
    public string Description { get; set; }

    public ICollection<NeuralNetworkModel> NeuralNetworkModels { get; set; }

    public ICollection<Epoch> Epochs { get; set; }

    public GeneticConfig GeneticConfig { get; set; }

    public DbFitConfig FitConfig { get; set; }
}