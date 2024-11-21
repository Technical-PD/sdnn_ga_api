using SdnnGa.Model.Models.Core.GAConfigs;
using SdnnGa.Model.Services.Models.ServiceResult;
using System.Threading.Tasks;
using System.Threading;

namespace SdnnGa.Model.Services;

public interface IGeneticService
{
    Task<ServiceResult> StartGeneticFlow(
        ModelRangeConfig modelRangeConfig,
        string sessionId,
        CancellationToken cancellationToken = default);
}
