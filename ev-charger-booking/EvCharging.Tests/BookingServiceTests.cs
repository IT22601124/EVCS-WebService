using EvCharging.Application.DTOs;
using EvCharging.Domain.Entities;
using EvCharging.Domain.Enums;
using EvCharging.Infrastructure.Services;
using EvCharging.Tests.TestDoubles;
using FluentAssertions;

namespace EvCharging.Tests;

public class BookingServiceTests
{
    [Fact]
    public async Task Create_ShouldReject_WhenDateBeyond7Days()
    {
        var bookings = new InMemoryRepository<Booking>();
        var schedules = new InMemoryRepository<StationSchedule>();
        var owners = new InMemoryRepository<EvOwner>();
        var stations = new InMemoryRepository<Station>();
        var svc = new BookingService(bookings, schedules, owners, stations);

        var req = new CreateBookingRequest("923456789V", "S1",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(8)),
            new TimeOnly(9,0), new TimeOnly(10,0));

        var act = async () => await svc.CreateAsync(req);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*within the next 7 days*");
    }

    [Fact]
    public async Task Create_ShouldReject_WhenCapacityReached()
    {
        var bookings = new InMemoryRepository<Booking>();
        var schedules = new InMemoryRepository<StationSchedule>();
        var owners = new InMemoryRepository<EvOwner>();
        var stations = new InMemoryRepository<Station>();
        var svc = new BookingService(bookings, schedules, owners, stations);

        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        // schedule with 1 capacity slot
        await schedules.InsertAsync(new StationSchedule
        {
            Id = Guid.NewGuid().ToString("N"),
            StationId = "S1",
            Date = date,
            Slots = new() { new TimeSlot { Start = new TimeOnly(9,0), End = new TimeOnly(10,0), Available = true, Capacity = 1 } }
        });

        // existing approved booking consuming capacity
        await bookings.InsertAsync(new Booking
        {
            Id = Guid.NewGuid().ToString("N"),
            Nic = "X",
            StationId = "S1",
            Date = date,
            Start = new TimeOnly(9,0),
            End = new TimeOnly(10,0),
            Status = BookingStatus.Approved
        });

        var req = new CreateBookingRequest("Y", "S1", date, new TimeOnly(9,0), new TimeOnly(10,0));
        var act = async () => await svc.CreateAsync(req);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*capacity*");
    }

    [Fact]
    public async Task Update_ShouldReject_WhenInside12Hours()
    {
        var bookings = new InMemoryRepository<Booking>();
        var schedules = new InMemoryRepository<StationSchedule>();
        var owners = new InMemoryRepository<EvOwner>();
        var stations = new InMemoryRepository<Station>();
        var svc = new BookingService(bookings, schedules, owners, stations);

        var date = DateOnly.FromDateTime(DateTime.UtcNow); // today
        // schedule for both the existing slot and the update slot (same)
        await schedules.InsertAsync(new StationSchedule
        {
            Id = Guid.NewGuid().ToString("N"),
            StationId = "S1",
            Date = date,
            Slots = new() { new TimeSlot { Start = new TimeOnly(DateTime.UtcNow.Hour+1, 0), End = new TimeOnly(DateTime.UtcNow.Hour+2, 0), Available = true, Capacity = 2 } }
        });

        var booking = new Booking
        {
            Id = "B1", Nic = "NIC", StationId = "S1", Date = date,
            Start = new TimeOnly(DateTime.UtcNow.Hour+1, 0),
            End = new TimeOnly(DateTime.UtcNow.Hour+2, 0),
            Status = BookingStatus.Pending
        };
        await bookings.InsertAsync(booking);

        var req = new UpdateBookingRequest(date, booking.Start, booking.End);
        var act = async () => await svc.UpdateAsync("B1", req);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*12 hours*");
    }
}
