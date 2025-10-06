using EvCharging.Infrastructure.Config;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using EvCharging.Domain.Common;
using EvCharging.Domain.Entities;

namespace EvCharging.Infrastructure.Persistence;

public class MongoDbContext
{
    public IMongoDatabase Database { get; }

    static MongoDbContext()
    {
        // Configure MongoDB serialization only once
        if (!BsonClassMap.IsClassMapRegistered(typeof(EntityBase)))
        {
            // Use camelCase naming convention
            var conventionPack = new ConventionPack { new CamelCaseElementNameConvention() };
            ConventionRegistry.Register("camelCase", conventionPack, t => true);

            // Configure EntityBase mapping
            BsonClassMap.RegisterClassMap<EntityBase>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(c => c.Id);
                cm.MapMember(c => c.CreatedAt).SetElementName("createdAt");
                cm.MapMember(c => c.UpdatedAt).SetElementName("updatedAt");
            });

            // Configure Station mapping with explicit field names to handle variations
            BsonClassMap.RegisterClassMap<Station>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true); // Ignore unknown fields
                // Explicitly map fields to handle both camelCase and PascalCase
                cm.MapMember(c => c.Name).SetElementName("name");
                cm.MapMember(c => c.Address).SetElementName("address"); 
                cm.MapMember(c => c.Latitude).SetElementName("latitude");
                cm.MapMember(c => c.Longitude).SetElementName("longitude");
                cm.MapMember(c => c.Type).SetElementName("type");
                cm.MapMember(c => c.Slots).SetElementName("slots");
                cm.MapMember(c => c.IsActive).SetElementName("isActive");
            });

            // Configure other entities to ignore extra elements
            BsonClassMap.RegisterClassMap<User>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });

            BsonClassMap.RegisterClassMap<Booking>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });

            BsonClassMap.RegisterClassMap<StationSchedule>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });

            BsonClassMap.RegisterClassMap<EvOwner>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
        }
    }

    public MongoDbContext(IOptions<MongoSettings> options)
    {
        var client = new MongoClient(options.Value.ConnectionString);
        Database = client.GetDatabase(options.Value.Database);
    }

    public IMongoCollection<T> GetCollection<T>(string? name = null)
        => Database.GetCollection<T>(name ?? typeof(T).Name);
}