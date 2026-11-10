using SdnnGa.Model.Services;
using System;
using System.Threading.Tasks;

namespace SdnnGa.Core.Jobs;

internal static class ModelFailureRecorder
{
    /// <summary>
    /// Записує помилку в модель (<c>FitError</c>). Модель з помилкою вважається завершеною
    /// і не бере участі у виборі найкращої моделі.
    /// </summary>
    public static async Task RecordAsync(INeuralNetworkModelService modelService, string modelId, string error)
    {
        if (string.IsNullOrEmpty(modelId))
        {
            return;
        }

        try
        {
            var modelResult = await modelService.GetModelByIdAsync(modelId, default, useTracking: false);

            if (modelResult.IsError || modelResult.Entity == null)
            {
                Console.WriteLine($"Could not record failure for model '{modelId}': {modelResult.Message}");
                return;
            }

            // Не перезаписуємо вже записану причину (наприклад, "Stopped by user").
            if (modelResult.Entity.FitError != null || modelResult.Entity.IsTrained)
            {
                return;
            }

            modelResult.Entity.IsTrained = false;
            modelResult.Entity.FitError = error;

            await modelService.UpdateModelAssync(modelResult.Entity);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not record failure for model '{modelId}': {ex.Message}");
        }
    }
}
