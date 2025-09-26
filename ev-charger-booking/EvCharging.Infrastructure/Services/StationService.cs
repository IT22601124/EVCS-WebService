using EvCharging.Application.Contracts;
using EvCharging.Application.DTOs;
using EvCharging.Domain.Entities;


namespace EvCharging.Infrastructure.Services;


public class StationService : IStationService
{
    private readonly IRepository<Station> _stations;


    public StationService(IRepository<Station> stations) { _stations = stations; }


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
        => (await _stations.GetAllAsync()).Select(Map).ToList();


    public async Task<StationResponse?> GetByIdAsync(string id)
    {
        var entity = await _stations.GetByIdAsync(id);
        return entity is null ? null : Map(entity);
    }


    public async Task UpdateAsync(string id, UpdateStationRequest req)
    {
        var entity = await _stations.GetByIdAsync(id) ?? throw new KeyNotFoundException("Station not found");


        // TODO: when bookings are implemented, block deactivation if active bookings exist
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