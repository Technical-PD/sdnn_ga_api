using SdnnGa.Model.Models.Core.GAConfigs;
using SdnnGa.Model.Services.Models.ServiceResult;
using System.Threading.Tasks;
using System.Threading;

namespace SdnnGa.Model.Services;

public interface IGeneticService
{
    Task<ServiceResult> StartGeneticFlow(
        ModelRangeConfig modelRangeConfig,
        CompileConfig compileConfig,
        TrainConfig trainConfig,
        char separator,
        string sessionId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult> TestLearningAsync(string sessionId, string modelId, CancellationToken cancellationToken = default);
}
