using SdnnGa.Model.Models;
using SdnnGa.Model.Services.Models.ServiceResult;
using System.Threading.Tasks;
using System.Threading;
namespace SdnnGa.Model.Services;

public interface INeuralNetworkModelService
{
    Task<ServiceResult<NeuralNetworkModel>> CreateModelAsync(NeuralNetworkModel neuralNetworkModel, CancellationToken cancellationToken = default);
}
