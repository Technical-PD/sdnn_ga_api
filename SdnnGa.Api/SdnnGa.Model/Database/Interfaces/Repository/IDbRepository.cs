using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SdnnGa.Model.Database.Interfaces.Repository;

public interface IDbRepository<T>
{
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<T> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);

    Task<T> DeleteAsync(string id, CancellationToken cancellationToken = default);
}