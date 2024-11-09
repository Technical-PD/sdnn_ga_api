using Microsoft.EntityFrameworkCore;
using SdnnGa.Model.Database.Interfaces.Repository;
using SdnnGa.Model.Database.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace SdnnGa.Database.Repository;

public class DbRepository<T> : IDbRepository<T> where T : BaseModel
{
    private readonly ApiDbContext _context;
    private readonly DbSet<T> _dbSet;

    public DbRepository(ApiDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context)); ;
        _dbSet = _context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet.ToListAsync(cancellationToken);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<T> GetByIdAsync(string id, Func<IQueryable<T>, IQueryable<T>> include = null, CancellationToken cancellationToken = default)
    {
        try
        {
            IQueryable<T> query = _dbSet;

            if (include != null)
            {
                query = include(query);
            }

            var entity = await query.FirstOrDefaultAsync(e => EF.Property<string>(e, "Id") == id, cancellationToken);

            if (entity == null)
            {
                throw new KeyNotFoundException($"Entity with ID {id} not found.");
            }

            return entity;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        try
        {
            entity.Id = Guid.NewGuid().ToString();
            entity.RecCreated = DateTime.UtcNow;
            entity.RecModified = DateTime.UtcNow;

            await _dbSet.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            throw;
        }

        return entity;
    }

    public async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        try
        {
            entity.RecModified = DateTime.UtcNow;

            _dbSet.Update(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            throw;
        }

        return entity;
    }

    public async Task<T> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        T entity = null;

        try
        {
            entity = await GetByIdAsync(id, null, cancellationToken);

            if (entity == null)
            {
                throw new KeyNotFoundException($"Entity with ID {id} not found.");
            }

            _dbSet.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            throw;
        }

        return entity;
    }
}