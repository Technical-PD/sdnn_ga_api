using Microsoft.EntityFrameworkCore;
using SdnnGa.Model.Database.Interfaces.Repository;
using SdnnGa.Model.Database.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was canceled in GetAllAsync.");
            return new List<T>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetAllAsync: {ex.Message}");
            return new List<T>();
        }
    }

    public async Task<T> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _dbSet.FindAsync(new object[] { id }, cancellationToken);

            if (entity == null)
            {
                throw new KeyNotFoundException($"Entity with ID {id} not found.");
            }

            return entity;
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was canceled in GetByIdAsync.");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetByIdAsync: {ex.Message}");
            return null;
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
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was canceled in AddAsync.");
        }
        catch (DbUpdateException dbEx)
        {
            Console.WriteLine($"Database update error in AddAsync: {dbEx.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in AddAsync: {ex.Message}");
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
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was canceled in UpdateAsync.");
        }
        catch (DbUpdateException dbEx)
        {
            Console.WriteLine($"Database update error in UpdateAsync: {dbEx.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in UpdateAsync: {ex.Message}");
        }

        return entity;
    }

    public async Task<T> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        T entity = null;

        try
        {
            entity = await GetByIdAsync(id, cancellationToken);

            if (entity == null)
            {
                throw new KeyNotFoundException($"Entity with ID {id} not found.");
            }

            _dbSet.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was canceled in DeleteAsync.");
        }
        catch (DbUpdateException dbEx)
        {
            Console.WriteLine($"Database update error in DeleteAsync: {dbEx.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in DeleteAsync: {ex.Message}");
        }

        return entity;
    }
}
