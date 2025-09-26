using System.Linq.Expressions;
using EvCharging.Application.Contracts;
using EvCharging.Infrastructure.Persistence;
using MongoDB.Driver;


namespace EvCharging.Infrastructure.Repositories;


public class Repository<T> : IRepository<T>
{
    private readonly IMongoCollection<T> _collection;


    public Repository(MongoDbContext ctx)
    {
        _collection = ctx.GetCollection<T>();
    }


    public async Task<T?> GetByIdAsync(string id)
    {
        var filter = Builders<T>.Filter.Eq("Id", id);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }


    public async Task<List<T>> FindAsync(Expression<Func<T, bool>> filter)
        => await _collection.Find(filter).ToListAsync();


    public async Task<List<T>> GetAllAsync()
        => await _collection.Find(Builders<T>.Filter.Empty).ToListAsync();


    public async Task<T> InsertAsync(T entity)
    {
        await _collection.InsertOneAsync(entity);
        return entity;
    }


    public async Task UpdateAsync(string id, T entity)
    {
        var filter = Builders<T>.Filter.Eq("Id", id);
        await _collection.ReplaceOneAsync(filter, entity);
    }


    public async Task DeleteAsync(string id)
    {
        var filter = Builders<T>.Filter.Eq("Id", id);
        await _collection.DeleteOneAsync(filter);
    }
}