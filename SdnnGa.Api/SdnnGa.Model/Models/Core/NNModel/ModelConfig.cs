using System.Collections.Generic;

namespace SdnnGa.Model.Models.Core.NNModel;

public class ModelConfig
{
    public int[] InputShape { get; set; }

    public List<Layer> InternalLayers { get; set; }

    public Layer OutputLayer { get; set; }


}