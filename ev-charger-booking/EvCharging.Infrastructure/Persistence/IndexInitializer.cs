using MongoDB.Driver;
using EvCharging.Domain.Entities;

namespace EvCharging.Infrastructure.Persistence;

public static class IndexInitializer
{
    public static async Task EnsureIndexesAsync(MongoDbContext ctx)
    {
        // owners: NIC unique
        var owners = ctx.GetCollection<EvOwner>();
        var nicIndex = new CreateIndexModel<EvOwner>(
            Builders<EvOwner>.IndexKeys.Ascending(o => o.Nic),
            new CreateIndexOptions { Unique = true, Name = "UX_EvOwner_Nic" });
        await owners.Indexes.CreateOneAsync(nicIndex);

        // stations: name non-unique index
        var stations = ctx.GetCollection<Station>();
        var nameIndex = new CreateIndexModel<Station>(
            Builders<Station>.IndexKeys.Ascending(s => s.Name),
            new CreateIndexOptions { Name = "IX_Station_Name" });
        await stations.Indexes.CreateOneAsync(nameIndex);

        // schedules: stationId + date composite
        var schedules = ctx.GetCollection<StationSchedule>();
        var schedIndex = new CreateIndexModel<StationSchedule>(
            Builders<StationSchedule>.IndexKeys
                .Ascending(s => s.StationId)
                .Ascending(s => s.Date),
            new CreateIndexOptions { Name = "IX_Schedule_Station_Date" });
        await schedules.Indexes.CreateOneAsync(schedIndex);
    }
}
