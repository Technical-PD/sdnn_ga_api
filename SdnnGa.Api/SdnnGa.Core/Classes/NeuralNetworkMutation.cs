using SdnnGa.Model.Models.Core.NNModel;
using System.Collections.Generic;
using System;

namespace SdnnGa.Core.Classes;

public static class NeuralNetworkMutation
{
    private static Random random = new Random(DateTime.UtcNow.Millisecond);
    private static List<string> activationFunctions = new List<string> { "relu", "sigmoid", "tanh", "softmax", "linear" };

    public static ModelConfig Mutate(ModelConfig model, float mutateCof)
    {
        // Create a copy of the model to apply mutations
        var mutatedModel = new ModelConfig
        {
            InputShape = (int[])model.InputShape.Clone(),
            InternalLayers = new List<Layer>(model.InternalLayers),
            OutputLayer = new Layer
            {
                NeuronsCount = model.OutputLayer.NeuronsCount,
                ActivationFunc = model.OutputLayer.ActivationFunc,
                UseBias = model.OutputLayer.UseBias
            }
        };

        // Mutate the output layer's activation function with a small probability
        if (random.NextDouble() < mutateCof * 0.5)
        {
            mutatedModel.OutputLayer.ActivationFunc = GetRandomActivationFunction(mutatedModel.OutputLayer.ActivationFunc);
        }

        // Mutate the internal layers
        for (int i = 0; i < mutatedModel.InternalLayers.Count; i++)
        {
            // Mutate layer neurons count with some probability
            if (random.NextDouble() < mutateCof)
            {
                int minNeurons = Math.Max(1, mutatedModel.InternalLayers[i].NeuronsCount - 10);
                int maxNeurons = mutatedModel.InternalLayers[i].NeuronsCount + 10;
                mutatedModel.InternalLayers[i].NeuronsCount = random.Next(minNeurons, maxNeurons + 1);
            }

            // Mutate layer activation function with some probability
            if (random.NextDouble() < mutateCof * 0.5)
            {
                mutatedModel.InternalLayers[i].ActivationFunc = GetRandomActivationFunction(mutatedModel.InternalLayers[i].ActivationFunc);
            }
        }

        // Mutate the number of internal layers with a small probability
        if (random.NextDouble() < mutateCof * 0.75)
        {
            if (random.NextDouble() < 0.5 && mutatedModel.InternalLayers.Count > 1) // Remove a layer
            {
                int removeIndex = random.Next(mutatedModel.InternalLayers.Count);
                mutatedModel.InternalLayers.RemoveAt(removeIndex);
            }
            else // Add a new layer
            {
                int averageNeurons = GetAverageNeurons(mutatedModel.InternalLayers);
                var newLayer = new Layer
                {
                    NeuronsCount = random.Next(Math.Max(1, averageNeurons - 5), averageNeurons + 6),
                    ActivationFunc = activationFunctions[random.Next(activationFunctions.Count)],
                    UseBias = random.NextDouble() < 0.5
                };
                int insertIndex = random.Next(mutatedModel.InternalLayers.Count + 1);
                mutatedModel.InternalLayers.Insert(insertIndex, newLayer);
            }
        }

        return mutatedModel;
    }

    private static string GetRandomActivationFunction(string currentFunction)
    {
        string newFunction;
        do
        {
            newFunction = activationFunctions[random.Next(activationFunctions.Count)];
        } while (newFunction == currentFunction);

        return newFunction;
    }

    private static int GetAverageNeurons(List<Layer> layers)
    {
        if (layers.Count == 0) return 10;
        int totalNeurons = 0;
        foreach (var layer in layers)
        {
            totalNeurons += layer.NeuronsCount;
        }
        return totalNeurons / layers.Count;
    }
}
