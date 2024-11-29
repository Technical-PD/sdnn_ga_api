using Microsoft.Extensions.DependencyInjection;
using Quartz;
using SdnnGa.Model.Infrastructure.Interfaces.Quartz.Scheduler;
using SdnnGa.Model.Services;
using System.Collections.Generic;
using System;

namespace SdnnGa.Core.Listeners;

public class ParentJobCompletionListenerFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ParentJobCompletionListenerFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ParentJobCompletionListener Create(
        JobKey parentJobKey,
        IJobScheduler jobScheduler,
        IScheduler scheduler,
        List<IJobDetail> childJobs,
        Dictionary<string, string> settings,
        int epocheNo,
        int maxEpochNo,
        string sessionId,
        string epochId,
        float stopLossValue,
        float stopAccValue,
        string selectionCriterion)
    {
        var scope = _serviceProvider.CreateScope();

        var neuralNetworkModelService = scope.ServiceProvider.GetRequiredService<INeuralNetworkModelService>();

        return new ParentJobCompletionListener(
            neuralNetworkModelService: neuralNetworkModelService,
            scheduler: scheduler,
            jobScheduler: jobScheduler,
            scope: scope,
            parentJobKey: parentJobKey,
            childJobs: childJobs,
            settings: settings,
            epocheNo: epocheNo,
            maxEpochNo: maxEpochNo,
            sessionId: sessionId,
            epochId: epochId,
            selectionCriterion: selectionCriterion,
            stopLossValue: stopLossValue,
            stopAccValue: stopAccValue);
    }
}
