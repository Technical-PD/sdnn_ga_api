using SdnnGa.Model.Models;
using SdnnGa.Model.Models.Core.GAConfigs;
using SdnnGa.Model.Models.Core.NNModel;
using System.Collections.Generic;
using System;
using SdnnGa.Model.Core.Interfaces;

namespace SdnnGa.Core.Classes;

public class ModelGenerator : IModelGenerator
{
    public ModelConfig GenerateRandomModelConfig(ModelRangeConfig modelRangeConfig)
    {
        var random = new Random(DateTime.UtcNow.Millisecond);

        var internaLayers = new List<Layer>();

        for (int layerNumber = 0; layerNumber < modelRangeConfig.CountOfInternalLayers; layerNumber++)
        {
            var layer = new Layer()
            {
                ActivationFunc = modelRangeConfig.ActivationFunc,
                NeuronsCount = random.Next(modelRangeConfig.NeuronsInLayerMin, modelRangeConfig.NeuronsInLayerMax) + 1,
                UseBias = modelRangeConfig.UseBias.Value,
            };

            internaLayers.Add(layer);
        }

        var createConfig = new ModelConfig
        {
            InputShape = [modelRangeConfig.InputCount],
            InternalLayers = internaLayers,
            OutputLayer = new Layer
            {
                ActivationFunc = modelRangeConfig.ActivationFunc,
                NeuronsCount = modelRangeConfig.OutputCount,
                UseBias = false,
            },
        };

        return createConfig;
    }
}
