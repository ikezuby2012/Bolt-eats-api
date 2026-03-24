using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Application.Abstractions.Interface;

public interface IRepository<T> where T : class
{
    IEnumerable<T> GetAll(string? includeProperties = null);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken, string? includeProperties = null);
    T Get(Expression<Func<T, bool>> predicate, string? includeProperties = null, bool tracked = false);
    Task<T?> GetAsync(Expression<Func<T, bool>> predicate, string? includeProperties = null, bool tracked = false, CancellationToken cancellationToken = default);
    void Add(T entity);
    Task AddAsync(T entity);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
    Task AddRange(IEnumerable<T> entities, CancellationToken cancellationToken);
    Task<T> GetByIdAsync(int id);
    IQueryable<T> Query(Expression<Func<T, bool>> predicate);
    IQueryable<T> QueryAble();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken);
    Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken);
    Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken);
}
