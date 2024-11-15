using Quartz;
using SdnnGa.Model.Infrastructure.Interfaces.Quartz.Scheduler;
using System.Threading.Tasks;

namespace SdnnGa.Infrastructure.Quartz.Scheduler;

public class SchedulerService : ISchedulerService
{
    private readonly ISchedulerFactory _schedulerFactory;

    private IScheduler _scheduler;

    public SchedulerService(ISchedulerFactory schedulerFactory)
    {
        _schedulerFactory = schedulerFactory;
    }

    public async Task<IScheduler> GetSchedulerAsync()
    {
        if (_scheduler == null)
        {
            _scheduler = await _schedulerFactory.GetScheduler();
            var jobCompleteListener = new JobCompleteListener(_scheduler);
            _scheduler.ListenerManager.AddJobListener(jobCompleteListener);
        }

        return _scheduler;
    }
}
