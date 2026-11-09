using SdnnGa.Model.Models.Core.NNModel;
using SdnnGa.Model.Models.Core.Worker;
using System.Threading;
using System.Threading.Tasks;

namespace SdnnGa.Model.Infrastructure.Interfaces.Worker;

/// <summary>
/// Клієнт локального Python-воркера (TensorFlow), який будує та навчає моделі.
/// Замінює RabbitMQ-клієнти серверної версії.
/// </summary>
public interface IModelWorkerClient
{
    WorkerStatus Status { get; }

    /// <summary>Запускає воркер (якщо ще не запущений) і чекає, поки він завантажить TensorFlow.</summary>
    Task EnsureStartedAsync(CancellationToken cancellationToken = default);

    /// <summary>Будує Keras-модель за .NET-конфігом і повертає її JSON.</summary>
    Task<string> CreateModelAsync(ModelConfig modelConfig, CancellationToken cancellationToken = default);

    /// <summary>Навчає модель і повертає метрики, історію та ваги (base64).</summary>
    Task<FitResult> FitModelAsync(FitModelJobConfig fitConfig, CancellationToken cancellationToken = default);
}
