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

namespace SdnnGa.Services.Service;

public class DataService : IDataService
{
    private readonly IStorage _storage;
    private readonly ISessionService _sessionService;

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
}
