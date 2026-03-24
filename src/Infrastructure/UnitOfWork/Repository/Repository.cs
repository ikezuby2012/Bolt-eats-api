using System.Linq.Expressions;
using Application.Abstractions.Interface;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.UnitOfWork.Repository;

public class Repository<T> : IRepository<T> where T : class
{
    internal DbSet<T> dbSet;
    private static readonly char[] separator = new char[] { ',' };

    public Repository(ApplicationDbContext db)
    {
        dbSet = db.Set<T>();
    }

    public void Add(T entity)
    {
        dbSet.Add(entity);
    }

    public async Task AddAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await dbSet.AddAsync(entity);
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
    {
        return await dbSet.Where(predicate).ToListAsync(cancellationToken: cancellationToken);
    }

    public T Get(Expression<Func<T, bool>> predicate, string? includeProperties = null, bool tracked = false)
    {
        IQueryable<T> query = tracked ? dbSet : dbSet.AsNoTracking();
        query = query.Where<T>(predicate);

        if (includeProperties != null)
        {
            foreach (string property in includeProperties.Split(separator, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(property);
            }
        }

        return query.FirstOrDefault();
    }

    public IEnumerable<T> GetAll(string? includeProperties = null)
    {
        IQueryable<T> query = dbSet;

        if (!string.IsNullOrEmpty(includeProperties))
        {
            foreach (string property in includeProperties.Split(separator, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(property);
            }
        }
        return query.ToList();
    }

    public async Task<T> GetByIdAsync(int id)
    {
        T? entity = await dbSet.FindAsync(id);
        if (entity == null)
        {
            return null;
        }

        return entity;
    }

    public IQueryable<T> Query(Expression<Func<T, bool>> predicate)
    {
        return dbSet.AsQueryable().Where(predicate);
    }

    public IQueryable<T> QueryAble()
    {
        return dbSet.AsQueryable();
    }

    public void Remove(T entity)
    {
        dbSet.Remove(entity);
    }

    public void RemoveRange(IEnumerable<T> entities)
    {
        dbSet.RemoveRange(entities);
    }

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
    {
        try
        {
            return await dbSet.AnyAsync(predicate, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }

    }

    public async Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
    {
        return await dbSet.SingleOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken, string? includeProperties = null)
    {
        IQueryable<T> query = dbSet;

        if (!string.IsNullOrEmpty(includeProperties))
        {
            foreach (string property in includeProperties.Split(separator, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(property);
            }
        }
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<T?> GetAsync(Expression<Func<T, bool>> predicate, string? includeProperties = null, bool tracked = false, CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = tracked ? dbSet : dbSet.AsNoTracking();
        query = query.Where<T>(predicate);

        if (includeProperties != null)
        {
            foreach (string property in includeProperties.Split(separator, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(property);
            }
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddRange(IEnumerable<T> entities, CancellationToken cancellationToken)
    {
        if (entities == null || !entities.Any())
        {
            return;
        }

        await dbSet.AddRangeAsync(entities, cancellationToken);
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
    {
        return await dbSet.CountAsync(predicate, cancellationToken);
    }
}

