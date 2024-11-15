using SdnnGa.Model.Enums;
using SdnnGa.Model.Services.Models.ServiceResult;
using System.IO;
using System.Threading.Tasks;

namespace SdnnGa.Model.Services;

public interface IDataService
{
    Task<ServiceResult> PutTrainDataAsync(string sessiongId, MemoryStream memoryStream, DataType dataType);
}
