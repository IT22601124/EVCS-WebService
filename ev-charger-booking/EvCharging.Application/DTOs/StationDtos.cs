using EvCharging.Domain.Entities;

namespace EvCharging.Application.DTOs;


public record CreateStationRequest(string Name, string Address, double Latitude, double Longitude, string Type, int Slots);
public record UpdateStationRequest(string Name, string Address, double Latitude, double Longitude, string Type, int Slots, bool IsActive);
public record StationResponse(string Id, string Name, string Address, double Latitude, double Longitude, string Type, int Slots, bool IsActive);
public record StationWithSchedulesResponse(string Id, string Name, string Address, double Latitude, double Longitude, string Type, int Slots, bool IsActive, List<ScheduleResponse> Schedules);