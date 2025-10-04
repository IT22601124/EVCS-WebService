using EvCharging.Infrastructure.Config;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Conventions;

namespace EvCharging.Infrastructure.Persistence;

public class MongoDbContext
{
    public IMongoDatabase Database { get; }

    static MongoDbContext()
    {
        // Use camelCase naming convention
        var conventionPack = new ConventionPack { new CamelCaseElementNameConvention() };
        ConventionRegistry.Register("camelCase", conventionPack, t => true);
    }

    public MongoDbContext(IOptions<MongoSettings> options)
    {
        var client = new MongoClient(options.Value.ConnectionString);
        Database = client.GetDatabase(options.Value.Database);
    }

    public IMongoCollection<T> GetCollection<T>(string? name = null)
        => Database.GetCollection<T>(name ?? typeof(T).Name);
}