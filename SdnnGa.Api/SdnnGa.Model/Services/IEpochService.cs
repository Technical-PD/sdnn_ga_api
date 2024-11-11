using SdnnGa.Model.Models;
using SdnnGa.Model.Services.Models.ServiceResult;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace SdnnGa.Model.Services;

public interface IEpochService
{
    Task<ServiceResult<IEnumerable<Epoch>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<ServiceResult<IEnumerable<Epoch>>> GetAllBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default);

    Task<ServiceResult<Epoch>> GetByIdAsync(string epochId, CancellationToken cancellationToken = default);

    Task<ServiceResult<Epoch>> AddEpochAsync(string sessionId, CancellationToken cancellationToken = default);
}