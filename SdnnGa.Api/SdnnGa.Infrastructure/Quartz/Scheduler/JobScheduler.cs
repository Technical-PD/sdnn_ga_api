using Quartz;
using SdnnGa.Model.Infrastructure.Interfaces.Quartz.Scheduler;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace SdnnGa.Infrastructure.Quartz.Scheduler;

public class JobScheduler : IJobScheduler
{
    private readonly IScheduler _scheduler;

    public JobScheduler(IScheduler scheduler)
    {
        _scheduler = scheduler;
    }

    public async Task ScheduleJob<T>(string jobTypeName, Dictionary<string, string> jobSettings) where T : IJob
    {
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
        await _scheduler.ScheduleJob(job, trigger);
    }
}
