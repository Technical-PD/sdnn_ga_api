using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SdnnGa.Services.DataModel;

public class CustomFormFile : IFormFile
{
    private readonly Stream _stream;

    public CustomFormFile(Stream stream, string fileName, string contentType)
    {
        _stream = stream;
        FileName = fileName;
        ContentType = contentType;
    }

    public Stream OpenReadStream() => _stream;

    public string ContentType { get; }
    public string ContentDisposition { get; set; }
    public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();
    public long Length => _stream.Length;
    public string Name { get; set; }
    public string FileName { get; }

    public void CopyTo(Stream target)
    {
        _stream.CopyTo(target);
    }

    public async Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
    {
        await _stream.CopyToAsync(target, cancellationToken);
    }
}
