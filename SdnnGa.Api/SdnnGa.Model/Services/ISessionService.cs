using SdnnGa.Model.Models;
using SdnnGa.Model.Services.Models.ServiceResult;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SdnnGa.Model.Services;

public interface ISessionService
{
    Task<ServiceResult<Session>> GetSessionAsync(string id, CancellationToken cancellationToken = default);

    Task<ServiceResult<ICollection<Session>>> GetAllSessionsAsync(CancellationToken cancellationToken = default);

    Task<ServiceResult<Session>> CreateSessionAssync(Session session, CancellationToken cancellationToken = default);

    Task<ServiceResult<Session>> UpdateSessionAssync(Session session, CancellationToken cancellationToken = default);

    Task<ServiceResult<Session>> DeleteSessionAsync(string sessionId, CancellationToken cancellationToken = default);
}