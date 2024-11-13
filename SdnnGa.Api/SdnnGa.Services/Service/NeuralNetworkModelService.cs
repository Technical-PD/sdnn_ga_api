using AutoMapper;
using SdnnGa.Database.Models;
using SdnnGa.Model.Database.Interfaces.Repository;
using SdnnGa.Model.Models;
using SdnnGa.Model.Services;
using SdnnGa.Model.Services.Models.ServiceResult;
using System;
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
}