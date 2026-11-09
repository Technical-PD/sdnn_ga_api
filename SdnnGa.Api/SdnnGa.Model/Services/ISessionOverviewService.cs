using SdnnGa.Model.Models;
using SdnnGa.Model.Services.Models.ServiceResult;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SdnnGa.Model.Services;

public interface ISessionOverviewService
{
    /// <summary>Огляд усіх сесій (без підсумків по поколіннях), новіші — першими.</summary>
    Task<ServiceResult<List<SessionOverview>>> GetOverviewsAsync(CancellationToken cancellationToken = default);

    /// <summary>Огляд однієї сесії разом з підсумками по поколіннях.</summary>
    Task<ServiceResult<SessionOverview>> GetOverviewAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>Чи виконується зараз генетичний алгоритм для сесії.</summary>
    Task<bool> IsRunningAsync(string sessionId, CancellationToken cancellationToken = default);
}
