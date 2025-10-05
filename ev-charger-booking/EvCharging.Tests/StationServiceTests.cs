using EvCharging.Application.DTOs;
using EvCharging.Domain.Entities;
using EvCharging.Domain.Enums;
using EvCharging.Infrastructure.Services;
using EvCharging.Tests.TestDoubles;
using FluentAssertions;

namespace EvCharging.Tests;

public class StationServiceTests
{
    [Fact]
    public async Task Update_ShouldBlockDeactivation_WhenActiveBookingsExist()
    {
        var stationRepo = new InMemoryRepository<Station>();
        var bookingRepo = new InMemoryRepository<Booking>();
        var svc = new StationService(stationRepo, bookingRepo);

        var st = new Station
        {
            Id = "S1", Name = "A", Address = "x", Latitude = 0, Longitude = 0, Type = "AC", Slots = 2, IsActive = true
        };
        await stationRepo.InsertAsync(st);

        await bookingRepo.InsertAsync(new Booking
        {
            Id = "B1", Nic = "N", StationId = "S1", Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            Start = new TimeOnly(9,0), End = new TimeOnly(10,0), Status = BookingStatus.Pending
        });

        var req = new UpdateStationRequest(st.Name, st.Address, st.Latitude, st.Longitude, st.Type, st.Slots, false);

        var act = async () => await svc.UpdateAsync("S1", req);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot deactivate station with active bookings");
    }

    [Fact]
    public async Task Update_ShouldAllowNormalEdits_WhenNoActiveBookings()
    {
        var stationRepo = new InMemoryRepository<Station>();
        var bookingRepo = new InMemoryRepository<Booking>();
        var svc = new StationService(stationRepo, bookingRepo);

        var st = new Station
        {
            Id = "S1", Name = "A", Address = "x", Latitude = 0, Longitude = 0, Type = "AC", Slots = 2, IsActive = true
        };
        await stationRepo.InsertAsync(st);

        var req = new UpdateStationRequest("B", "y", 1, 1, "DC", 3, true);
        await svc.UpdateAsync("S1", req);

        var updated = await stationRepo.GetByIdAsync("S1");
        updated!.Name.Should().Be("B");
        updated.Type.Should().Be("DC");
        updated.Slots.Should().Be(3);
        updated.IsActive.Should().BeTrue();
    }
}
