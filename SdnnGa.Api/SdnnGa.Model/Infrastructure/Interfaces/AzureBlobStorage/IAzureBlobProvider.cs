using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SdnnGa.Model.Infrastructure.Interfaces.AzureBlobStorage;

public interface IAzureBlobProvider
{
    Task<Stream> GetFileAsync(string containerName, string fileName);

    Task PutFileAsync(string containerName, string fileName, Stream file, bool overwrite, Dictionary<string, string> tags = null);

    Task<bool> ExistsAsync(string containerName, string fileName);

    Task DeleteAsync(string containerName, string fileName);
}