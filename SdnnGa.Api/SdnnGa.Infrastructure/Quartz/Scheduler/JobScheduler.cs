using Quartz;
using SdnnGa.Model.Infrastructure.Interfaces.Quartz.Scheduler;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Quartz.Impl.Matchers;
using System.Runtime.CompilerServices;

namespace SdnnGa.Infrastructure.Quartz.Scheduler;

public class JobScheduler : IJobScheduler
{
    private readonly ISchedulerService _schedulerService;

    public JobScheduler(ISchedulerService schedulerService)
    {
        _schedulerService = schedulerService;
    }

    public async Task ScheduleJobAsync<T>(string jobTypeName, Dictionary<string, string> jobSettings) where T : IJob
    {
        var scheduler = await _schedulerService.GetSchedulerAsync();

        // Створюємо JobDataMap та додаємо в нього необхідні параметри
        var jobDataMap = new JobDataMap();

        foreach (var setting in jobSettings)
        {
            jobDataMap.Put(setting.Key, setting.Value);
        }

        // Створення JobDetail з JobDataMap
        var job = JobBuilder.Create<T>()
            .WithIdentity($"{jobTypeName}-{Guid.NewGuid()}")
            .UsingJobData(jobDataMap)
            .Build();

        // Тригер для виконання Job
        var trigger = TriggerBuilder.Create()
            .WithIdentity($"{jobTypeName}-trigger-{Guid.NewGuid()}")
            .StartNow()
            .Build();

        // Запланувати Job з тригером
        await scheduler.ScheduleJob(job, trigger);
    }

    // Удосконалений метод для послідовного виконання Job
    public async Task ScheduleSequentialJobsAsync<T1, T2>(string firstJobName, string secondJobName, Dictionary<string, string> firstJobSettings, Dictionary<string, string> secondJobSettings)
        where T1 : IJob
        where T2 : IJob
    {
        var scheduler = await _schedulerService.GetSchedulerAsync();

        // Створюємо першу джобу
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

        // Створюємо другу джобу
        var secondJobDataMap = new JobDataMap(secondJobSettings);
        var secondJobKey = new JobKey($"{secondJobName}");
        var secondJob = JobBuilder.Create<T2>()
            .WithIdentity(secondJobKey)
            .UsingJobData(secondJobDataMap)
            .StoreDurably()
            .Build();

        // Додаємо другу джобу до планувальника
        await scheduler.AddJob(secondJob, true);

        // Запланувати першу джобу з тригером
        await scheduler.ScheduleJob(firstJob, firstJobTrigger);

        // Додаємо тригер для виконання другої джоби після завершення першої
        var secondJobTrigger = TriggerBuilder.Create()
            .ForJob(secondJobKey)
            .WithIdentity($"{secondJobName}-trigger")
            .StartAt(firstJobTrigger.GetNextFireTimeUtc().Value.AddSeconds(20)) // Запустити через деякий час після першої
            .Build();

        await scheduler.ScheduleJob(secondJobTrigger);
    }
}
