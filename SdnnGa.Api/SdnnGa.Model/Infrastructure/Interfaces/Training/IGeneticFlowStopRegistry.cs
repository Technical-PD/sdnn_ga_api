namespace SdnnGa.Model.Infrastructure.Interfaces.Training;

/// <summary>
/// Сесії, для яких користувач зупинив генетичний пошук. Джоби та слухач перевіряють прапорець,
/// щоб не запускати наступне покоління. Прапорець знімається при новому запуску.
/// </summary>
public interface IGeneticFlowStopRegistry
{
    void RequestStop(string sessionId);

    void Clear(string sessionId);

    bool IsStopRequested(string sessionId);
}
