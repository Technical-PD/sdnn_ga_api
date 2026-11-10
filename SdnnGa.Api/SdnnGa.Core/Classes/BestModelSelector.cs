using SdnnGa.Model.Constants;
using SdnnGa.Model.Models;
using System.Collections.Generic;
using System.Linq;

namespace SdnnGa.Core.Classes;

public static class BestModelSelector
{
    /// <summary>
    /// Найкраща успішно навчена модель за критерієм відбору
    /// (<see cref="SelectionCriterion.ByLoss"/> — мінімальний loss, інакше — максимальна accuracy).
    /// Моделі з помилкою не враховуються. Повертає <c>null</c>, якщо успішних моделей немає.
    /// </summary>
    public static NeuralNetworkModel Select(IEnumerable<NeuralNetworkModel> models, string selectionCriterion) =>
        Rank(Successful(models), selectionCriterion).FirstOrDefault();

    /// <summary>
    /// Пара батьків для схрещування: найкраща модель за першим критерієм і найкраща з решти — за другим.
    /// Якщо критерії однакові — це дві найкращі моделі за ним. <c>Second</c> = <c>null</c>, якщо успішних моделей менше двох.
    /// </summary>
    public static (NeuralNetworkModel First, NeuralNetworkModel Second) SelectPair(
        IEnumerable<NeuralNetworkModel> models,
        string firstCriterion,
        string secondCriterion)
    {
        var successful = Successful(models).ToList();

        var first = Rank(successful, firstCriterion).FirstOrDefault();
        if (first == null)
        {
            return (null, null);
        }

        var second = Rank(successful, secondCriterion).FirstOrDefault(model => model.Id != first.Id);
        return (first, second);
    }

    private static IEnumerable<NeuralNetworkModel> Successful(IEnumerable<NeuralNetworkModel> models) =>
        (models ?? Enumerable.Empty<NeuralNetworkModel>()).Where(model => model.IsTrained && model.FitError == null);

    private static IEnumerable<NeuralNetworkModel> Rank(IEnumerable<NeuralNetworkModel> models, string criterion) =>
        criterion == SelectionCriterion.ByLoss
            ? models.OrderBy(model => model.LossValue).ThenByDescending(model => model.AccuracyValue)
            : models.OrderByDescending(model => model.AccuracyValue).ThenBy(model => model.LossValue);
}
