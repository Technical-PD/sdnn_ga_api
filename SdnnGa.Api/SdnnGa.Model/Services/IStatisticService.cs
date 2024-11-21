using SdnnGa.Model.Models;
using SdnnGa.Model.Services.Models.ServiceResult;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SdnnGa.Model.Services;

public interface IStatisticService
{
    Task<ServiceResult<List<NeuralNetworkModel>>> GetBestModelInSessionByLossAsync(string sessionId, CancellationToken cancellationToken = default);

    Task<ServiceResult<List<NeuralNetworkModel>>> GetBestModelInSessionByAccuracyAsync(string sessionId, CancellationToken cancellationToken = default);
}
