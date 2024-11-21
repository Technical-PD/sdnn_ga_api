using SdnnGa.Model.Constants;
using SdnnGa.Model.Enums;
using SdnnGa.Model.Infrastructure.Interfaces.AzureBlobStorage;
using SdnnGa.Model.Services.Models.ServiceResult;
using SdnnGa.Model.Services;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Threading;
using SdnnGa.Model.Models;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;
using SdnnGa.Services.DataModel;

namespace SdnnGa.Services.Service;

public class DataService : IDataService
{
    private readonly IStorage _storage;
    private readonly ISessionService _sessionService;

    private const string FileExtensionRegex = "\\.[0-9a-z]+$";
    private const string HD5ContentType = "x-hdf5";
    private const string HD5Extentsion = "h5";

    public DataService(
        IStorage storage,
        ISessionService sessionService)
    {
        _storage = storage;
        _sessionService = sessionService;
    }

    public async Task<ServiceResult<Session>> PutTrainDataAsync(string sessiongId, MemoryStream memoryStream, DataType dataType, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessiongId))
        {
            return ServiceResult<Session>.FromError($"Argument null error. Argument '{nameof(sessiongId)}' can not be null");
        }

        if (memoryStream == null)
        {
            return ServiceResult<Session>.FromError($"Argument null error. Argument '{nameof(memoryStream)}' can not be null");
        }

        try
        {
            var session = await _sessionService.GetSessionAsync(sessiongId, cancellationToken);

            string filePath = dataType switch
            {
                DataType.XData => string.Format(StoragePath.XDataPath, sessiongId),
                DataType.YData => string.Format(StoragePath.YDataPath, sessiongId),
                _ => string.Empty
            };

            if (dataType == DataType.XData)
            {
                session.Entity.XTrainFileName = filePath;
            }
            else
            {
                session.Entity.YTrainFileName = filePath;
            }

            Console.WriteLine(session.Entity.XTrainFileName);
            Console.WriteLine(session.Entity.YTrainFileName);

            var sessionUpdated = await _sessionService.UpdateSessionAssync(session.Entity, cancellationToken);

            if (string.IsNullOrWhiteSpace(filePath))
            {
                return ServiceResult<Session>.FromError($"Invalid DataType");
            }

            memoryStream.Position = 0;
            await _storage.PutFileAsync(filePath, memoryStream, true);
            return ServiceResult<Session>.FromSuccess(sessionUpdated.Entity);
        }
        catch (Exception ex)
        {
            return ServiceResult<Session>.FromUnexpectedError($"Unnexpected error occured on putting XData to the storage for session '{sessiongId}'. Message: '{ex.Message}'");
        }
    }

    public async Task<ServiceResult<IFormFile>> GetDataFromStorageAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return ServiceResult<IFormFile>.FromError("FilePath is null or empty.");
        }

        try
        {
            var stream = await _storage.GetFileAsync(filePath);

            if (stream == null)
            {
                return ServiceResult<IFormFile>.FromNotFound($"Data does not exist in storage.");
            }

            return ServiceResult<IFormFile>.FromSuccess(CreateFormFile(stream, filePath));
        }
        catch (Exception ex)
        {
            return ServiceResult<IFormFile>.FromUnexpectedError(ex.Message);
        }
    }

    private string GetFileExtension(string fileName)
    {
        var regex = new Regex(FileExtensionRegex, RegexOptions.IgnoreCase);
        return regex.Match(fileName).Value.TrimStart('.');
    }

    private IFormFile CreateFormFile(Stream stream, string fileName)
    {
        var ext = GetFileExtension(fileName);

        var contentType = ext == HD5Extentsion ? $"{HD5ContentType}/hdf5" : $"application/{ext}";

        var generatedFileName = $"{Path.GetFileNameWithoutExtension(fileName)}-{DateTime.UtcNow:HHmmss}.{ext}";

        var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        memoryStream.Position = 0;

        var formFile = new CustomFormFile(memoryStream, generatedFileName, contentType);
        return formFile;
    }
}
