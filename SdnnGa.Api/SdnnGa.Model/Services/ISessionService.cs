using SdnnGa.Model.Models;
using SdnnGa.Model.Services.Models.ServiceResult;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SdnnGa.Model.Services;

public interface ISessionService
{
    /// <summary>
    /// Gets the session asynchronous.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<ServiceResult<Session>> GetSessionAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all sessions asynchronous.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<ServiceResult<ICollection<Session>>> GetAllSessionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates the session assync.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<ServiceResult<Session>> CreateSessionAssync(Session session, CancellationToken cancellationToken = default);
}