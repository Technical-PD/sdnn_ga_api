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
    public static NeuralNetworkModel Select(IEnumerable<NeuralNetworkModel> models, string selectionCriterion)
    {
        var trainedModels = (models ?? Enumerable.Empty<NeuralNetworkModel>())
            .Where(model => model.IsTrained && model.FitError == null);

        return selectionCriterion == SelectionCriterion.ByLoss
            ? trainedModels.OrderBy(model => model.LossValue).FirstOrDefault()
            : trainedModels.OrderByDescending(model => model.AccuracyValue).FirstOrDefault();
    }
}
