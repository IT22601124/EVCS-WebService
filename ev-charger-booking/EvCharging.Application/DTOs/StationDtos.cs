namespace EvCharging.Application.DTOs;


public record CreateStationRequest(string Name, string Address, double Latitude, double Longitude, string Type, int Slots, string? AssignedOperator = null); // allow assigning operator when creating
public record UpdateStationRequest(string Name, string Address, double Latitude, double Longitude, string Type, int Slots, bool IsActive, string? AssignedOperator = null); // allow assigning operator when updating
public record StationResponse(string Id, string Name, string Address, double Latitude, double Longitude, string Type, int Slots, bool IsActive, List<string> AssignedOperators); // include assigned operators in response