using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SdnnGa.Model.Infrastructure.Interfaces.Storage;

/// <summary>
/// Сховище файлів (дані сесій, конфіги моделей, ваги). Ключ — відносний шлях з <see cref="Constants.StoragePath"/>.
/// </summary>
public interface IStorage
{
    /// <returns>Потік з вмістом файлу або <c>null</c>, якщо файл не існує.</returns>
    Task<Stream> GetFileAsync(string fileName);

    Task PutFileAsync(string fileName, Stream file, bool overwrite, Dictionary<string, string> tags = null);

    Task<bool> ExistsAsync(string fileName);

    Task DeleteAsync(string fileName);

    /// <summary>Видаляє всі файли з ключами, що починаються з <paramref name="directory"/> (наприклад, теку сесії).</summary>
    Task DeleteDirectoryAsync(string directory);

    string ReadStreamToString(Stream stream, Encoding encoding);
}
