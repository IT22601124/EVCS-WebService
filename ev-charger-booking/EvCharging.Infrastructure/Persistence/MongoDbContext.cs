using EvCharging.Infrastructure.Config;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace EvCharging.Infrastructure.Persistence;

public class MongoDbContext
{
    public IMongoDatabase Database { get; }

    public MongoDbContext(IOptions<MongoSettings> options)
    {
        var client = new MongoClient(options.Value.ConnectionString);
        Database = client.GetDatabase(options.Value.Database);
    }

    public IMongoCollection<T> GetCollection<T>(string? name = null)
        => Database.GetCollection<T>(name ?? typeof(T).Name);
}