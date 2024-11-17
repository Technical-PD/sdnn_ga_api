using Quartz;
using SdnnGa.Core.Classes;
using SdnnGa.Model.Constants;
using SdnnGa.Model.Core.Interfaces;
using SdnnGa.Model.Infrastructure.Interfaces.AzureBlobStorage;
using SdnnGa.Model.Infrastructure.Interfaces.RabbitMq;
using SdnnGa.Model.Models;
using SdnnGa.Model.Models.Core.GAConfigs;
using SdnnGa.Model.Services;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SdnnGa.Core.Jobs;

public class CreateModelJob : ICreateModelJob
{
    private ModelRangeConfig _modelConfig;
    private string _modelStoragePath;
    private string _sessionId;
    private string _epocheNo;
    private string _modelId;

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
        InizializeSettingValues(context);

        var model = _modelGenerator.GenerateRandomModelConfig(_modelConfig);

        string modelJson = JsonSerializer.Serialize(model);

        var result = await _rabbitMqClient.SendMessageAsync(modelJson, timeoutInSeconds: 1000);

        using (var file = new MemoryStream(Encoding.UTF8.GetBytes(result)))
        {
            file.Position = 0;
            await _storage.PutFileAsync(_modelStoragePath, file, true);
        }

        var modelToUpdateResult = await _networkModelService.GetModelByIdAsync(_modelId);

        modelToUpdateResult.Entity.ModelConfigFileName = _modelStoragePath;

        await _networkModelService.UpdateModelAssync(modelToUpdateResult.Entity);
    }

    private void InizializeSettingValues(IJobExecutionContext context)
    {
        var jobDataMap = context.JobDetail.JobDataMap;

        _modelConfig = JsonSerializer.Deserialize<ModelRangeConfig>(jobDataMap.GetString(JobSettings.CreateModel.ModelConfigSettingName));
        _sessionId = jobDataMap.GetString(JobSettings.CreateModel.SessionIdSettingName);
        _epocheNo = jobDataMap.GetString(JobSettings.CreateModel.EpocheNoSettingName);
        _modelId = jobDataMap.GetString(JobSettings.CreateModel.ModelIdSettingName);

        _modelStoragePath = string.Format(StoragePath.ModelPath, _sessionId, _epocheNo, Guid.NewGuid());
    }
}
