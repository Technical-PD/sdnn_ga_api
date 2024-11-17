using AutoMapper;
using SdnnGa.Database.Models;
using SdnnGa.Model.Database.Interfaces.Repository;
using SdnnGa.Model.Models;
using SdnnGa.Model.Services.Models.ServiceResult;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;
using SdnnGa.Model.Services;

namespace SdnnGa.Services.SessionService;

public class FitConfigService : IFitConfigService
{
    private readonly IDbRepository<DbFitConfig> _dbRepository;
    private readonly IMapper _mapper;

    public FitConfigService(
        IDbRepository<DbFitConfig> dbRepository,
        IMapper mapper)
    {
        _dbRepository = dbRepository;
        _mapper = mapper;
    }

    public async Task<ServiceResult<FitConfig>> AddFitConfigAssync(FitConfig fitConfig, CancellationToken cancellationToken = default)
    {
        if (fitConfig == null)
        {
            return ServiceResult<FitConfig>.FromError($"Failed on FitConfig creating. Parammeter {nameof(fitConfig)} can not be null.");
        }

        try
        {
            var dbFitConfig = _mapper.Map<DbFitConfig>(fitConfig);

            var dbFitConfigCreated = await _dbRepository.AddAsync(dbFitConfig, cancellationToken);

            if (dbFitConfigCreated == null)
            {
                return ServiceResult<FitConfig>.FromError($"Failed on FitConfig creating.");
            }

            return ServiceResult<FitConfig>.FromSuccess(_mapper.Map<FitConfig>(dbFitConfigCreated));
        }
        catch (Exception ex)
        {
            return ServiceResult<FitConfig>.FromUnexpectedError($"UnespectedError ocured on FitConfig creating. Exception message '{ex.Message}'.");
        }
    }

    public async Task<ServiceResult<FitConfig>> UpdateFitConfigAssync(FitConfig fitConfig, CancellationToken cancellationToken = default)
    {
        if (fitConfig == null)
        {
            return ServiceResult<FitConfig>.FromError($"Failed on FitConfig updating. Parammeter {nameof(fitConfig)} can not be null.");
        }

        try
        {
            var newDbFitConfig = _mapper.Map<DbFitConfig>(fitConfig);

            var dbFitConfig = await _dbRepository.UpdateAsync(newDbFitConfig, cancellationToken);

            if (dbFitConfig == null)
            {
                return ServiceResult<FitConfig>.FromError($"Failed on FitConfig updating.");
            }

            return ServiceResult<FitConfig>.FromSuccess(_mapper.Map<FitConfig>(dbFitConfig));
        }
        catch (Exception ex)
        {
            return ServiceResult<FitConfig>.FromUnexpectedError($"UnespectedError ocured on FitConfig updating. Exception message '{ex.Message}'.");
        }
    }

    public async Task<ServiceResult<ICollection<FitConfig>>> GetAllFitConfigsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var dbSessions = await _dbRepository.GetAllAsync(cancellationToken);

            if (dbSessions == null || !dbSessions.Any())
            {
                return ServiceResult<ICollection<FitConfig>>.FromNotFound($"FitConfigs was not found.");
            }

            var sessions = dbSessions.Select(dbSession => _mapper.Map<FitConfig>(dbSession)).ToList();

            return ServiceResult<ICollection<FitConfig>>.FromSuccess(sessions);
        }
        catch (Exception ex)
        {
            return ServiceResult<ICollection<FitConfig>>.FromUnexpectedError($"UnespectedError ocured on obtaining all FitConfigs. Exception message '{ex.Message}'.");
        }
    }

    public async Task<ServiceResult<FitConfig>> GetFitConfigAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(id))
        {
            return ServiceResult<FitConfig>.FromError($"Failed on obtaining FitConfig by Id. Parammeter {nameof(id)} can not be null or empty.");
        }

        try
        {
            var dbSession = await _dbRepository.GetByIdAsync(id, null, cancellationToken);

            if (dbSession == null)
            {
                return ServiceResult<FitConfig>.FromNotFound($"FitConfigs with Id({id}) was not found.");
            }

            return ServiceResult<FitConfig>.FromSuccess(_mapper.Map<FitConfig>(dbSession));
        }
        catch (Exception ex)
        {
            return ServiceResult<FitConfig>.FromUnexpectedError($"UnespectedError ocured on obtaining FitConfig by Id. Exception message '{ex.Message}'.");
        }
    }

    public async Task<ServiceResult<FitConfig>> GetFitConfigBySessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(sessionId))
        {
            return ServiceResult<FitConfig>.FromError($"Failed on obtaining FitConfig by Id. Parammeter {nameof(sessionId)} can not be null or empty.");
        }

        try
        {
            var dbSession = await _dbRepository.GetByFieldAsync("SessionId", sessionId, null, cancellationToken);

            if (dbSession == null)
            {
                return ServiceResult<FitConfig>.FromNotFound($"FitConfigs with Id({sessionId}) was not found.");
            }

            return ServiceResult<FitConfig>.FromSuccess(_mapper.Map<FitConfig>(dbSession.FirstOrDefault()));
        }
        catch (Exception ex)
        {
            return ServiceResult<FitConfig>.FromUnexpectedError($"UnespectedError ocured on obtaining FitConfig by Id. Exception message '{ex.Message}'.");
        }
    }
}