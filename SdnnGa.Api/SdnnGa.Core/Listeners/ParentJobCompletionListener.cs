using Quartz;
using SdnnGa.Core.Classes;
using SdnnGa.Core.Jobs;
using SdnnGa.Model.Infrastructure.Interfaces.Quartz.Scheduler;
using SdnnGa.Model.Infrastructure.Interfaces.Training;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;
using SdnnGa.Model.Services;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using SdnnGa.Model.Services.Models.ServiceResult;
using SdnnGa.Model.Models;
using SdnnGa.Model.Constants;
using Microsoft.VisualBasic;

namespace SdnnGa.Core.Listeners;

public class ParentJobCompletionListener : IJobListener
{
    private readonly INeuralNetworkModelService _neuralNetworkModelService;
    private readonly IJobScheduler _jobScheduler;
    private readonly IServiceScope _scope;
    private readonly IScheduler _scheduler;
    private readonly IGeneticFlowStopRegistry _stopRegistry;

    private readonly Dictionary<string, string> _settings;
    private readonly List<IJobDetail> _childJobs;
    private readonly JobKey _parentJobKey;

    private readonly string _epochId;
    private readonly string _sessionId;
    private readonly string _selectionCriterion;
    private readonly float _stopLossValue;
    private readonly float _stopAccValue;
    private readonly int _maxEpochNo;
    private readonly int _epocheNo;

    // 1 — покоління вже оброблене (кілька дочірніх джоб можуть завершитися одночасно).
    private int _handled;

    public ParentJobCompletionListener(
        INeuralNetworkModelService neuralNetworkModelService,
        IScheduler scheduler,
        IGeneticFlowStopRegistry stopRegistry,
        IJobScheduler jobScheduler,
        IServiceScope scope,
        JobKey parentJobKey,
        List<IJobDetail> childJobs,
        Dictionary<string, string> settings,
        int epocheNo,
        int maxEpochNo,
        string sessionId,
        string epochId,
        string selectionCriterion,
        float stopLossValue,
        float stopAccValue)
    {
        _neuralNetworkModelService = neuralNetworkModelService;
        _scheduler = scheduler;
        _stopRegistry = stopRegistry;
        _jobScheduler = jobScheduler;
        _scope = scope;
        _parentJobKey = parentJobKey;
        _childJobs = childJobs;
        _settings = settings;
        _epocheNo = epocheNo;
        _maxEpochNo = maxEpochNo;
        _sessionId = sessionId;
        _stopLossValue = stopLossValue;
        _stopAccValue = stopAccValue;
        _epochId = epochId;
        _selectionCriterion = selectionCriterion;
    }

    // Quartz ідентифікує слухачів за назвою: однакова назва для всіх сесій означала, що слухач нової сесії
    // заміняв слухача іншої, і та сесія більше не переходила до наступного покоління.
    public string Name => $"ParentJobCompletionListener-{_epochId}";

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

        if (allChildJobsCompleted && Interlocked.Exchange(ref _handled, 1) == 0)
        {
            // Покоління завершене — слухач більше не потрібен.
            _scheduler.ListenerManager.RemoveJobListener(Name);

            ServiceResult<List<NeuralNetworkModel>> trainedModelInLastEpocheResult;

            while (true)
            {
                trainedModelInLastEpocheResult = await _neuralNetworkModelService.GetModelByEpochIdAsync(_epochId, false, cancellationToken);
                if (trainedModelInLastEpocheResult.Entity != null && trainedModelInLastEpocheResult.Entity.All(x => x.IsFinished))
                {
                    break;
                }

                await Task.Delay(5000);
            }
            
            Console.WriteLine("####################################################################\n");
            Console.WriteLine("ParentJobCompletionListener - All jobs are completed for the current job.\n");
            await _scheduler.DeleteJob(_parentJobKey);

            foreach (var childJob in _childJobs)
            {
                await _scheduler.DeleteJob(childJob.Key);
            }

            var bestModel = BestModelSelector.Select(trainedModelInLastEpocheResult?.Entity, _selectionCriterion);

            if (bestModel == null)
            {
                Console.WriteLine($"No model was trained successfully in epoch {_epocheNo - 1} of session '{_sessionId}'. Genetic flow is stopped.");
                Console.WriteLine("####################################################################");
                _scope.Dispose();
                return;
            }

            Console.WriteLine("#------------------------------------------------------------------------------#\n");
            Console.WriteLine($"#{_epocheNo} < {_maxEpochNo} && {bestModel.AccuracyValue} < {_stopAccValue} && {bestModel.LossValue} > {_stopLossValue}" +
                $" == {_epocheNo < _maxEpochNo && bestModel.AccuracyValue < _stopAccValue && bestModel.LossValue > _stopLossValue}#\n");
            Console.WriteLine("#------------------------------------------------------------------------------#\n");

            if (_stopRegistry.IsStopRequested(_sessionId))
            {
                Console.WriteLine($"Genetic flow of session '{_sessionId}' was stopped by the user.");
            }
            else if (_epocheNo < _maxEpochNo && bestModel.AccuracyValue < _stopAccValue && bestModel.LossValue > _stopLossValue)
            {
                Console.WriteLine($"Planning next epoch: {_epocheNo + 1}");
                await _jobScheduler.ScheduleJobAsync<GeneticEpochJob>($"GeneticEpochRun-{Guid.NewGuid()}", $"Epoche-{_epocheNo + 1}-{_sessionId}", _settings);
            }
            else
            {
                Console.WriteLine("No more epochs to schedule.");
            }

            Console.WriteLine("####################################################################");

            _scope.Dispose();
        }
    }

    public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
