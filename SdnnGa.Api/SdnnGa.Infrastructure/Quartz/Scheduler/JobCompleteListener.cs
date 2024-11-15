using Quartz;
using System.Threading;
using System.Threading.Tasks;

namespace SdnnGa.Model.Infrastructure.Interfaces.Quartz.Scheduler;

public class JobCompleteListener : IJobListener
{
    private readonly IScheduler _scheduler;

    public JobCompleteListener(IScheduler scheduler)
    {
        _scheduler = scheduler;
    }

    public string Name => "JobCompletionListener";

    public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public async Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException, CancellationToken cancellationToken = default)
    {
        var jobKey = context.JobDetail.Key;
        await _scheduler.DeleteJob(jobKey);
    }
}
