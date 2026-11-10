using AutoMapper;
using SdnnGa.Model.Constants;
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

            // Нові параметри схрещування необов'язкові: старі клієнти отримують поведінку "лише мутація".
            geneticConfig.EvolutionStrategy ??= EvolutionStrategy.Mutation;
            geneticConfig.CrossoverMethod ??= CrossoverMethod.SinglePoint;
            geneticConfig.FirstParentCriterion ??= SelectionCriterion.ByAccuracy;
            geneticConfig.SecondParentCriterion ??= SelectionCriterion.ByLoss;

            var validationError = Validate(geneticConfig);
            if (validationError != null)
            {
                return ServiceResult<GeneticConfig>.FromError(validationError);
            }
            var dbCeneticConfig = _mapper.Map<DbGeneticConfig>(geneticConfig);

            // Одна конфігурація на сесію: повторне збереження оновлює існуючу (раніше додавався дублікат, який ігнорувався).
            var existing = (await _dbRepository.GetByFieldAsync(nameof(GeneticConfig.SessionId), sessionId, false, null, cancellationToken)).FirstOrDefault();

            DbGeneticConfig createdNewDbGeneticConfig;
            if (existing != null)
            {
                dbCeneticConfig.Id = existing.Id;
                dbCeneticConfig.RecCreated = existing.RecCreated;
                createdNewDbGeneticConfig = await _dbRepository.UpdateAsync(dbCeneticConfig, cancellationToken);
            }
            else
            {
                createdNewDbGeneticConfig = await _dbRepository.AddAsync(dbCeneticConfig, cancellationToken);
            }

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

    public async Task<ServiceResult<GeneticConfig>> GetAllBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var dbGeneticConfigs = await _dbRepository.GetByFieldAsync(nameof(Epoch.SessionId), sessionId, true, null, cancellationToken);

            return ServiceResult<GeneticConfig>.FromSuccess(_mapper.Map<GeneticConfig>(dbGeneticConfigs.FirstOrDefault()));
        }
        catch (Exception ex)
        {
            return ServiceResult<GeneticConfig>.FromUnexpectedError($"Unexpected error occured on obaitning genetic configs. Message: '{ex.Message}'.");
        }
    }

    private static string Validate(GeneticConfig config)
    {
        string[] criteria = { SelectionCriterion.ByLoss, SelectionCriterion.ByAccuracy };

        if (!EvolutionStrategy.All.Contains(config.EvolutionStrategy))
        {
            return $"Unknown evolution strategy '{config.EvolutionStrategy}'. Allowed: {string.Join(", ", EvolutionStrategy.All)}.";
        }

        if (!CrossoverMethod.All.Contains(config.CrossoverMethod))
        {
            return $"Unknown crossover method '{config.CrossoverMethod}'. Allowed: {string.Join(", ", CrossoverMethod.All)}.";
        }

        if (!criteria.Contains(config.FirstParentCriterion) || !criteria.Contains(config.SecondParentCriterion))
        {
            return $"Parent criteria must be one of: {string.Join(", ", criteria)}.";
        }

        return null;
    }
}
