using System;
using System.Collections.Generic;

namespace SdnnGa.Model.Models;

/// <summary>
/// Стан сесії для UI: наскільки вона налаштована, чи працює генетичний алгоритм і які найкращі результати.
/// </summary>
public class SessionOverview
{
    public string Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public DateTime RecCreated { get; set; }

    public bool HasData { get; set; }

    public bool HasFitConfig { get; set; }

    public bool HasGeneticConfig { get; set; }

    public bool IsRunning { get; set; }

    /// <summary><see cref="SessionStatus"/>.</summary>
    public string Status { get; set; }

    public int EpochCount { get; set; }

    /// <summary>Максимальна кількість поколінь з генетичного конфігу.</summary>
    public int? MaxEpoches { get; set; }

    public string SelectionCriterion { get; set; }

    public int ModelCount { get; set; }

    public int TrainedModelCount { get; set; }

    public int FailedModelCount { get; set; }

    public int PendingModelCount { get; set; }

    /// <summary>Найкраща модель сесії за критерієм відбору (лише успішно навчені).</summary>
    public string BestModelId { get; set; }

    public string BestModelName { get; set; }

    public string BestModelEpochId { get; set; }

    public float? BestModelAccuracy { get; set; }

    public float? BestModelLoss { get; set; }

    /// <summary>Підсумки по поколіннях (заповнюється лише для однієї сесії).</summary>
    public List<EpochOverview> Epochs { get; set; }
}

public class EpochOverview
{
    public string Id { get; set; }

    public int EpochNo { get; set; }

    public DateTime RecCreated { get; set; }

    public int ModelCount { get; set; }

    public int TrainedModelCount { get; set; }

    public int FailedModelCount { get; set; }

    public int PendingModelCount { get; set; }

    public float? BestAccuracy { get; set; }

    public float? BestLoss { get; set; }
}

public static class SessionStatus
{
    public const string NeedsSetup = "NeedsSetup";
    public const string Ready = "Ready";
    public const string Running = "Running";
    public const string Finished = "Finished";
}
