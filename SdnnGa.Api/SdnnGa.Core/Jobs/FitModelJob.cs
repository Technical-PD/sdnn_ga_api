using Quartz;
using SdnnGa.Model.Constants;
using SdnnGa.Model.Core.Interfaces;
using SdnnGa.Model.Infrastructure.Interfaces.Storage;
using SdnnGa.Model.Infrastructure.Interfaces.Training;
using SdnnGa.Model.Infrastructure.Interfaces.Worker;
using SdnnGa.Model.Models;
using SdnnGa.Model.Models.Core.NNModel;
using SdnnGa.Model.Services;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SdnnGa.Core.Jobs;

public class FitModelJob : IFitModelJob
{
    private static readonly TimeSpan CreatePollInterval = TimeSpan.FromSeconds(2);

    private readonly IModelWorkerClient _workerClient;
    private readonly IStorage _storage;
    private readonly INeuralNetworkModelService _neuralNetworkModelService;
    private readonly ITrainingProgressTracker _progressTracker;
    private readonly IGeneticFlowStopRegistry _stopRegistry;

    private string _modelId;
    private string _sessionId;
    private string _epochId;
    private string _modelName;
    private string _epocheNo;
    private string _weigthPath;

    public FitModelJob(
        IModelWorkerClient workerClient,
        IStorage storage,
        INeuralNetworkModelService neuralNetworkModelService,
        ITrainingProgressTracker progressTracker,
        IGeneticFlowStopRegistry stopRegistry)
    {
        _workerClient = workerClient;
        _storage = storage;
        _neuralNetworkModelService = neuralNetworkModelService;
        _progressTracker = progressTracker;
        _stopRegistry = stopRegistry;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var fitModelJobConfig = await InizializeSettingValuesAsync(context);

            if (fitModelJobConfig == null)
            {
                // Створення моделі завершилось з помилкою — вона вже записана в модель.
                return;
            }

            if (_stopRegistry.IsStopRequested(_sessionId))
            {
                await ModelFailureRecorder.RecordAsync(_neuralNetworkModelService, _modelId, ModelStatusMessages.StoppedByUser);
                return;
            }

            Console.WriteLine($"Start fit model {_modelId}");

            var fitResult = await _workerClient.FitModelAsync(
                fitModelJobConfig,
                progress => _progressTracker.Report(_modelId, _modelName, _sessionId, _epochId, progress),
                context.CancellationToken);

            if (!string.IsNullOrEmpty(fitResult.Weights))
            {
                using var weightsStream = new MemoryStream(Convert.FromBase64String(fitResult.Weights));
                await _storage.PutFileAsync(_weigthPath, weightsStream, true);
            }

            var modelToUpdateResult = await _neuralNetworkModelService.GetModelByIdAsync(_modelId);

            if (modelToUpdateResult.IsError)
            {
                throw new Exception("Error ocured on obraining NnModel by id in FitModelJob.");
            }

            modelToUpdateResult.Entity.LossValue = fitResult.Loss;
            modelToUpdateResult.Entity.AccuracyValue = fitResult.Accuracy;
            modelToUpdateResult.Entity.IsTrained = true;
            modelToUpdateResult.Entity.FitError = null;
            modelToUpdateResult.Entity.FitHistory = JsonSerializer.Serialize(fitResult.History);
            modelToUpdateResult.Entity.WeightsFileName = string.IsNullOrEmpty(fitResult.Weights) ? null : _weigthPath;

            await _neuralNetworkModelService.UpdateModelAssync(modelToUpdateResult.Entity);

            Console.WriteLine($"End fit model {_modelId} (loss={fitResult.Loss}, accuracy={fitResult.Accuracy})");
        }
        catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested || _stopRegistry.IsStopRequested(_sessionId))
        {
            Console.WriteLine($"FitModelJob for model '{_modelId}' was stopped.");
            await ModelFailureRecorder.RecordAsync(_neuralNetworkModelService, _modelId, ModelStatusMessages.StoppedByUser);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FitModelJob failed for model '{_modelId}': {ex}");
            await ModelFailureRecorder.RecordAsync(_neuralNetworkModelService, _modelId, $"Training failed: {ex.Message}");
        }
        finally
        {
            _progressTracker.Complete(_modelId);
        }
    }

    /// <returns>Конфіг для навчання або <c>null</c>, якщо модель не вдалося створити.</returns>
    private async Task<FitModelJobConfig> InizializeSettingValuesAsync(IJobExecutionContext context)
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
            Patience = int.Parse(jobDataMap.GetString(JobSettings.FitModel.PatienceSettingName)),
            IsLearnWithValidation = bool.Parse(jobDataMap.GetString(JobSettings.FitModel.IsLearnWithValidationSettingName)),
            Optimizer = jobDataMap.GetString(JobSettings.FitModel.OptimizerSettingName),
            LossFunc = jobDataMap.GetString(JobSettings.FitModel.LossFuncSettingName),
            Epochs = int.Parse(jobDataMap.GetString(JobSettings.FitModel.EpochsSettingName)),
            BatchSize = int.Parse(jobDataMap.GetString(JobSettings.FitModel.BatchSizeSettingName)),
        };

        var model = await WaitForCreatedModelAsync(context.CancellationToken);
        _epochId = model.EpocheId;
        _modelName = model.Name;

        if (model.FitError != null)
        {
            return null;
        }

        fitModelJobConfig.ModelConfigJson = await ReadRequiredTextAsync(model.ModelConfigFileName);
        fitModelJobConfig.XTrain = await ReadRequiredTextAsync(jobDataMap.GetString(JobSettings.FitModel.XTrainPathSettingName));
        fitModelJobConfig.YTrain = await ReadRequiredTextAsync(jobDataMap.GetString(JobSettings.FitModel.YTrainPathSettingName));

        return fitModelJobConfig;
    }

    /// <summary>
    /// Fit-джоба стартує через 10 с після create-джоби, але воркер обробляє запити по черзі,
    /// тож модель могла ще не бути створена. Чекаємо, поки CreateModelJob запише конфіг (або помилку).
    /// </summary>
    private async Task<NeuralNetworkModel> WaitForCreatedModelAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var modelResult = await _neuralNetworkModelService.GetModelByIdAsync(_modelId, cancellationToken, useTracking: false);

            if (modelResult.IsError || modelResult.Entity == null)
            {
                throw new Exception($"Model '{_modelId}' was not found: {modelResult.Message}");
            }

            var model = modelResult.Entity;

            if (!string.IsNullOrEmpty(model.ModelConfigFileName) || model.FitError != null)
            {
                return model;
            }

            await Task.Delay(CreatePollInterval, cancellationToken);
        }
    }

    private async Task<string> ReadRequiredTextAsync(string storagePath)
    {
        if (string.IsNullOrEmpty(storagePath))
        {
            throw new InvalidOperationException("Required file path is empty (was the training data uploaded for this session?).");
        }

        using var stream = await _storage.GetFileAsync(storagePath)
            ?? throw new FileNotFoundException($"File '{storagePath}' was not found in storage.");

        return _storage.ReadStreamToString(stream, Encoding.UTF8);
    }
}
