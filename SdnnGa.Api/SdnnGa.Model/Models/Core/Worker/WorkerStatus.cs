using System.Text.Json.Serialization;

namespace SdnnGa.Model.Models.Core.Worker;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WorkerState
{
    NotStarted,
    Starting,
    Ready,
    Failed
}

/// <param name="Progress">Прогрес запуску воркера 0–100 (етапи імпорту бібліотек), <c>null</c> — невідомо.</param>
public record WorkerStatus(
    WorkerState State,
    string Message = null,
    string PythonVersion = null,
    string TensorFlowVersion = null,
    int? Progress = null);
