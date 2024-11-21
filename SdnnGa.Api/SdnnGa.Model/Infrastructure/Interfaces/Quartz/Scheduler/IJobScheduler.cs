using Quartz;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SdnnGa.Model.Infrastructure.Interfaces.Quartz.Scheduler;

public interface IJobScheduler
{
    Task ScheduleJobAsync<T>(string jobTypeName, string groupName, Dictionary<string, string> jobSettings) where T : IJob;

    Task ScheduleSequentialJobsAsync<T1, T2>(
        string firstJobName,
        string secondJobName,
        Dictionary<string, string> firstJobSettings,
        Dictionary<string, string> secondJobSettings)
        where T1 : IJob
        where T2 : IJob;

    Task ScheduleSequentialParentJobsAsync<TParent>(
        string parentJobBaseName,
        int numberOfParentJobs,
        Func<int, Dictionary<string, string>> parentJobSettingsFactory)
        where TParent : IJob;
}
