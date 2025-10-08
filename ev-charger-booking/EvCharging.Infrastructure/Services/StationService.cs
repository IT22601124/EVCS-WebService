using EvCharging.Application.Contracts;
using EvCharging.Application.DTOs;
using EvCharging.Domain.Entities;
using EvCharging.Domain.Enums;

namespace EvCharging.Infrastructure.Services;

public class StationService : IStationService
{
    private readonly IRepository<Station> _stations;
    private readonly IRepository<Booking> _bookings;
    private readonly IRepository<StationSchedule> _schedules;

    public StationService(IRepository<Station> stations, IRepository<Booking> bookings, IRepository<StationSchedule> schedules)
    {
        _stations = stations;
        _bookings = bookings;
        _schedules = schedules;
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
            IsActive = true,
            AssignedOperators = new List<string>() // new stations start with no operators

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

        // Business rule: prevent deactivation if there are active bookings
        if (entity.IsActive && !req.IsActive)
        {
            var active = await _bookings.FindAsync(b =>
                b.StationId == id &&
                (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Approved));

            if (active.Any())
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

        // Key logic: filter by operator username
        public async Task<List<StationResponse>> GetByAssignedOperatorAsync(string operatorUsername)
        {
            var list = await _stations.FindAsync(s =>
                s.AssignedOperators.Contains(operatorUsername)
            );

            return list.Select(Map).ToList();
        }
    
    }

    public async Task DeleteAsync(string id)
    {
        var entity = await _stations.GetByIdAsync(id) ?? throw new KeyNotFoundException("Station not found");
        
        // Business rule: prevent deletion if there are active bookings
        var activeBookings = await _bookings.FindAsync(b =>
            b.StationId == id &&
            (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Approved));

        if (activeBookings.Any())
            throw new InvalidOperationException("Cannot delete station with active bookings");

        await _stations.DeleteAsync(id);
    }

    public async Task<List<StationWithSchedulesResponse>> GetAllWithSchedulesAsync(DateOnly? date = null)
    {
        var stations = await _stations.GetAllAsync();
        var result = new List<StationWithSchedulesResponse>();

        foreach (var station in stations)
        {
            var schedules = new List<ScheduleResponse>();
            
            if (date.HasValue)
            {
                // Get schedules for specific date
                var stationSchedules = await _schedules.FindAsync(s => s.StationId == station.Id && s.Date == date.Value);
                schedules = stationSchedules.Select(MapSchedule).ToList();
            }
            else
            {
                // Get all schedules for the station (next 7 days)
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var futureDate = today.AddDays(7);
                var stationSchedules = await _schedules.FindAsync(s => s.StationId == station.Id && s.Date >= today && s.Date <= futureDate);
                schedules = stationSchedules.Select(MapSchedule).ToList();
            }

            var stationWithSchedules = new StationWithSchedulesResponse(
                station.Id, 
                station.Name, 
                station.Address, 
                station.Latitude, 
                station.Longitude, 
                station.Type, 
                station.Slots, 
                station.IsActive, 
                schedules
            );

            result.Add(stationWithSchedules);
        }

        return result;
    }

    public async Task<List<StationWithSchedulesResponse>> GetAllWithWeeklySchedulesAsync(DateOnly? startDate = null)
    {
        // Use provided start date or default to today (first day of the week)
        var weekStart = startDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var weekEnd = weekStart.AddDays(6); // 7 days total (0-6)

        var stations = await _stations.GetAllAsync();
        
        // Debug: Log what we got from database
        Console.WriteLine($"=== DEBUG: Found {stations.Count} stations from database ===");
        foreach (var station in stations)
        {
            Console.WriteLine($"Station ID: {station.Id}");
            Console.WriteLine($"Name: '{station.Name}' (null: {station.Name == null})");
            Console.WriteLine($"Address: '{station.Address}' (null: {station.Address == null})");
            Console.WriteLine($"Type: '{station.Type}'");
            Console.WriteLine($"Slots: {station.Slots}");
            Console.WriteLine($"IsActive: {station.IsActive}");
            Console.WriteLine("---");
        }
        
        var result = new List<StationWithSchedulesResponse>();

        foreach (var station in stations)
        {
            // Get all schedules for this station within the week range
            var stationSchedules = await _schedules.FindAsync(s => 
                s.StationId == station.Id && 
                s.Date >= weekStart && 
                s.Date <= weekEnd);
            
            var schedules = stationSchedules
                .OrderBy(s => s.Date)
                .Select(MapSchedule)
                .ToList();

            var stationWithSchedules = new StationWithSchedulesResponse(
                station.Id, 
                station.Name, 
                station.Address, 
                station.Latitude, 
                station.Longitude, 
                station.Type, 
                station.Slots, 
                station.IsActive, 
                schedules
            );

            result.Add(stationWithSchedules);
        }

        return result;
    }

    private static StationResponse Map(Station e)
        => new(e.Id, e.Name, e.Address, e.Latitude, e.Longitude, e.Type, e.Slots, e.IsActive, e.AssignedOperators); // include AssignedOperators in response

    private static ScheduleResponse MapSchedule(StationSchedule e)
        => new(e.Id, e.StationId, e.Date, e.Slots);
}
