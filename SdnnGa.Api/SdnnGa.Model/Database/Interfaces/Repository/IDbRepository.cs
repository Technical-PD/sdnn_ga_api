using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SdnnGa.Model.Database.Interfaces.Repository;

public interface IDbRepository<T>
{
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<T>> GetByFieldAsync(string fieldName, string fieldValue, bool useTracking = true, Func<IQueryable<T>, IQueryable<T>> include = null, CancellationToken cancellationToken = default);

    Task<T> GetByIdAsync(string id, Func<IQueryable<T>, IQueryable<T>> include = null, CancellationToken cancellationToken = default);

    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);

    Task<T> DeleteAsync(string id, CancellationToken cancellationToken = default);
}