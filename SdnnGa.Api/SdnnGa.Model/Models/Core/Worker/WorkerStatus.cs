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

public record WorkerStatus(
    WorkerState State,
    string Message = null,
    string PythonVersion = null,
    string TensorFlowVersion = null);
