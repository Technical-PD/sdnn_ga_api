using Quartz;
using System.Threading.Tasks;

namespace SdnnGa.Model.Infrastructure.Interfaces.Quartz.Scheduler;

public interface ISchedulerService
{
    Task<IScheduler> GetSchedulerAsync();
}
