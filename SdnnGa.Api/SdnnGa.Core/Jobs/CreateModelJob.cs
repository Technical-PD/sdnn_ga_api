using Quartz;
using SdnnGa.Core.Classes;
using SdnnGa.Model.Constants;
using SdnnGa.Model.Core.Interfaces;
using SdnnGa.Model.Database.Models;
using SdnnGa.Model.Infrastructure.Interfaces.AzureBlobStorage;
using SdnnGa.Model.Infrastructure.Interfaces.RabbitMq;
using SdnnGa.Model.Models;
using SdnnGa.Model.Models.Core.GAConfigs;
using SdnnGa.Model.Models.Core.NNModel;
using SdnnGa.Model.Services;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SdnnGa.Core.Jobs;

public class CreateModelJob : ICreateModelJob
{
    private ModelRangeConfig _modelRangeConfig;
    private ModelConfig _modelConfig;
    private string _modelStoragePath;
    private string _modelNetStoragePath;
    private string _sessionId;
    private string _epocheNo;
    private string _modelId;
    private float _actFuncMutationProb;
    private float _countOfNeuronMutationProb;
    private float _countOfInternalLayerMutationProb;
    private float _biasMutationProb;
    private int _modelNo;

    private readonly IRabbitMqClientCreateModelService _rabbitMqClient;
    private readonly INeuralNetworkModelService _networkModelService;
    private readonly IModelGenerator _modelGenerator;
    private readonly IStorage _storage;

    public CreateModelJob(
        IRabbitMqClientCreateModelService rabbitMqClient,
        INeuralNetworkModelService networkModelService,
        IModelGenerator modelGenerator,
        IStorage storage)
    {
        _networkModelService = networkModelService;
        _rabbitMqClient = rabbitMqClient;
        _modelGenerator = modelGenerator;
        _storage = storage;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await InizializeSettingValuesAsync(context);

        ModelConfig model;
        
        if (_epocheNo == "0")
        {
            model = _modelGenerator.GenerateRandomModelConfig(_modelRangeConfig);
        }
        else
        {
            model = NeuralNetworkMutation.Mutate(
                model: _modelConfig,
                actFuncProb: _actFuncMutationProb,
                countOfNeuronProb: _countOfNeuronMutationProb,
                countOfInternalLayerProb: _countOfInternalLayerMutationProb,
                biasProb: _biasMutationProb);
        }

        string modelJson = JsonSerializer.Serialize(model);

        var result = await _rabbitMqClient.SendMessageAsync(modelJson, timeoutInSeconds: 1000);

        using (var file = new MemoryStream(Encoding.UTF8.GetBytes(modelJson)))
        {
            file.Position = 0;
            await _storage.PutFileAsync(_modelNetStoragePath, file, true);
        }

        using (var file = new MemoryStream(Encoding.UTF8.GetBytes(result)))
        {
            file.Position = 0;
            await _storage.PutFileAsync(_modelStoragePath, file, true);
        }

        var modelToUpdateResult = await _networkModelService.GetModelByIdAsync(_modelId);

        modelToUpdateResult.Entity.ModelConfigFileName = _modelStoragePath;
        modelToUpdateResult.Entity.ModelConfigDotNetFileName = _modelNetStoragePath;

        await _networkModelService.UpdateModelAssync(modelToUpdateResult.Entity);
    }

    private async Task InizializeSettingValuesAsync(IJobExecutionContext context)
    {
        var jobDataMap = context.JobDetail.JobDataMap;

        string modelConfigNetPath = jobDataMap.GetString(JobSettings.CreateModel.ModelConfigNetPathSettingName);

        if (!string.IsNullOrEmpty(modelConfigNetPath))
        {
            using (var memoryStream = await _storage.GetFileAsync(modelConfigNetPath))
            {
                var modelConfigString = _storage.ReadStreamToString(memoryStream, Encoding.UTF8);
                _modelConfig = JsonSerializer.Deserialize<ModelConfig>(modelConfigString);
            }
        }

        string modelRangeConfig = jobDataMap.GetString(JobSettings.CreateModel.ModelRangeConfigSettingName);

        if (!string.IsNullOrEmpty(modelRangeConfig))
        {
            _modelRangeConfig = JsonSerializer.Deserialize<ModelRangeConfig>(modelRangeConfig);
        }

        _sessionId = jobDataMap.GetString(JobSettings.CreateModel.SessionIdSettingName);
        _epocheNo = jobDataMap.GetString(JobSettings.CreateModel.EpocheNoSettingName);
        _modelId = jobDataMap.GetString(JobSettings.CreateModel.ModelIdSettingName);

        _actFuncMutationProb = float.Parse(jobDataMap.GetString(JobSettings.CreateModel.ActFuncMutationProbSettingName));
        _countOfNeuronMutationProb = float.Parse(jobDataMap.GetString(JobSettings.CreateModel.CountOfNeuronMutationProbSettingName));
        _countOfInternalLayerMutationProb = float.Parse(jobDataMap.GetString(JobSettings.CreateModel.CountOfInternalLayerMutationProbSettingName));
        _biasMutationProb = float.Parse(jobDataMap.GetString(JobSettings.CreateModel.BiasMutationProbSettingName));
        
        _modelNo = int.Parse(jobDataMap.GetString(JobSettings.CreateModel.ModelNoSettingName));

        _modelNetStoragePath = string.Format(StoragePath.DotNetModelPath, _sessionId, _epocheNo, _modelNo);
        _modelStoragePath = string.Format(StoragePath.ModelPath, _sessionId, _epocheNo, _modelNo);
    }
}
