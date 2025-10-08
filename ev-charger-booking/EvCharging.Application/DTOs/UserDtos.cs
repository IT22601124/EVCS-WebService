namespace EvCharging.Application.DTOs;

public record CreateUserRequest(string Username, string Password, string Role, bool IsActive = true, string? AssignedStationId = null);
public record UpdateUserRequest(string? Password, string Role, bool IsActive, string? AssignedStationId = null);
public record UserResponse(string Id, string Username, string Role, bool IsActive, string? AssignedStationId);

public record UserAssignStationRequest(string? StationId);
