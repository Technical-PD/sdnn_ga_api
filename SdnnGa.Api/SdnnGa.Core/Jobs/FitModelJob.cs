using Quartz;
using SdnnGa.Model.Constants;
using SdnnGa.Model.Core.Interfaces;
using SdnnGa.Model.Infrastructure.Interfaces.AzureBlobStorage;
using SdnnGa.Model.Infrastructure.Interfaces.RabbitMq;
using SdnnGa.Model.Models.Core.NNModel;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SdnnGa.Core.Jobs;

public class FitModelJob : ICreateModelJob
{
    private readonly IRabbitMqClient _rabbitMqClient;
    private readonly IStorage _storage;
    private readonly int _timeout = 1800;

    private string _fitModelJobConfig;
    private string _weightPath;

    public FitModelJob(
        IRabbitMqClient rabbitMqClient,
        IStorage storage)
    {
        _rabbitMqClient = rabbitMqClient;
        _storage = storage;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await InizializeSettingValuesAsync(context);

        var result = await _rabbitMqClient.SendMessageAsync(_fitModelJobConfig, timeoutInSeconds: _timeout);

        var fitResult = JsonSerializer.Deserialize<FitResult>(result);

        using (var file = new MemoryStream(Encoding.UTF8.GetBytes(fitResult.Weights)))
        {
            file.Position = 0;

            await _storage.PutFileAsync(_weightPath, file, true);
        }
    }

    private async Task InizializeSettingValuesAsync(IJobExecutionContext context)
    {
        var jobDataMap = context.JobDetail.JobDataMap;

        var fitModelJobConfig = new FitModelJobConfig
        {
            CountOfLines = jobDataMap.GetString(JobSettings.FitModelJob.CountOfLinesSettingName),
            CountOfInputs = jobDataMap.GetString(JobSettings.FitModelJob.CountOfInputsSettingName),
            CountOfOutputs = jobDataMap.GetString(JobSettings.FitModelJob.CountOfOutputsSettingName),
            UseEarlyStopping = jobDataMap.GetString(JobSettings.FitModelJob.UseEarlyStoppingSettingName),
            MinDelta = jobDataMap.GetString(JobSettings.FitModelJob.MinDeltaSettingName),
            Patience = jobDataMap.GetString(JobSettings.FitModelJob.PatienceSettingName),
            IsLearnWithValidation = jobDataMap.GetString(JobSettings.FitModelJob.IsLearnWithValidationSettingName),
            Optimizer = jobDataMap.GetString(JobSettings.FitModelJob.OptimizerSettingName),
            LossFunc = jobDataMap.GetString(JobSettings.FitModelJob.LossFuncSettingName),
            Epochs = jobDataMap.GetString(JobSettings.FitModelJob.EpochsSettingName),
            BatchSize = jobDataMap.GetString(JobSettings.FitModelJob.BatchSizeSettingName),
        };

        string modelConfigPath = jobDataMap.GetString(JobSettings.FitModelJob.ModelConfigJsonPathSettingName);
        using (var memoryStream = await _storage.GetFileAsync(modelConfigPath))
        {
            fitModelJobConfig.ModelConfigJson = _storage.ReadStreamToString(memoryStream, Encoding.UTF8);
        }

        string xTrainPath = jobDataMap.GetString(JobSettings.FitModelJob.ModelConfigJsonPathSettingName);
        using (var memoryStream = await _storage.GetFileAsync(xTrainPath))
        {
            fitModelJobConfig.XTrain = _storage.ReadStreamToString(memoryStream, Encoding.UTF8);
        }

        string yTrainPath = jobDataMap.GetString(JobSettings.FitModelJob.ModelConfigJsonPathSettingName);
        using (var memoryStream = await _storage.GetFileAsync(yTrainPath))
        {
            fitModelJobConfig.YTrain = _storage.ReadStreamToString(memoryStream, Encoding.UTF8);
        }

        _fitModelJobConfig = JsonSerializer.Serialize(fitModelJobConfig);
        _weightPath = jobDataMap.GetString(JobSettings.FitModelJob.WeightPathSettingName);
    }
}
