using Quartz;
using SdnnGa.Model.Infrastructure.Interfaces.Quartz.Scheduler;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace SdnnGa.Infrastructure.Quartz.Scheduler;

public class JobScheduler : IJobScheduler
{
    private const string JobKeyTemaplate = "{0}-{1}";
    private const string TriggerNameTemaplate = "{0}-trigger-{1}";

    private readonly ISchedulerService _schedulerService;

    public JobScheduler(ISchedulerService schedulerService)
    {
        _schedulerService = schedulerService;
    }

    public async Task ScheduleJobAsync<T>(string jobTypeName, string groupName, Dictionary<string, string> jobSettings) where T : IJob
    {
        var scheduler = await _schedulerService.GetSchedulerAsync();

        var jobDataMap = new JobDataMap();

        foreach (var setting in jobSettings)
        {
            jobDataMap.Put(setting.Key, setting.Value);
        }

        var job = JobBuilder.Create<T>()
            .WithIdentity(string.Format(JobKeyTemaplate, jobTypeName, Guid.NewGuid()), groupName)
            .UsingJobData(jobDataMap)
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity(string.Format(TriggerNameTemaplate, jobTypeName, Guid.NewGuid()), groupName)
            .StartNow()
            .Build();

        await scheduler.ScheduleJob(job, trigger);
    }

    public async Task ScheduleSequentialJobsAsync<T1, T2>(string firstJobName, string secondJobName, Dictionary<string, string> firstJobSettings, Dictionary<string, string> secondJobSettings)
        where T1 : IJob
        where T2 : IJob
    {
        var scheduler = await _schedulerService.GetSchedulerAsync();

        var firstJobDataMap = new JobDataMap(firstJobSettings);
        var firstJobKey = new JobKey($"{firstJobName}");
        var firstJob = JobBuilder.Create<T1>()
            .WithIdentity(firstJobKey)
            .UsingJobData(firstJobDataMap)
            .Build();

        var firstJobTrigger = TriggerBuilder.Create()
            .WithIdentity($"{firstJobName}-trigger")
            .StartNow()
            .Build();

        var secondJobDataMap = new JobDataMap(secondJobSettings);
        var secondJobKey = new JobKey($"{secondJobName}");
        var secondJob = JobBuilder.Create<T2>()
            .WithIdentity(secondJobKey)
            .UsingJobData(secondJobDataMap)
            .StoreDurably()
            .Build();

        await scheduler.AddJob(secondJob, true);

        await scheduler.ScheduleJob(firstJob, firstJobTrigger);

        var secondJobTrigger = TriggerBuilder.Create()
            .ForJob(secondJobKey)
            .WithIdentity($"{secondJobName}-trigger")
            .StartAt(firstJobTrigger.GetNextFireTimeUtc().Value.AddSeconds(5)) 
            .Build();

        await scheduler.ScheduleJob(secondJobTrigger);
    }

    public async Task ScheduleSequentialParentJobsAsync<TParent>(string parentJobBaseName, int numberOfParentJobs, Func<int, Dictionary<string, string>> parentJobSettingsFactory)
        where TParent : IJob
    {
        var scheduler = await _schedulerService.GetSchedulerAsync();

        for (int i = 0; i < numberOfParentJobs; i++)
        {
            var parentJobName = $"{parentJobBaseName}-{i}";
            var parentJobSettings = parentJobSettingsFactory(i);
            var parentJobDataMap = new JobDataMap(parentJobSettings);
            var parentJobKey = new JobKey(parentJobName);

            var parentJob = JobBuilder.Create<TParent>()
                .WithIdentity(parentJobKey)
                .UsingJobData(parentJobDataMap)
                .StoreDurably()
                .Build();

            await scheduler.AddJob(parentJob, true);

            var parentJobTrigger = TriggerBuilder.Create()
                .WithIdentity($"{parentJobName}-trigger")
                .StartNow()
                .Build();

            await scheduler.ScheduleJob(parentJob, parentJobTrigger);
        }
    }
}