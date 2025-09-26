using System.Linq.Expressions;

namespace EvCharging.Application.Contracts;

public interface IRepository<T>
{
    Task<T?> GetByIdAsync(string id);
    Task<List<T>> FindAsync(Expression<Func<T, bool>> filter);
    Task<List<T>> GetAllAsync();
    Task<T> InsertAsync(T entity);
    Task UpdateAsync(string id, T entity);
    Task DeleteAsync(string id);
}