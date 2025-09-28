using EvCharging.Domain.Common;


namespace EvCharging.Domain.Entities;


public class Booking : EntityBase
{
    public string Nic { get; set; } = default!; // EV owner NIC
    public string StationId { get; set; } = default!;
    public DateOnly Date { get; set; }
    public TimeOnly Start { get; set; }
    public TimeOnly End { get; set; }
    public string Status { get; set; } = "Pending"; // see BookingStatus
    public string? QrToken { get; set; } // set when Approved
}