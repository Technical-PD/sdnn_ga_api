using Quartz;
using SdnnGa.Model.Constants;
using SdnnGa.Model.Core.Interfaces;
using SdnnGa.Model.Database.Interfaces.Repository;
using SdnnGa.Model.Infrastructure.Interfaces.Quartz.Scheduler;
using SdnnGa.Model.Models;
using SdnnGa.Model.Models.Core.GAConfigs;
using SdnnGa.Model.Services;
using SdnnGa.Model.Services.Models.ServiceResult;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SdnnGa.Services.Service;

public class GeneticService : IGeneticService
{
    private readonly IEpochService _epochService;
    private readonly IModelGenerator _modelGenerator;
    private readonly IJobScheduler _jobScheduler;
    private readonly INeuralNetworkModelService _networkModelService;

    public GeneticService(
        IEpochService epochService,
        IModelGenerator modelGenerator,
        IJobScheduler jobScheduler,
        INeuralNetworkModelService networkModelService)
    {
        _epochService = epochService;
        _modelGenerator = modelGenerator;
        _jobScheduler = jobScheduler;
        _networkModelService = networkModelService;
    }

    public async Task<ServiceResult> StartGeneticFlow(
        ModelRangeConfig modelRangeConfig, 
        CompileConfig compileConfig, 
        TrainConfig trainConfig, 
        char separator, 
        string sessionId, 
        CancellationToken cancellationToken = default)
    {
        var currentEpochResult = await _epochService.AddEpochAsync(sessionId);

        if (currentEpochResult.IsError)
        {
            ServiceResult.FromError($"Error on creating epoch in Genetic Flow. Message: '{currentEpochResult.Message}'");
        }

        for (int i = 0; i < modelRangeConfig.CountOfModels; i++)
        {
            var model = _modelGenerator.GenerateRandomModelConfig(modelRangeConfig);

            string modelJson = JsonSerializer.Serialize(model);

            string epochId = currentEpochResult.Entity.Id;

            string modelPath = string.Format(StoragePath.ModelPath, sessionId, currentEpochResult.Entity.EpochNo, Guid.NewGuid());

            await _jobScheduler.ScheduleJobAsync<ICreateModelJob>("CreateModel", new Dictionary<string, string>
            {
                { JobSettings.CreateModel.ModelConfigSettingName, modelJson },
                { JobSettings.CreateModel.ModelStoragePathSettingName, modelPath },
            });

            var newModel = new NeuralNetworkModel
            {
                SessionId = sessionId,
                EpocheId = epochId,
                ModelConfigFileName = modelPath,
                IsTrained = false,
                Name = $"Model_{currentEpochResult.Entity.EpochNo}",
            };

            await _networkModelService.CreateModelAsync(newModel);
        }

        return ServiceResult.FromSuccess();
    }
}
