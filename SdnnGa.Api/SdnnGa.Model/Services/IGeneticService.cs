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

    /// <summary>Зупиняє генетичний пошук сесії: скасовує поточні джоби, не запускає нове покоління.</summary>
    Task<ServiceResult> StopGeneticFlow(string sessionId, CancellationToken cancellationToken = default);
}
