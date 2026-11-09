namespace SdnnGa.Infrastructure.Worker;

/// <summary>
/// Налаштування Python-воркера (секція <c>Worker</c> в appsettings.json).
/// </summary>
public class WorkerOptions
{
    /// <summary>
    /// Шлях до python.exe. Якщо порожній — використовується вбудований
    /// <c>{AppDir}/python/python.exe</c> (збірка build-desktop.ps1), а якщо його немає — <c>python</c> з PATH.
    /// </summary>
    public string PythonPath { get; set; }

    /// <summary>Шлях до sdnnga_worker.py. За замовчуванням — <c>{AppDir}/worker/sdnnga_worker.py</c>.</summary>
    public string ScriptPath { get; set; }

    /// <summary>Скільки чекати, поки воркер імпортує TensorFlow.</summary>
    public int StartupTimeoutSeconds { get; set; } = 300;

    public int CreateModelTimeoutSeconds { get; set; } = 300;

    /// <summary>Максимальний час навчання однієї моделі.</summary>
    public int FitModelTimeoutSeconds { get; set; } = 3600;
}
