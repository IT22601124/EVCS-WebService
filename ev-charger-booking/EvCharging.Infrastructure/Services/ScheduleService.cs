using EvCharging.Application.Contracts;
using EvCharging.Application.DTOs;
using EvCharging.Domain.Entities;


namespace EvCharging.Infrastructure.Services;


public class ScheduleService : IScheduleService
{
    private readonly IRepository<StationSchedule> _schedules;
    private readonly IRepository<Station> _stations;


    public ScheduleService(IRepository<StationSchedule> schedules, IRepository<Station> stations) 
    { 
        _schedules = schedules;
        _stations = stations;
    }


    public async Task<ScheduleResponse> UpsertAsync(UpsertScheduleRequest req)
    {
        var existing = (await _schedules.FindAsync(s => s.StationId == req.StationId && s.Date == req.Date)).FirstOrDefault();
        if (existing is null)
        {
            var entity = new StationSchedule
            {
                Id = Guid.NewGuid().ToString("N"),
                StationId = req.StationId,
                Date = req.Date,
                Slots = req.Slots
            };
            await _schedules.InsertAsync(entity);
            return new ScheduleResponse(entity.Id, entity.StationId, entity.Date, entity.Slots);
        }
        else
        {
            existing.Slots = req.Slots;
            existing.UpdatedAt = DateTime.UtcNow;
            await _schedules.UpdateAsync(existing.Id, existing);
            return new ScheduleResponse(existing.Id, existing.StationId, existing.Date, existing.Slots);
        }
    }


    public Task<List<ScheduleResponse>> GetByStationAndDateAsync(string stationId, DateOnly date)
        => GetByFilterAsync(s => s.StationId == stationId && s.Date == date);

    public Task<List<ScheduleResponse>> GetAllAsync()
        => GetByFilterAsync(_ => true);

    public async Task<List<ScheduleWithStationResponse>> GetAllWithStationsAsync()
    {
        var schedules = await _schedules.FindAsync(_ => true);
        
        // Filter out schedules with null or empty StationId
        var validSchedules = schedules.Where(s => !string.IsNullOrEmpty(s.StationId)).ToList();
        
        if (!validSchedules.Any())
        {
            return new List<ScheduleWithStationResponse>();
        }
        
        var stationIds = validSchedules.Select(s => s.StationId).Distinct().ToList();
        var stations = await _stations.FindAsync(st => stationIds.Contains(st.Id));
        var stationDict = stations.ToDictionary(st => st.Id, st => new StationResponse(st.Id, st.Name, st.Address, st.Latitude, st.Longitude, st.Type, st.Slots, st.IsActive, st.AssignedOperators));

        return validSchedules
            .Where(s => !string.IsNullOrEmpty(s.StationId)) // Double-check for safety
            .Select(s => new ScheduleWithStationResponse(
                s.Id, 
                s.StationId, 
                s.Date, 
                s.Slots, 
                stationDict.TryGetValue(s.StationId, out var station) ? station : null
            ))
            .Where(sr => sr.Station != null) // Only return schedules with valid stations
            .ToList();
    }

    public async Task<int> CleanupInvalidSchedulesAsync()
    {
        var invalidSchedules = await _schedules.FindAsync(s => string.IsNullOrEmpty(s.StationId));
        int deletedCount = 0;
        
        foreach (var schedule in invalidSchedules)
        {
            await _schedules.DeleteAsync(schedule.Id);
            deletedCount++;
        }
        
        return deletedCount;
    }


    private async Task<List<ScheduleResponse>> GetByFilterAsync(System.Linq.Expressions.Expression<Func<StationSchedule,bool>> filter)
    {
        var items = await _schedules.FindAsync(filter);
        return items.Select(s => new ScheduleResponse(s.Id, s.StationId, s.Date, s.Slots)).ToList();
    }
}