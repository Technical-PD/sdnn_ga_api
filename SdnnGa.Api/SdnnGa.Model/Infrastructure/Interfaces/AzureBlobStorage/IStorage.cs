using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SdnnGa.Model.Infrastructure.Interfaces.AzureBlobStorage;

public interface IStorage
{
    Task<Stream> GetFileAsync(string fileName);

    Task PutFileAsync(string fileName, Stream file, bool overwrite, Dictionary<string, string> tags = null);

    Task<bool> ExistsAsync(string fileName);

    Task DeleteAsync(string fileName);

    string ReadStreamToString(Stream stream, Encoding encoding);
}