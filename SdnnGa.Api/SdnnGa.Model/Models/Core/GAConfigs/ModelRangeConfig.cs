namespace SdnnGa.Model.Models.Core.GAConfigs;

public class ModelRangeConfig
{
    public int CountOfModels { get; set; }

    public int InputCount { get; set; }

    public int OutputCount { get; set; }

    public int CountOfInternalLayers { get; set; }

    public int NeuronsInLayerMin { get; set; }

    public int NeuronsInLayerMax { get; set; }

    public string ActivationFunc { get; set; }

    public bool? UseBias { get; set; }
}
