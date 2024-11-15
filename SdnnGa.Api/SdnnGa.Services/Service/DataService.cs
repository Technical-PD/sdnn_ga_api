using SdnnGa.Model.Constants;
using SdnnGa.Model.Enums;
using SdnnGa.Model.Infrastructure.Interfaces.AzureBlobStorage;
using SdnnGa.Model.Services.Models.ServiceResult;
using SdnnGa.Model.Services;
using System.IO;
using System.Threading.Tasks;
using System;

namespace SdnnGa.Services.Service;

public class DataService : IDataService
{
    private readonly IStorage _storage;

    public DataService(IStorage storage)
    {
        _storage = storage;
    }

    public async Task<ServiceResult> PutTrainDataAsync(string sessiongId, MemoryStream memoryStream, DataType dataType)
    {
        if (string.IsNullOrWhiteSpace(sessiongId))
        {
            return ServiceResult.FromError($"Argument null error. Argument '{nameof(sessiongId)}' can not be null");
        }

        if (memoryStream == null)
        {
            return ServiceResult.FromError($"Argument null error. Argument '{nameof(memoryStream)}' can not be null");
        }

        try
        {
            string filePath = dataType switch
            {
                DataType.XData => string.Format(StoragePath.XDataPath, sessiongId),
                DataType.YData => string.Format(StoragePath.YDataPath, sessiongId),
                _ => string.Empty
            };

            if (string.IsNullOrWhiteSpace(filePath))
            {
                return ServiceResult.FromError($"Invalid DataType");
            }

            memoryStream.Position = 0;
            await _storage.PutFileAsync(filePath, memoryStream, true);
            return ServiceResult.FromSuccess();
        }
        catch (Exception ex)
        {
            return ServiceResult.FromUnexpectedError($"Unnexpected error occured on putting XData to the storage for session '{sessiongId}'. Message: '{ex.Message}'");
        }
    }
}
