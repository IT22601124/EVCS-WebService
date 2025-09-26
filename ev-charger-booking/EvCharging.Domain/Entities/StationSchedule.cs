using EvCharging.Domain.Common;


namespace EvCharging.Domain.Entities;


public class StationSchedule : EntityBase
{
    public string StationId { get; set; } = default!;
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public List<TimeSlot> Slots { get; set; } = new();
}


public class TimeSlot
{
    public TimeOnly Start { get; set; }
    public TimeOnly End { get; set; }
    public bool Available { get; set; } = true;
    public int Capacity { get; set; } = 1; // how many vehicles per slot
}