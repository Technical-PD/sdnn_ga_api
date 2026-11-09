using SdnnGa.Model.Infrastructure.Interfaces.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SdnnGa.Infrastructure.Storage;

/// <summary>
/// Зберігає файли на локальному диску в теці <c>rootDirectory</c>
/// (ключ <c>v1/session-.../x_data.csv</c> → <c>{root}/v1/session-.../x_data.csv</c>).
/// </summary>
public class FileSystemStorage : IStorage
{
    private readonly string _rootDirectory;

    public FileSystemStorage(string rootDirectory)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory))
        {
            throw new ArgumentException("Storage root directory must be specified.", nameof(rootDirectory));
        }

        _rootDirectory = Path.TrimEndingDirectorySeparator(Path.GetFullPath(rootDirectory));
        Directory.CreateDirectory(_rootDirectory);
    }

    public Task<Stream> GetFileAsync(string fileName)
    {
        var fullPath = ResolvePath(fileName);

        if (!File.Exists(fullPath))
        {
            return Task.FromResult<Stream>(null);
        }

        // Повертаємо копію в пам'яті, щоб не тримати файл відкритим (його можуть перезаписати паралельні джоби).
        var memoryStream = new MemoryStream(File.ReadAllBytes(fullPath));
        return Task.FromResult<Stream>(memoryStream);
    }

    public async Task PutFileAsync(string fileName, Stream file, bool overwrite, Dictionary<string, string> tags = null)
    {
        ArgumentNullException.ThrowIfNull(file);

        var fullPath = ResolvePath(fileName);

        if (!overwrite && File.Exists(fullPath))
        {
            throw new IOException($"File '{fileName}' already exists.");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        // Пишемо у тимчасовий файл і атомарно підміняємо, щоб не залишити напівзаписаний файл.
        var tempPath = fullPath + ".tmp";
        await using (var target = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await file.CopyToAsync(target);
        }

        File.Move(tempPath, fullPath, overwrite: true);
    }

    public Task<bool> ExistsAsync(string fileName) => Task.FromResult(File.Exists(ResolvePath(fileName)));

    public Task DeleteAsync(string fileName)
    {
        var fullPath = ResolvePath(fileName);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    public string ReadStreamToString(Stream stream, Encoding encoding)
    {
        if (stream == null)
        {
            return null;
        }

        using var reader = new StreamReader(stream, encoding);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Перетворює ключ сховища на абсолютний шлях і не дозволяє вийти за межі кореневої теки
    /// (ключ приходить у тому числі з клієнта через Data/GetDataFile).
    /// </summary>
    private string ResolvePath(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File name must be specified.", nameof(fileName));
        }

        var relative = fileName.Replace('\\', '/').TrimStart('/');
        var fullPath = Path.GetFullPath(Path.Combine(_rootDirectory, relative));

        if (!fullPath.StartsWith(_rootDirectory + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException($"Path '{fileName}' is outside of the storage directory.");
        }

        return fullPath;
    }
}
