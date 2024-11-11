using SdnnGa.Model.Infrastructure.Interfaces.AzureBlobStorage;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SdnnGa.Infrastructure.AzureBlobStorage;

public class AzureBlobStorage : IStorage
{
    private const string ContainerName = "models";
    private readonly IAzureBlobProvider _azureBlobProvider;

    public AzureBlobStorage(IAzureBlobProvider azureBlobProvider)
    {
        _azureBlobProvider = azureBlobProvider;
    }

    public async Task<Stream> GetFileAsync(string fileName)
    {
        return await _azureBlobProvider.GetFileAsync(ContainerName, fileName);
    }

    public async Task PutFileAsync(string fileName, Stream file, bool overwrite, Dictionary<string, string> tags = null)
    {
        await _azureBlobProvider.PutFileAsync(ContainerName, fileName, file, overwrite, tags);
    }

    public async Task<bool> ExistsAsync(string fileName)
    {
        return await _azureBlobProvider.ExistsAsync(ContainerName, fileName);
    }

    public async Task DeleteAsync(string fileName)
    {
        await _azureBlobProvider.DeleteAsync(ContainerName, fileName);
    }
}
