namespace SdnnGa.Api.Desktop;

/// <summary>
/// Секція <c>Desktop</c> в appsettings.json.
/// </summary>
public class DesktopOptions
{
    /// <summary>
    /// Тека з базою даних, файлами сесій і логами. Порожньо — <c>%LOCALAPPDATA%\SdnnGa</c>.
    /// Підтримує змінні середовища (<c>%USERPROFILE%\SdnnGaData</c>).
    /// </summary>
    public string DataDirectory { get; set; }

    /// <summary>Порт локального сервера на 127.0.0.1. 0 — вибрати вільний автоматично.</summary>
    public int Port { get; set; }

    /// <summary>
    /// Лише сервер без вікна (для розробки UI через <c>npm run dev</c>).
    /// Також вмикається аргументом <c>--headless</c>.
    /// </summary>
    public bool Headless { get; set; }

    public string WindowTitle { get; set; } = "SDNN-GA";

    public int WindowWidth { get; set; } = 1440;

    public int WindowHeight { get; set; } = 900;
}
