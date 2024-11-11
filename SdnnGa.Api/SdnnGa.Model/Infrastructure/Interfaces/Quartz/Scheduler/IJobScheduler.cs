using Quartz;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SdnnGa.Model.Infrastructure.Interfaces.Quartz.Scheduler;

public interface IJobScheduler
{
    Task ScheduleJob<T>(string jobTypeName, Dictionary<string, string> jobSettings) where T : IJob;
}
