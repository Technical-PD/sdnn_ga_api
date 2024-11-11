using Quartz;
using SdnnGa.Model.Infrastructure.Interfaces.RabbitMq;
using System.Threading.Tasks;

namespace SdnnGa.Core.Jobs;

public class CreateModelJob : IJob
{
    private readonly IRabbitMqClient _rabbitMqClient;

    public CreateModelJob(IRabbitMqClient rabbitMqClient)
    {
        _rabbitMqClient = rabbitMqClient;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var jobDataMap = context.JobDetail.JobDataMap;

        string modelConfig = jobDataMap.GetString("modelConfig");

        var result = await _rabbitMqClient.SendMessageAsync(modelConfig);

        await Task.Delay(1000);
    }
}
