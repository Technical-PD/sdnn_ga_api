using SdnnGa.Model.Services.Models.ServiceResult.Enums;

namespace SdnnGa.Model.Services.Models.ServiceResult;

public class ServiceResult : BaseResult
{
    public ServiceResult(
        ResultStatus status,
        string message)
        : base(status, message)
    {
    }

    public static ServiceResult FromSuccess()
    {
        return new ServiceResult(ResultStatus.Success, string.Empty);
    }

    public static ServiceResult FromNotFound(string message)
    {
        return new ServiceResult(ResultStatus.NotFound, message);
    }

    public static ServiceResult FromError(string message)
    {
        return new ServiceResult(ResultStatus.Error, message);
    }

    public static ServiceResult FromUnexpectedError(string message)
    {
        return new ServiceResult(ResultStatus.UnexpectedError, message);
    }
}