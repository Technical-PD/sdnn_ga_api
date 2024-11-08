using SdnnGa.Database.Models;
using SdnnGa.Model.Database.Interfaces.Repository;
using SdnnGa.Model.Models;
using SdnnGa.Model.Services;
using SdnnGa.Model.Services.Models.ServiceResult;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SdnnGa.Services.SessionService;

public class SessionService : ISessionService
{
    private readonly IDbRepository<DbSession> _dbRepository;

    public SessionService(IDbRepository<DbSession> dbRepository)
    {
        _dbRepository = dbRepository;
    }

    public async Task<ServiceResult<Session>> CreateSessionAssync(Session session, CancellationToken cancellationToken = default)
    {
        if (session == null)
        {
            return ServiceResult<Session>.FromError($"Failed on session creating. Parammeter {nameof(session)} can not be null.");
        }

        try
        {
            var newDbSession = new DbSession
            {
                Name = session.Name,
                Description = session.Description,
            };

            var dbSession = await _dbRepository.AddAsync(newDbSession);

            if (dbSession == null)
            {
                return ServiceResult<Session>.FromError($"Failed on session creating.");
            }

            session.Id = dbSession.Id;
            session.RecCreated = dbSession.RecCreated;
            session.RecModified = dbSession.RecModified;

            return ServiceResult<Session>.FromSuccess(session);
        }
        catch (Exception ex)
        {
            return ServiceResult<Session>.FromUnexpectedError($"UnespectedError ocured on Session creating. Exception message '{ex.Message}'.");
        }
    }

    public async Task<ServiceResult<ICollection<Session>>> GetAllSessionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var dbSessions = await _dbRepository.GetAllAsync();

            if (dbSessions == null || !dbSessions.Any())
            {
                return ServiceResult<ICollection<Session>>.FromNotFound($"Sessions was not found.");
            }

            var sessions = dbSessions.Select(dbSession => new Session
            {
                Id = dbSession.Id,
                Name = dbSession.Name,
                Description = dbSession.Description,
                RecCreated = dbSession.RecCreated,
                RecModified = dbSession.RecModified,
            }).ToList();

            return ServiceResult<ICollection<Session>>.FromSuccess(sessions);
        }
        catch (Exception ex)
        {
            return ServiceResult<ICollection<Session>>.FromUnexpectedError($"UnespectedError ocured on obtaining all sessions. Exception message '{ex.Message}'.");
        }
    }

    public async Task<ServiceResult<Session>> GetSessionAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(id))
        {
            return ServiceResult<Session>.FromError($"Failed on obtaining session by Id. Parammeter {nameof(id)} can not be null or empty.");
        }

        try
        {
            var dbSession = await _dbRepository.GetByIdAsync(id);

            if (dbSession == null)
            {
                return ServiceResult<Session>.FromNotFound($"Sessions with Id({id}) was not found.");
            }

            var session = new Session
            {
                Id = dbSession.Id,
                Name = dbSession.Name,
                Description = dbSession.Description,
                RecCreated = dbSession.RecCreated,
                RecModified = dbSession.RecModified,
            };

            return ServiceResult<Session>.FromSuccess(session);
        }
        catch (Exception ex)
        {
            return ServiceResult<Session>.FromUnexpectedError($"UnespectedError ocured on obtaining session by Id. Exception message '{ex.Message}'.");
        }
    }
}