using Quartz;
using SdnnGa.Model.Infrastructure.Interfaces.AzureBlobStorage;
using SdnnGa.Model.Infrastructure.Interfaces.RabbitMq;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SdnnGa.Core.Jobs;

public class CreateModelJob : IJob
{
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
        var jobDataMap = context.JobDetail.JobDataMap;

        string modelConfig = jobDataMap.GetString("modelConfig");

        var result = await _rabbitMqClient.SendMessageAsync(modelConfig);

        using (var file = new MemoryStream(Encoding.UTF8.GetBytes(result)))
        {
            file.Position = 0;
            await _storage.PutFileAsync($"TestModels/{Guid.NewGuid()}.json", file, true);
        }

        _rabbitMqClient.Close();
    }
}
