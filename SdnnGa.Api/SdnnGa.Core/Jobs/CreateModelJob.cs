using Quartz;
using SdnnGa.Model.Constants;
using SdnnGa.Model.Core.Interfaces;
using SdnnGa.Model.Infrastructure.Interfaces.AzureBlobStorage;
using SdnnGa.Model.Infrastructure.Interfaces.RabbitMq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SdnnGa.Core.Jobs;

public class CreateModelJob : ICreateModelJob
{
    private string _modelConfig;
    private string _modelStoragePath;
    
    private readonly IRabbitMqClient _rabbitMqClient;
    private readonly IStorage _storage;

    public CreateModelJob(
        IRabbitMqClient rabbitMqClient,
        IStorage storage)
    {
        _rabbitMqClient = rabbitMqClient;
        _storage = storage;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        InizializeSettingValues(context);

        var result = await _rabbitMqClient.SendMessageAsync(_modelConfig, timeoutInSecconds: 30);

        using (var file = new MemoryStream(Encoding.UTF8.GetBytes(result)))
        {
            file.Position = 0;
            await _storage.PutFileAsync(_modelStoragePath, file, true);
        }

        _rabbitMqClient.Close();
    }

    private void InizializeSettingValues(IJobExecutionContext context)
    {
        var jobDataMap = context.JobDetail.JobDataMap;

        _modelConfig = jobDataMap.GetString(JobSettings.CreateModel.ModelConfigSettingName);
        _modelStoragePath = jobDataMap.GetString(JobSettings.CreateModel.ModelStoragePathSettingName);
    }
}
