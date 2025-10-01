using EvCharging.Application.Contracts;
using EvCharging.Application.DTOs;
using EvCharging.Domain.Entities;
using EvCharging.Domain.Enums;

namespace EvCharging.Infrastructure.Services;

public class BookingService : IBookingService
{
    private readonly IRepository<Booking> _bookings;
    private readonly IRepository<StationSchedule> _schedules;
    private readonly IRepository<EvOwner> _owners;
    private readonly IRepository<Station> _stations;

    public BookingService(
        IRepository<Booking> bookings, 
        IRepository<StationSchedule> schedules,
        IRepository<EvOwner> owners,
        IRepository<Station> stations)
    { 
        _bookings = bookings; 
        _schedules = schedules;
        _owners = owners;
        _stations = stations;
    }

    public async Task<BookingResponse> CreateAsync(CreateBookingRequest req)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (req.Date < today || req.Date > today.AddDays(7))
            throw new InvalidOperationException("Booking date must be within the next 7 days");

        if (req.End <= req.Start)
            throw new InvalidOperationException("End time must be after Start time");

        // Verify owner exists and is active
        var owner = (await _owners.FindAsync(o => o.Nic == req.Nic && o.IsActive)).FirstOrDefault()
            ?? throw new InvalidOperationException("Owner not found or inactive");

        // Verify station exists and is active
        var station = await _stations.GetByIdAsync(req.StationId);
        if (station == null || !station.IsActive)
            throw new InvalidOperationException("Station not found or inactive");

        var schedule = (await _schedules.FindAsync(s => s.StationId == req.StationId && s.Date == req.Date)).FirstOrDefault()
            ?? throw new InvalidOperationException("No schedule published for the selected date");

        var slot = schedule.Slots.FirstOrDefault(x => x.Start == req.Start && x.End == req.End && x.Available)
            ?? throw new InvalidOperationException("Selected time slot is not available");

        var existing = await _bookings.FindAsync(b => b.StationId == req.StationId && b.Date == req.Date &&
                                                      ((b.Start < req.End) && (req.Start < b.End)) &&
                                                      (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Approved));
        if (existing.Count >= slot.Capacity)
            throw new InvalidOperationException("Time slot capacity reached");

