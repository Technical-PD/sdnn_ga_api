using AutoMapper;
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

namespace SdnnGa.Services.Service;

public class EpochService : IEpochService
{
    private readonly IDbRepository<DbEpoch> _dbRepository;
    private readonly IMapper _mapper;

    public EpochService(
        IDbRepository<DbEpoch> dbRepository,
        IMapper mapper)
    {
        _dbRepository = dbRepository;
        _mapper = mapper;
    }

    public async Task<ServiceResult<Epoch>> AddEpochAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(sessionId))
        {
            return ServiceResult<Epoch>.FromError($"Argument error: 'Parammeter {nameof(sessionId)} is null or empty'");
        }

        try
        {
            var epoches = (await _dbRepository.GetByFieldAsync(nameof(Epoch.SessionId), sessionId, null, cancellationToken)).ToList();

            var newDbEpoch = new DbEpoch
            {
                SessionId = sessionId,
                IsTrained = false,
                ModelCount = 0,
            };

            if (!epoches.Any())
            {
                newDbEpoch.Name = "0";
                newDbEpoch.EpochNo = 0;
            }
            else
            {
                int currentNo = epoches.OrderByDescending(epoch => epoch.EpochNo).FirstOrDefault().EpochNo;

                newDbEpoch.Name = $"{currentNo + 1}";
                newDbEpoch.EpochNo = currentNo + 1;
            }

            var createdNewDbEpoch = await _dbRepository.AddAsync(newDbEpoch);

            return ServiceResult<Epoch>.FromSuccess(_mapper.Map<Epoch>(createdNewDbEpoch));
        }
        catch (Exception ex)
        {
            return ServiceResult<Epoch>.FromUnexpectedError($"Unexpected error occured on creating new epoch. Message: '{ex.Message}'.");
        }
    }

    public async Task<ServiceResult<IEnumerable<Epoch>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var dbEpoches = (await _dbRepository.GetAllAsync(cancellationToken)).ToList();

            return ServiceResult<IEnumerable<Epoch>>.FromSuccess(dbEpoches.Select(epoche => _mapper.Map<Epoch>(epoche)));
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<Epoch>>.FromUnexpectedError($"Unexpected error occured on creating new epoch. Message: '{ex.Message}'.");
        }
    }

    public async Task<ServiceResult<Epoch>> GetByIdAsync(string epochId, CancellationToken cancellationToken = default)
    {
        try
        {
            var dbEpoch = await _dbRepository.GetByIdAsync(epochId, null, cancellationToken);

            return ServiceResult<Epoch>.FromSuccess( _mapper.Map<Epoch>(dbEpoch));
        }
        catch (Exception ex)
        {
            return ServiceResult<Epoch>.FromUnexpectedError($"Unexpected error occured on creating new epoch. Message: '{ex.Message}'.");
        }
    }

    public async Task<ServiceResult<IEnumerable<Epoch>>> GetAllBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var dbEpoches = (await _dbRepository.GetByFieldAsync(nameof(Epoch.SessionId), sessionId, null, cancellationToken)).ToList();

            return ServiceResult<IEnumerable<Epoch>>.FromSuccess(dbEpoches.Select(epoche => _mapper.Map<Epoch>(epoche)));
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<Epoch>>.FromUnexpectedError($"Unexpected error occured on creating new epoch. Message: '{ex.Message}'.");
        }
    }
}