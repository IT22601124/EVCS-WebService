using EvCharging.Domain.Entities;


namespace EvCharging.Application.DTOs;


public record UpsertScheduleRequest(string StationId, DateOnly Date, List<TimeSlot> Slots);
public record ScheduleResponse(string Id, string StationId, DateOnly Date, List<TimeSlot> Slots);