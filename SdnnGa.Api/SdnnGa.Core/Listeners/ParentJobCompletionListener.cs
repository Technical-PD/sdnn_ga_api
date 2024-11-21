using Quartz;
using SdnnGa.Core.Jobs;
using SdnnGa.Model.Infrastructure.Interfaces.Quartz.Scheduler;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace SdnnGa.Core.Listeners;

public class ParentJobCompletionListener : IJobListener
{
    private readonly IScheduler _scheduler;
    private readonly IJobScheduler _jobScheduler;
    private readonly JobKey _parentJobKey;
    private readonly List<IJobDetail> _childJobs;
    private readonly Dictionary<string, string> _settings;

    private readonly int _epocheNo;
    private readonly int _maxEpochNo;
    private readonly string _sessionId;

    public ParentJobCompletionListener(
        IScheduler scheduler,
        IJobScheduler jobScheduler,
        JobKey parentJobKey,
        List<IJobDetail> childJobs,
        Dictionary<string, string> settings,
        int epocheNo,
        int maxEpochNo,
        string sessionId)
    {
        _scheduler = scheduler;
        _jobScheduler = jobScheduler;
        _parentJobKey = parentJobKey;
        _childJobs = childJobs;
        _settings = settings;
        _epocheNo = epocheNo;
        _maxEpochNo = maxEpochNo;
        _sessionId = sessionId;
    }

    public string Name => "ParentJobCompletionListener";

    public async Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException, CancellationToken cancellationToken = default)
    {
        bool allChildJobsCompleted = true;

        Console.WriteLine($"==============> ParentJobCompletionListener - Listener is working. Сhild jobs to verify - {_childJobs.Count}");

        foreach (var childJob in _childJobs)
        {
            var triggers = await _scheduler.GetTriggersOfJob(childJob.Key);
            if (triggers.Any(t => t.GetNextFireTimeUtc().HasValue))
            {
                allChildJobsCompleted = false;
                Console.WriteLine($"Child job {childJob.Key} is either scheduled or hasn't fired yet.");
                break;
            }
        }

        if (allChildJobsCompleted)
        {
            Console.WriteLine("####################################################################\n");
            Console.WriteLine("ParentJobCompletionListener - All jobs are completed for the current job.\n");
            await _scheduler.DeleteJob(_parentJobKey);


            foreach (var childJob in _childJobs)
            {
                await _scheduler.DeleteJob(childJob.Key);
            }

            if (_epocheNo < _maxEpochNo)
            {
                Console.WriteLine($"Planning next epoch: {_epocheNo + 1}");
                await _jobScheduler.ScheduleJobAsync<GeneticEpochJob>($"GeneticEpochRun-{Guid.NewGuid()}", $"Epoche-{_epocheNo + 1}-{_sessionId}", _settings);
            }
            else
            {
                Console.WriteLine("No more epochs to schedule.");
            }

            Console.WriteLine("####################################################################");
        }
    }

    public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
