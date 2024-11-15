﻿using Quartz;
using SdnnGa.Model.Infrastructure.Interfaces.Quartz.Scheduler;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

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
}
