using Quartz;
using SdnnGa.Model.Constants;
using SdnnGa.Model.Core.Interfaces;
using SdnnGa.Model.Infrastructure.Interfaces.AzureBlobStorage;
using SdnnGa.Model.Infrastructure.Interfaces.RabbitMq;
using SdnnGa.Model.Models.Core.NNModel;
using SdnnGa.Model.Services;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SdnnGa.Core.Jobs;

public class FitModelJob : IFitModelJob
{
    private readonly IRabbitMqClientFitModelService _rabbitMqClient;
    private readonly IStorage _storage;
    private readonly INeuralNetworkModelService _neuralNetworkModelService;
    private readonly int _timeout = 1800;

    private string _modelId;
    private string _fitModelJobConfig;
    private string _sessionId;
    private string _epocheNo;
    private string _weigthPath;

    public FitModelJob(
        IRabbitMqClientFitModelService rabbitMqClient,
        IStorage storage,
        INeuralNetworkModelService neuralNetworkModelService)
    {
        _rabbitMqClient = rabbitMqClient;
        _storage = storage;
        _neuralNetworkModelService = neuralNetworkModelService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            await InizializeSettingValuesAsync(context);
            
            Console.WriteLine($"Start fit model {_modelId}");

            var result = await _rabbitMqClient.SendMessageAsync(_fitModelJobConfig, timeoutInSeconds: _timeout);

            Console.WriteLine(result);

            var fitResult = JsonSerializer.Deserialize<FitResult>(result);

            var modelToUpdateResult = await _neuralNetworkModelService.GetModelByIdAsync(_modelId);

            if (modelToUpdateResult.IsError)
            {
                throw new Exception("Error ocured on obraining NnModel by id in FitModelJob.");
            }

            modelToUpdateResult.Entity.LossValue = fitResult.Loss;
            modelToUpdateResult.Entity.AccuracyValue = fitResult.Accuracy;
            modelToUpdateResult.Entity.IsTrained = true;
            modelToUpdateResult.Entity.FitHistory = JsonSerializer.Serialize(fitResult.History);
            modelToUpdateResult.Entity.WeightsFileName = _weigthPath;

            await _neuralNetworkModelService.UpdateModelAssync(modelToUpdateResult.Entity);

            Console.WriteLine($"End fit model {_modelId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private async Task InizializeSettingValuesAsync(IJobExecutionContext context)
    {
        var jobDataMap = context.JobDetail.JobDataMap;

        _modelId = jobDataMap.GetString(JobSettings.FitModel.ModelIdSettingName);
        _sessionId = jobDataMap.GetString(JobSettings.FitModel.SessionIdSettingName);
        _epocheNo = jobDataMap.GetString(JobSettings.FitModel.EpocheNoSettingName);
        _weigthPath = string.Format(StoragePath.WeightPath, _sessionId, _epocheNo, Guid.NewGuid());

        var fitModelJobConfig = new FitModelJobConfig
        {
            UseEarlyStopping = bool.Parse(jobDataMap.GetString(JobSettings.FitModel.UseEarlyStoppingSettingName)),
            MinDelta = float.Parse(jobDataMap.GetString(JobSettings.FitModel.MinDeltaSettingName)),
            Patience = bool.Parse(jobDataMap.GetString(JobSettings.FitModel.PatienceSettingName)),
            IsLearnWithValidation = bool.Parse(jobDataMap.GetString(JobSettings.FitModel.IsLearnWithValidationSettingName)),
            Optimizer = jobDataMap.GetString(JobSettings.FitModel.OptimizerSettingName),
            LossFunc = jobDataMap.GetString(JobSettings.FitModel.LossFuncSettingName),
            Epochs = int.Parse(jobDataMap.GetString(JobSettings.FitModel.EpochsSettingName)),
            BatchSize = int.Parse(jobDataMap.GetString(JobSettings.FitModel.BatchSizeSettingName)),
            WeigthPath = _weigthPath
        };

        var model = await _neuralNetworkModelService.GetModelByIdAsync(_modelId);

        using (var memoryStream = await _storage.GetFileAsync(model.Entity.ModelConfigFileName))
        {
            fitModelJobConfig.ModelConfigJson = _storage.ReadStreamToString(memoryStream, Encoding.UTF8);
        }

        string xTrainPath = jobDataMap.GetString(JobSettings.FitModel.XTrainPathSettingName);
        using (var memoryStream = await _storage.GetFileAsync(xTrainPath))
        {
            fitModelJobConfig.XTrain = _storage.ReadStreamToString(memoryStream, Encoding.UTF8);
        }

        string yTrainPath = jobDataMap.GetString(JobSettings.FitModel.YTrainPathSettingName);
        using (var memoryStream = await _storage.GetFileAsync(yTrainPath))
        {
            fitModelJobConfig.YTrain = _storage.ReadStreamToString(memoryStream, Encoding.UTF8);
        }

        _fitModelJobConfig = JsonSerializer.Serialize(fitModelJobConfig);
    }
}
