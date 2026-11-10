using SdnnGa.Model.Constants;
using SdnnGa.Model.Models.Core.NNModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SdnnGa.Core.Classes;

/// <summary>
/// Схрещування двох батьківських архітектур. Схрещуються лише приховані шари:
/// розмір входу та кількість нейронів вихідного шару завжди залишаються незмінними.
/// </summary>
public static class NeuralNetworkCrossover
{
    private static Random Rng => Random.Shared;

    public static ModelConfig Cross(ModelConfig parentA, ModelConfig parentB, string method)
    {
        ArgumentNullException.ThrowIfNull(parentA);
        ArgumentNullException.ThrowIfNull(parentB);

        if (!parentA.InputShape.SequenceEqual(parentB.InputShape) || parentA.OutputLayer.NeuronsCount != parentB.OutputLayer.NeuronsCount)
        {
            throw new InvalidOperationException("Parents must have the same input size and the same number of output neurons.");
        }

        var layersA = parentA.InternalLayers ?? new List<Layer>();
        var layersB = parentB.InternalLayers ?? new List<Layer>();

        var hidden = method switch
        {
            CrossoverMethod.Uniform => Uniform(layersA, layersB),
            CrossoverMethod.Blend => Blend(layersA, layersB),
            _ => SinglePoint(layersA, layersB),
        };

        // Мережа повинна мати хоча б один прихований шар.
        if (hidden.Count == 0)
        {
            var source = layersA.Concat(layersB).ToList();
            hidden.Add(source.Count > 0
                ? Copy(source[Rng.Next(source.Count)])
                : new Layer { NeuronsCount = 8, ActivationFunc = parentA.OutputLayer.ActivationFunc, UseBias = true });
        }

        // Активація та bias виходу успадковуються від випадкового батька, кількість нейронів — фіксована.
        var outputSource = Rng.Next(2) == 0 ? parentA.OutputLayer : parentB.OutputLayer;

        return new ModelConfig
        {
            InputShape = (int[])parentA.InputShape.Clone(),
            InternalLayers = hidden,
            OutputLayer = new Layer
            {
                NeuronsCount = parentA.OutputLayer.NeuronsCount,
                ActivationFunc = outputSource.ActivationFunc,
                UseBias = outputSource.UseBias,
            },
        };
    }

    /// <summary>
    /// Одноточкове: шари батька A до випадкової точки розрізу + шари батька B після своєї точки розрізу.
    /// Кількість шарів дитини може відрізнятися від обох батьків.
    /// </summary>
    private static List<Layer> SinglePoint(List<Layer> layersA, List<Layer> layersB)
    {
        var cutA = Rng.Next(layersA.Count + 1);
        var cutB = Rng.Next(layersB.Count + 1);

        return layersA.Take(cutA).Concat(layersB.Skip(cutB)).Select(Copy).ToList();
    }

    /// <summary>
    /// Рівномірне: довжина — від випадкового батька, кожен шар — від випадкового батька, який має шар на цій позиції.
    /// </summary>
    private static List<Layer> Uniform(List<Layer> layersA, List<Layer> layersB)
    {
        var length = Rng.Next(2) == 0 ? layersA.Count : layersB.Count;
        var child = new List<Layer>(length);

        for (int i = 0; i < length; i++)
        {
            var candidates = new[] { layersA.ElementAtOrDefault(i), layersB.ElementAtOrDefault(i) }.Where(layer => layer != null).ToArray();
            child.Add(Copy(candidates[Rng.Next(candidates.Length)]));
        }

        return child;
    }

    /// <summary>
    /// Змішування: кількість нейронів — випадкова точка між розмірами шарів батьків,
    /// активація та bias — від випадкового батька.
    /// </summary>
    private static List<Layer> Blend(List<Layer> layersA, List<Layer> layersB)
    {
        var length = Rng.Next(Math.Min(layersA.Count, layersB.Count), Math.Max(layersA.Count, layersB.Count) + 1);
        var child = new List<Layer>(length);

        for (int i = 0; i < length; i++)
        {
            var a = layersA.ElementAtOrDefault(i);
            var b = layersB.ElementAtOrDefault(i);

            if (a == null || b == null)
            {
                child.Add(Copy(a ?? b));
                continue;
            }

            var weight = Rng.NextDouble();
            var traits = Rng.Next(2) == 0 ? a : b;

            child.Add(new Layer
            {
                NeuronsCount = Math.Max(1, (int)Math.Round(a.NeuronsCount + (b.NeuronsCount - a.NeuronsCount) * weight)),
                ActivationFunc = traits.ActivationFunc,
                UseBias = traits.UseBias,
            });
        }

        return child;
    }

    private static Layer Copy(Layer layer) => new()
    {
        NeuronsCount = layer.NeuronsCount,
        ActivationFunc = layer.ActivationFunc,
        UseBias = layer.UseBias,
    };
}
