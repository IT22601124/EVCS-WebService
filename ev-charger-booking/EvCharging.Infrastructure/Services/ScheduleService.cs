using EvCharging.Application.Contracts;
using EvCharging.Application.DTOs;
using EvCharging.Domain.Entities;


namespace EvCharging.Infrastructure.Services;


public class ScheduleService : IScheduleService
{
    private readonly IRepository<StationSchedule> _schedules;


    public ScheduleService(IRepository<StationSchedule> schedules) { _schedules = schedules; }


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


    private async Task<List<ScheduleResponse>> GetByFilterAsync(System.Linq.Expressions.Expression<Func<StationSchedule,bool>> filter)
    {
        var items = await _schedules.FindAsync(filter);
        return items.Select(s => new ScheduleResponse(s.Id, s.StationId, s.Date, s.Slots)).ToList();
    }
}