        var entity = new Booking
        {
            Id = Guid.NewGuid().ToString("N"),
            Nic = req.Nic,
            StationId = req.StationId,
            Date = req.Date,
            Start = req.Start,
            End = req.End,
            Status = BookingStatus.Pending
        };
        await _bookings.InsertAsync(entity);
        return MapEnhanced(entity, owner, station);
    }

    public async Task<BookingResponse?> GetByIdAsync(string id)
    {
        var e = await _bookings.GetByIdAsync(id);
        if (e is null) return null;
        
        var owner = (await _owners.FindAsync(o => o.Nic == e.Nic)).FirstOrDefault();
        var station = await _stations.GetByIdAsync(e.StationId);
        
        return MapEnhanced(e, owner, station);
    }

    public async Task<List<BookingResponse>> GetByOwnerAsync(string nic)
    {
        var bookings = await _bookings.FindAsync(b => b.Nic == nic);
        var owner = (await _owners.FindAsync(o => o.Nic == nic)).FirstOrDefault();
        
        var results = new List<BookingResponse>();
        foreach (var booking in bookings)
        {
            var station = await _stations.GetByIdAsync(booking.StationId);
            results.Add(MapEnhanced(booking, owner, station));
        }
        return results;
    }

    public async Task<List<BookingResponse>> GetByStationAndDateAsync(string stationId, DateOnly date)
    {
        var bookings = await _bookings.FindAsync(b => b.StationId == stationId && b.Date == date);
        var station = await _stations.GetByIdAsync(stationId);
        
        var results = new List<BookingResponse>();
        foreach (var booking in bookings)
        {
            var owner = (await _owners.FindAsync(o => o.Nic == booking.Nic)).FirstOrDefault();
            results.Add(MapEnhanced(booking, owner, station));
        }
        return results;
    }

    public async Task UpdateAsync(string id, UpdateBookingRequest req)
    {
        var e = await _bookings.GetByIdAsync(id) ?? throw new KeyNotFoundException("Booking not found");
        EnsureChangeAllowed(e);

        if (req.End <= req.Start) throw new InvalidOperationException("End time must be after Start time");

        var schedule = (await _schedules.FindAsync(s => s.StationId == e.StationId && s.Date == req.Date)).FirstOrDefault()
            ?? throw new InvalidOperationException("No schedule for selected date");

        var slot = schedule.Slots.FirstOrDefault(x => x.Start == req.Start && x.End == req.End && x.Available)
            ?? throw new InvalidOperationException("Selected time slot is not available");

        var existing = await _bookings.FindAsync(b => b.StationId == e.StationId && b.Date == req.Date && b.Id != id &&
                                                      ((b.Start < req.End) && (req.Start < b.End)) &&
                                                      (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Approved));
        if (existing.Count >= slot.Capacity)
            throw new InvalidOperationException("Time slot capacity reached");

        e.Date = req.Date; e.Start = req.Start; e.End = req.End; e.UpdatedAt = DateTime.UtcNow;
        await _bookings.UpdateAsync(e.Id, e);
    }

    public async Task CancelAsync(string id)
    {
        var e = await _bookings.GetByIdAsync(id) ?? throw new KeyNotFoundException("Booking not found");
        EnsureChangeAllowed(e);
        e.Status = BookingStatus.Cancelled; e.UpdatedAt = DateTime.UtcNow;
        await _bookings.UpdateAsync(e.Id, e);
    }

    public async Task<BookingResponse> ApproveAsync(string id)
    {
        var e = await _bookings.GetByIdAsync(id) ?? throw new KeyNotFoundException("Booking not found");
        if (e.Status != BookingStatus.Pending) throw new InvalidOperationException("Only pending bookings can be approved");
        EnsureChangeAllowed(e);
        e.Status = BookingStatus.Approved;
        e.QrToken = Guid.NewGuid().ToString("N");
        e.UpdatedAt = DateTime.UtcNow;
        await _bookings.UpdateAsync(e.Id, e);
        
        var owner = (await _owners.FindAsync(o => o.Nic == e.Nic)).FirstOrDefault();
        var station = await _stations.GetByIdAsync(e.StationId);
        
        return MapEnhanced(e, owner, station);
    }

    public async Task<ScanResponse> ScanAsync(string qrToken)
    {
        var e = (await _bookings.FindAsync(b => b.QrToken == qrToken)).FirstOrDefault()
            ?? throw new KeyNotFoundException("Invalid QR token");
        if (e.Status != BookingStatus.Approved)
            throw new InvalidOperationException("Booking is not in Approved state");
        
        var owner = (await _owners.FindAsync(o => o.Nic == e.Nic)).FirstOrDefault();
        var station = await _stations.GetByIdAsync(e.StationId);
        
        return new ScanResponse(
            e.Id,
            e.Nic,
            owner?.FullName ?? "Unknown Owner",
            e.StationId,
            station?.Name ?? "Unknown Station",
            station?.Address ?? "N/A",
            e.Date,
            e.Start,
            e.End,
            e.Status
        );
    }

    public async Task FinalizeAsync(string id)
    {
        var e = await _bookings.GetByIdAsync(id) ?? throw new KeyNotFoundException("Booking not found");
        if (e.Status != BookingStatus.Approved)
            throw new InvalidOperationException("Only approved bookings can be finalized");
        e.Status = BookingStatus.Completed; e.UpdatedAt = DateTime.UtcNow;
        await _bookings.UpdateAsync(e.Id, e);
    }

    private static BookingResponse MapEnhanced(Booking e, EvOwner? owner, Station? station)
    {
        return new BookingResponse(
            e.Id,
            e.Nic,
            owner?.FullName ?? "Unknown Owner",
            owner?.Email ?? "N/A",
            owner?.Phone ?? "N/A",
            e.StationId,
            station?.Name ?? "Unknown Station",
            station?.Address ?? "N/A",
            station?.Type ?? "AC",
            e.Date,
            e.Start,
            e.End,
            e.Status,
            e.QrToken,
            e.CreatedAt,
            e.UpdatedAt
        );
    }

    private static void EnsureChangeAllowed(Booking e)
    {
        if (e.Status == BookingStatus.Completed || e.Status == BookingStatus.Cancelled)
            throw new InvalidOperationException("Completed/Cancelled bookings cannot be modified");

        var nowUtc = DateTime.UtcNow;
        var startUtc = new DateTime(e.Date.Year, e.Date.Month, e.Date.Day, e.Start.Hour, e.Start.Minute, e.Start.Second, DateTimeKind.Utc);
        if (startUtc - nowUtc < TimeSpan.FromHours(12))
            throw new InvalidOperationException("Changes are allowed only up to 12 hours before the session start");
    }
}
