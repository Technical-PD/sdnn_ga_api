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

public class NeuralNetworkModelService : INeuralNetworkModelService
{
    private readonly IDbRepository<DbNeuralNetworkModel> _dbRepository;
    private readonly IMapper _mapper;

    public NeuralNetworkModelService(
        IDbRepository<DbNeuralNetworkModel> dbRepository,
        IMapper mapper)
    {
        _dbRepository = dbRepository;
        _mapper = mapper;
    }

    public async Task<ServiceResult<NeuralNetworkModel>> CreateModelAsync(NeuralNetworkModel neuralNetworkModel, CancellationToken cancellationToken = default)
    {
        if (neuralNetworkModel == null)
        {
            return ServiceResult<NeuralNetworkModel>.FromError($"Argument null error: {neuralNetworkModel} can not be null");
        }

        try
        {
            var dbModel = _mapper.Map<DbNeuralNetworkModel>(neuralNetworkModel);

            var createdDbModel = await _dbRepository.AddAsync(dbModel, cancellationToken);

            return ServiceResult<NeuralNetworkModel>.FromSuccess(_mapper.Map<NeuralNetworkModel>(dbModel));
        }
        catch (Exception ex)
        {
            return ServiceResult<NeuralNetworkModel>.FromUnexpectedError($"Unexpected error occured on neural network model adding to the DataBase. Message: '{ex.Message}'");
        }
    }

    public async Task<ServiceResult<NeuralNetworkModel>> UpdateModelAssync(NeuralNetworkModel model, CancellationToken cancellationToken = default)
    {
        if (model == null)
        {
            return ServiceResult<NeuralNetworkModel>.FromError($"Failed on NN model updating. Parammeter {nameof(model)} can not be null.");
        }

        try
        {
            var newDbModel = _mapper.Map<DbNeuralNetworkModel>(model);

            var dbSession = await _dbRepository.UpdateAsync(newDbModel, cancellationToken);

            if (dbSession == null)
            {
                return ServiceResult<NeuralNetworkModel>.FromError($"Failed on NN model updating.");
            }

            return ServiceResult<NeuralNetworkModel>.FromSuccess(_mapper.Map<NeuralNetworkModel>(dbSession));
        }
        catch (Exception ex)
        {
            return ServiceResult<NeuralNetworkModel>.FromUnexpectedError($"UnespectedError ocured on NN model updating. Exception message '{ex.Message}'.");
        }
    }

    public async Task<ServiceResult<NeuralNetworkModel>> GetModelByIdAsync(string modelId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(modelId))
        {
            return ServiceResult<NeuralNetworkModel>.FromError($"Argument null error: {modelId} can not be null or empty");
        }

        try
        {
            var dbModel = await _dbRepository.GetByIdAsync(modelId, null, cancellationToken);

            return ServiceResult<NeuralNetworkModel>.FromSuccess(_mapper.Map<NeuralNetworkModel>(dbModel));
        }
        catch (Exception ex)
        {
            return ServiceResult<NeuralNetworkModel>.FromUnexpectedError($"Unexpected error occured on obtaining by Id neural network model. Message: '{ex.Message}'");
        }
    }

    public async Task<ServiceResult<List<NeuralNetworkModel>>> GetModelByEpochIdAsync(string epochId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(epochId))
        {
            return ServiceResult<List<NeuralNetworkModel>>.FromError($"Argument null error: {epochId} can not be null or empty");
        }

        try
        {
            var createdDbModel = await _dbRepository.GetByFieldAsync("EpocheId", epochId, null, cancellationToken);

            var modelList = createdDbModel.Select(modelDb => _mapper.Map<NeuralNetworkModel>(modelDb)).ToList();

            return ServiceResult<List<NeuralNetworkModel>>.FromSuccess(modelList);
        }
        catch (Exception ex)
        {
            return ServiceResult<List<NeuralNetworkModel>>.FromUnexpectedError($"Unexpected error occured on obtaining by EpocheId neural network model. Message: '{ex.Message}'");
        }
    }

    public async Task<ServiceResult<List<NeuralNetworkModel>>> GetAllModelsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var createdDbModel = await _dbRepository.GetAllAsync(cancellationToken);

            var modelList = createdDbModel.Select(modelDb => _mapper.Map<NeuralNetworkModel>(modelDb)).ToList();

            return ServiceResult<List<NeuralNetworkModel>>.FromSuccess(modelList);
        }
        catch (Exception ex)
        {
            return ServiceResult<List<NeuralNetworkModel>>.FromUnexpectedError($"Unexpected error occured on obtaining by EpocheId neural network model. Message: '{ex.Message}'");
        }
    }
}