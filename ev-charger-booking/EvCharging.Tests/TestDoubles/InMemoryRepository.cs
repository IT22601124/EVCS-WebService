using System.Linq.Expressions;
using EvCharging.Application.Contracts;

namespace EvCharging.Tests.TestDoubles;

public class InMemoryRepository<T> : IRepository<T> where T : class
{
    private readonly List<T> _store = new();

    public Task<T?> GetByIdAsync(string id)
    {
        var prop = typeof(T).GetProperty("Id");
        var val = _store.FirstOrDefault(x => prop?.GetValue(x)?.ToString() == id);
        return Task.FromResult(val);
    }

    public Task<List<T>> FindAsync(Expression<Func<T, bool>> filter)
    {
        var res = _store.AsQueryable().Where(filter).ToList();
        return Task.FromResult(res);
    }

    public Task<List<T>> GetAllAsync() => Task.FromResult(_store.ToList());

    public Task<T> InsertAsync(T entity)
    {
        _store.Add(entity);
        return Task.FromResult(entity);
    }

    public Task UpdateAsync(string id, T entity)
    {
        var prop = typeof(T).GetProperty("Id");
        var idx = _store.FindIndex(x => prop?.GetValue(x)?.ToString() == id);
        if (idx >= 0) _store[idx] = entity;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string id)
    {
        var prop = typeof(T).GetProperty("Id");
        var obj = _store.FirstOrDefault(x => prop?.GetValue(x)?.ToString() == id);
        if (obj != null) _store.Remove(obj);
        return Task.CompletedTask;
    }
}
