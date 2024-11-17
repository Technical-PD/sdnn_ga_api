using Quartz;
using System.Threading.Tasks;
using System.Threading;

namespace SdnnGa.Infrastructure.Quartz.Scheduler;

public class SequentialJobListener<T> : IJobListener where T : IJob
{
    private readonly JobKey _nextJobKey;

    public SequentialJobListener(JobKey nextJobKey)
    {
        _nextJobKey = nextJobKey;
    }

    public string Name => "SequentialJobListener";

    public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public async Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException, CancellationToken cancellationToken = default)
    {
        if (jobException == null)
        {
            // Отримуємо планувальник і виконуємо наступну джобу
            var scheduler = context.Scheduler;
            if (await scheduler.CheckExists(_nextJobKey, cancellationToken))
            {
                await scheduler.TriggerJob(_nextJobKey, cancellationToken);
            }
        }
    }
}
