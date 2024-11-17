using SdnnGa.Model.Enums;
using SdnnGa.Model.Models;
using SdnnGa.Model.Services.Models.ServiceResult;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SdnnGa.Model.Services;

public interface IDataService
{
    Task<ServiceResult<Session>> PutTrainDataAsync(string sessiongId, MemoryStream memoryStream, DataType dataType, CancellationToken cancellationToken = default);
}
