using AutoMapper;
using SdnnGa.Model.Database.Interfaces.Repository;
using SdnnGa.Model.Models;
using SdnnGa.Model.Services.Models.ServiceResult;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;
using SdnnGa.Model.Services;
using SdnnGa.Database.Models;

namespace SdnnGa.Services.Service;

public class GeneticConfigService : IGeneticConfigService
{
    private readonly IDbRepository<DbGeneticConfig> _dbRepository;
    private readonly IMapper _mapper;

    public GeneticConfigService(
        IDbRepository<DbGeneticConfig> dbRepository,
        IMapper mapper)
    {
        _dbRepository = dbRepository;
        _mapper = mapper;
    }

    public async Task<ServiceResult<GeneticConfig>> AddGeneticConfigAsync(string sessionId, GeneticConfig geneticConfig, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(sessionId))
        {
            return ServiceResult<GeneticConfig>.FromError($"Argument error: 'Parammeter {nameof(sessionId)} is null or empty'");
        }

        if (geneticConfig == null)
        {
            return ServiceResult<GeneticConfig>.FromError($"Argument error: 'Parammeter {nameof(geneticConfig)} is null'");
        }

        try
        {
            geneticConfig.SessionId = sessionId;
            var dbCeneticConfig = _mapper.Map<DbGeneticConfig>(geneticConfig);
            var createdNewDbGeneticConfig = await _dbRepository.AddAsync(dbCeneticConfig, cancellationToken);

            return ServiceResult<GeneticConfig>.FromSuccess(_mapper.Map<GeneticConfig>(createdNewDbGeneticConfig));
        }
        catch (Exception ex)
        {
            return ServiceResult<GeneticConfig>.FromUnexpectedError($"Unexpected error occured on creating new genetic config. Message: '{ex.Message}'.");
        }
    }

    public async Task<ServiceResult<GeneticConfig>> GetByIdAsync(string geneticConfigId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(geneticConfigId))
        {
            return ServiceResult<GeneticConfig>.FromError($"Argument error: 'Parammeter {nameof(geneticConfigId)} is null or empty'");
        }
        
        try
        {
            var dbGeneticConfig = await _dbRepository.GetByIdAsync(geneticConfigId, null, cancellationToken);

            return ServiceResult<GeneticConfig>.FromSuccess(_mapper.Map<GeneticConfig>(dbGeneticConfig));
        }
        catch (Exception ex)
        {
            return ServiceResult<GeneticConfig>.FromUnexpectedError($"Unexpected error occured on obtaining geneticConfig. Message: '{ex.Message}'.");
        }
    }

    public async Task<ServiceResult<IEnumerable<GeneticConfig>>> GetAllBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var dbGeneticConfigs = (await _dbRepository.GetByFieldAsync(nameof(Epoch.SessionId), sessionId, null, cancellationToken)).ToList();

            return ServiceResult<IEnumerable<GeneticConfig>>.FromSuccess(dbGeneticConfigs.Select(geneticConfig => _mapper.Map<GeneticConfig>(geneticConfig)));
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<GeneticConfig>>.FromUnexpectedError($"Unexpected error occured on obaitning genetic configs. Message: '{ex.Message}'.");
        }
    }
}
