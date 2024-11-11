using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Azure;
using SdnnGa.Model.Configuration;
using SdnnGa.Model.Infrastructure.Interfaces.AzureBlobStorage;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace SdnnGa.Infrastructure.AzureBlobStorage;

public class AzureBlobProvider : IAzureBlobProvider
{
    private readonly BlobServiceClient _blobServiceClient;

    public AzureBlobProvider(ConfigurationProvider configurationProvider)
    {
        _blobServiceClient = new BlobServiceClient(configurationProvider.AzureBlobConnectionString);
    }

    public async Task<Stream> GetFileAsync(string containerName, string fileName)
    {
        var client = GetClient(containerName, fileName);

        var stream = new MemoryStream();
        await client.DownloadToAsync(stream);
        stream.Position = 0;

        return stream;
    }

    public async Task PutFileAsync(string containerName, string fileName, Stream file, bool overwrite, Dictionary<string, string> tags = null)
    {
        var client = GetClient(containerName, fileName);
        BlobRequestConditions requestCondition = overwrite ? null : new BlobRequestConditions { IfNoneMatch = new ETag("*") };
        tags ??= new Dictionary<string, string>();
        await client.UploadAsync(file, new BlobUploadOptions { Tags = tags, Conditions = requestCondition }, CancellationToken.None);
    }

    public async Task<bool> ExistsAsync(string containerName, string fileName)
    {
        var client = GetClient(containerName, fileName);
        return await client.ExistsAsync();
    }

    public async Task DeleteAsync(string containerName, string fileName)
    {
        var client = GetClient(containerName, fileName);
        await client.DeleteIfExistsAsync();
    }

    public IEnumerable<string> GetListOfFileNamesByTag(string containerName, string tagName, string tagValue)
    {
        var container = _blobServiceClient.GetBlobContainerClient(containerName);
        var names = container.GetBlobs(BlobTraits.Tags).Where(w => w.Tags.TryGetValue(tagName, out var value) && value.Equals(tagValue, System.StringComparison.Ordinal)).Select(s => s.Name);
        return names.ToList();
    }

    private BlobClient GetClient(string containerName, string fileName)
    {
        var container = _blobServiceClient.GetBlobContainerClient(containerName);
        return container.GetBlobClient(fileName);
    }
}