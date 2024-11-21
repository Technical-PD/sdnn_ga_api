using SdnnGa.Model.Models;
using SdnnGa.Model.Services.Models.ServiceResult;
using SdnnGa.Model.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace SdnnGa.Services.Service;

public class StatisticService : IStatisticService
{
    private readonly INeuralNetworkModelService _networkModelService;
    private readonly IEpochService _epochService;

    public StatisticService(
        INeuralNetworkModelService networkModelService, 
        IEpochService epochService)
    {
        _networkModelService = networkModelService;
        _epochService = epochService;
    }

    public async Task<ServiceResult<List<NeuralNetworkModel>>> GetBestModelInSessionByLossAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(sessionId))
        {
            return ServiceResult<List<NeuralNetworkModel>>.FromError($"Argument error. Argument {nameof(sessionId)} can not be null or empty");
        }

        try
        {
            var sessionEpochesResult = await _epochService.GetAllBySessionIdAsync(sessionId, cancellationToken);

            if (!sessionEpochesResult.IsSuccessful)
            {
                return ServiceResult<List<NeuralNetworkModel>>.FromError($"Epoches does not exist for session with id='{sessionId}'");
            }

            var bestModels = sessionEpochesResult.Entity
                .OrderBy(ep => ep.EpochNo)
                .Select(async epoch =>
                {
                    var bestModel = await _networkModelService.GetModelByEpochIdAsync(epoch.Id, cancellationToken);
                    return bestModel.Entity.OrderBy(md => md.LossValue).FirstOrDefault();
                }).ToList();

            var results = await Task.WhenAll(bestModels);


            return ServiceResult<List<NeuralNetworkModel>>.FromSuccess(results.ToList());
        }
        catch (Exception ex)
        {
            return ServiceResult<List<NeuralNetworkModel>>.FromUnexpectedError($"Unexpected error was occured on obtaining statistic from session. Message: '{ex.Message}'");
        }
    }

    public async Task<ServiceResult<List<NeuralNetworkModel>>> GetBestModelInSessionByAccuracyAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(sessionId))
        {
            return ServiceResult<List<NeuralNetworkModel>>.FromError($"Argument error. Argument {nameof(sessionId)} can not be null or empty");
        }

        try
        {
            var sessionEpochesResult = await _epochService.GetAllBySessionIdAsync(sessionId, cancellationToken);

            if (!sessionEpochesResult.IsSuccessful)
            {
                return ServiceResult<List<NeuralNetworkModel>>.FromError($"Epoches does not exist for session with id='{sessionId}'");
            }

            var bestModels = sessionEpochesResult.Entity
                .OrderBy(ep => ep.EpochNo)
                .Select(async epoch =>
                {
                    var bestModel = await _networkModelService.GetModelByEpochIdAsync(epoch.Id, cancellationToken);
                    return bestModel.Entity.OrderBy(md => md.AccuracyValue).LastOrDefault();
                }).ToList();

            var results = await Task.WhenAll(bestModels);


            return ServiceResult<List<NeuralNetworkModel>>.FromSuccess(results.ToList());
        }
        catch (Exception ex)
        {
            return ServiceResult<List<NeuralNetworkModel>>.FromUnexpectedError($"Unexpected error was occured on obtaining statistic from session. Message: '{ex.Message}'");
        }
    }
}
