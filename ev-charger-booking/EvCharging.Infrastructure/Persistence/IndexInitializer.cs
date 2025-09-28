using EvCharging.Domain.Entities;
using MongoDB.Driver;

namespace EvCharging.Infrastructure.Persistence;

public static class IndexInitializer
{
    public static async Task EnsureIndexesAsync(MongoDbContext ctx)
    {
        // --- Owners: NIC unique (we also use NIC as Id, but index helps queries) ---
        var owners = ctx.GetCollection<EvOwner>();
        var nicIndex = new CreateIndexModel<EvOwner>(
            Builders<EvOwner>.IndexKeys.Ascending(o => o.Nic),
            new CreateIndexOptions { Unique = true, Name = "UX_EvOwner_Nic" });
        await owners.Indexes.CreateOneAsync(nicIndex);

        // --- Stations: name index for quick search ---
        var stations = ctx.GetCollection<Station>();
        var stationNameIndex = new CreateIndexModel<Station>(
            Builders<Station>.IndexKeys.Ascending(s => s.Name),
            new CreateIndexOptions { Name = "IX_Station_Name" });
        await stations.Indexes.CreateOneAsync(stationNameIndex);

        // --- Schedules: composite on Station + Date ---
        var schedules = ctx.GetCollection<StationSchedule>();
        var scheduleComposite = new CreateIndexModel<StationSchedule>(
            Builders<StationSchedule>.IndexKeys
                .Ascending(s => s.StationId)
                .Ascending(s => s.Date),
            new CreateIndexOptions { Name = "IX_Schedule_Station_Date" });
        await schedules.Indexes.CreateOneAsync(scheduleComposite);

        // --- Bookings: composite on Station + Date + Start + Status (for capacity & queries) ---
        var bookings = ctx.GetCollection<Booking>();
        var bookingComposite = new CreateIndexModel<Booking>(
            Builders<Booking>.IndexKeys
                .Ascending(b => b.StationId)
                .Ascending(b => b.Date)
                .Ascending(b => b.Start)
                .Ascending(b => b.Status),
            new CreateIndexOptions { Name = "IX_Booking_Station_Date_Start_Status" });
        await bookings.Indexes.CreateOneAsync(bookingComposite);

        // (Optional) QR token lookup index (not unique to allow nulls). If you want uniqueness when set:
        // use PartialFilterExpression to ignore nulls.
        var qrPartial = new CreateIndexModel<Booking>(
            Builders<Booking>.IndexKeys.Ascending(b => b.QrToken),
            new CreateIndexOptions
            {
                Name = "IX_Booking_QrToken",
                PartialFilterExpression = Builders<Booking>.Filter.Ne(b => b.QrToken, null)
            });
        await bookings.Indexes.CreateOneAsync(qrPartial);
    }
}
