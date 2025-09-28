using EvCharging.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EvCharging.Infrastructure.Persistence;

public static class IndexInitializer
{
    public static async Task EnsureIndexesAsync(MongoDbContext ctx)
    {
        // Owners: NIC unique
        var owners = ctx.GetCollection<EvOwner>();
        await owners.Indexes.CreateOneAsync(
            new CreateIndexModel<EvOwner>(
                Builders<EvOwner>.IndexKeys.Ascending(o => o.Nic),
                new CreateIndexOptions<EvOwner> { Unique = true, Name = "UX_EvOwner_Nic" }));

        // Stations: name index
        var stations = ctx.GetCollection<Station>();
        await stations.Indexes.CreateOneAsync(
            new CreateIndexModel<Station>(
                Builders<Station>.IndexKeys.Ascending(s => s.Name),
                new CreateIndexOptions<Station> { Name = "IX_Station_Name" }));

        // Schedules: Station + Date
        var schedules = ctx.GetCollection<StationSchedule>();
        await schedules.Indexes.CreateOneAsync(
            new CreateIndexModel<StationSchedule>(
                Builders<StationSchedule>.IndexKeys
                    .Ascending(s => s.StationId)
                    .Ascending(s => s.Date),
                new CreateIndexOptions<StationSchedule> { Name = "IX_Schedule_Station_Date" }));

        // Bookings: Station + Date + Start + Status (capacity and queries)
        var bookings = ctx.GetCollection<Booking>();
        await bookings.Indexes.CreateOneAsync(
            new CreateIndexModel<Booking>(
                Builders<Booking>.IndexKeys
                    .Ascending(b => b.StationId)
                    .Ascending(b => b.Date)
                    .Ascending(b => b.Start)
                    .Ascending(b => b.Status),
                new CreateIndexOptions<Booking> { Name = "IX_Booking_Station_Date_Start_Status" }));

        // Bookings: unique QR token when it's a string (excludes null/missing)
        var qrFilter = Builders<Booking>.Filter.Type(b => b.QrToken, BsonType.String);
        await bookings.Indexes.CreateOneAsync(
            new CreateIndexModel<Booking>(
                Builders<Booking>.IndexKeys.Ascending(b => b.QrToken),
                new CreateIndexOptions<Booking>
                {
                    Name = "UX_Booking_QrToken_StringOnly",
                    Unique = true,
                    PartialFilterExpression = qrFilter
                }));

        // Users: username unique
        var users = ctx.GetCollection<User>();
        await users.Indexes.CreateOneAsync(
            new CreateIndexModel<User>(
                Builders<User>.IndexKeys.Ascending(u => u.Username),
                new CreateIndexOptions<User> { Name = "UX_User_Username", Unique = true }));
    }
}
