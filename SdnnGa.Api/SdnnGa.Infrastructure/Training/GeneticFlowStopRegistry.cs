using SdnnGa.Model.Infrastructure.Interfaces.Training;
using System.Collections.Concurrent;

namespace SdnnGa.Infrastructure.Training;

public class GeneticFlowStopRegistry : IGeneticFlowStopRegistry
{
    private readonly ConcurrentDictionary<string, bool> _stopped = new();

    public void RequestStop(string sessionId) => _stopped[sessionId] = true;

    public void Clear(string sessionId) => _stopped.TryRemove(sessionId, out _);

    public bool IsStopRequested(string sessionId) => !string.IsNullOrEmpty(sessionId) && _stopped.ContainsKey(sessionId);
}
