using SdnnGa.Model.Models;
using SdnnGa.Model.Services.Models.ServiceResult;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace SdnnGa.Model.Services;

public interface IFitConfigService
{
    Task<ServiceResult<FitConfig>> AddFitConfigAssync(FitConfig fitConfig, CancellationToken cancellationToken = default);

    Task<ServiceResult<FitConfig>> UpdateFitConfigAssync(FitConfig fitConfig, CancellationToken cancellationToken = default);

    Task<ServiceResult<ICollection<FitConfig>>> GetAllFitConfigsAsync(CancellationToken cancellationToken = default);

    Task<ServiceResult<FitConfig>> GetFitConfigAsync(string id, CancellationToken cancellationToken = default);

    Task<ServiceResult<FitConfig>> GetFitConfigBySessionAsync(string sessionId, CancellationToken cancellationToken = default);
}
