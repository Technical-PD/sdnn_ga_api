using SdnnGa.Model.Services.Models.ServiceResult.Enums;

namespace SdnnGa.Model.Services.Models.ServiceResult;

public class ServiceResult<TEntity> : BaseResult where TEntity : class
{
    public TEntity Entity { get; set; }

    public ServiceResult(
        TEntity entity,
        ResultStatus status,
        string message)
        : base(status, message)
    {
        this.Entity = entity;
    }

    public static ServiceResult<TEntity> FromSuccess(TEntity entity)
    {
        return new ServiceResult<TEntity>(entity, ResultStatus.Success, string.Empty);
    }

    public static ServiceResult<TEntity> FromNotFound(string message)
    {
        return new ServiceResult<TEntity>(null, ResultStatus.NotFound, message);
    }

    public static ServiceResult<TEntity> FromError(string message)
    {
        return new ServiceResult<TEntity>(null, ResultStatus.Error, message);
    }
    public static ServiceResult<TEntity> FromUnexpectedError(string message)
    {
        return new ServiceResult<TEntity>(null, ResultStatus.UnexpectedError, message);
    }
}