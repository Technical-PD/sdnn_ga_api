using SdnnGa.Model.Services.Models.ServiceResult.Enums;

namespace SdnnGa.Model.Services.Models.ServiceResult;

public class BaseResult
{
    public BaseResult(
        ResultStatus status,
        string message)
    {
        this.Status = status;
        this.Message = message;
    }

    public ResultStatus Status { get; set; }

    public string Message { get; set; }

    public bool IsSuccessful => this.Status == ResultStatus.Success || this.Status == ResultStatus.NotFound;

    public bool IsError => this.Status == ResultStatus.Error || this.Status == ResultStatus.UnexpectedError;
}