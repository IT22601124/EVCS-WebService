using EvCharging.Application.Contracts;
using EvCharging.Application.DTOs;
using EvCharging.Domain.Entities;
using EvCharging.Domain.Enums;

namespace EvCharging.Infrastructure.Services;

public class StationService : IStationService
{
    private readonly IRepository<Station> _stations;
    private readonly IRepository<Booking> _bookings;

    public StationService(IRepository<Station> stations, IRepository<Booking> bookings)
    {
        _stations = stations;
        _bookings = bookings;
    }

    public async Task<StationResponse> CreateAsync(CreateStationRequest req)
    {
        var entity = new Station
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = req.Name,
            Address = req.Address,
            Latitude = req.Latitude,
            Longitude = req.Longitude,
            Type = req.Type,
            Slots = req.Slots,
            IsActive = true
        };

        await _stations.InsertAsync(entity);
        return Map(entity);
    }

    public async Task<List<StationResponse>> GetAllAsync()
    {
        var all = await _stations.GetAllAsync();
        return all.Select(Map).ToList();
    }

    public async Task<StationResponse?> GetByIdAsync(string id)
    {
        var entity = await _stations.GetByIdAsync(id);
        return entity is null ? null : Map(entity);
    }

    public async Task UpdateAsync(string id, UpdateStationRequest req)
    {
        var entity = await _stations.GetByIdAsync(id) ?? throw new KeyNotFoundException("Station not found");

        // Business rule (Day 3–4): prevent deactivation if there are active bookings
        // Active bookings are those in Pending or Approved state, regardless of date.
        if (entity.IsActive && !req.IsActive)
        {
            var activeBookings = await _bookings.FindAsync(b =>
                b.StationId == id &&
                (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Approved));

            if (activeBookings.Any())
                throw new InvalidOperationException("Cannot deactivate station with active bookings");
        }

        entity.Name = req.Name;
        entity.Address = req.Address;
        entity.Latitude = req.Latitude;
        entity.Longitude = req.Longitude;
        entity.Type = req.Type;
        entity.Slots = req.Slots;
        entity.IsActive = req.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _stations.UpdateAsync(entity.Id, entity);
    }

    private static StationResponse Map(Station e)
        => new(e.Id, e.Name, e.Address, e.Latitude, e.Longitude, e.Type, e.Slots, e.IsActive);
}
