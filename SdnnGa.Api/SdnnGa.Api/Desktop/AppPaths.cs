using System;
using System.IO;

namespace SdnnGa.Api.Desktop;

/// <summary>
/// Розташування локальних даних застосунку.
/// </summary>
public sealed class AppPaths
{
    public AppPaths(string configuredDataDirectory)
    {
        DataDirectory = string.IsNullOrWhiteSpace(configuredDataDirectory)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SdnnGa")
            : Path.GetFullPath(Environment.ExpandEnvironmentVariables(configuredDataDirectory));

        Directory.CreateDirectory(DataDirectory);
        Directory.CreateDirectory(StorageDirectory);
        Directory.CreateDirectory(LogDirectory);
    }

    public string DataDirectory { get; }

    public string DatabasePath => Path.Combine(DataDirectory, "sdnnga.db");

    public string StorageDirectory => Path.Combine(DataDirectory, "storage");

    public string LogDirectory => Path.Combine(DataDirectory, "logs");

    public string WebViewDataDirectory => Path.Combine(DataDirectory, "webview");
}
