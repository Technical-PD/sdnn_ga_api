using SdnnGa.Model.Models;
using SdnnGa.Model.Services.Models.ServiceResult;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace SdnnGa.Model.Services;

public interface INeuralNetworkModelService
{
    Task<ServiceResult<NeuralNetworkModel>> CreateModelAsync(NeuralNetworkModel neuralNetworkModel, CancellationToken cancellationToken = default);

    Task<ServiceResult<NeuralNetworkModel>> UpdateModelAssync(NeuralNetworkModel model, CancellationToken cancellationToken = default);

    Task<ServiceResult<NeuralNetworkModel>> GetModelByIdAsync(string modelId, CancellationToken cancellationToken = default);

    Task<ServiceResult<List<NeuralNetworkModel>>> GetModelByEpochIdAsync(string epochId, bool useTracking = true, CancellationToken cancellationToken = default);

    Task<ServiceResult<List<NeuralNetworkModel>>> GetAllModelsAsync(CancellationToken cancellationToken = default);
}
