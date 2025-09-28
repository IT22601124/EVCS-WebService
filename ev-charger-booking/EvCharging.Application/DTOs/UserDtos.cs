namespace EvCharging.Application.DTOs;

public record CreateUserRequest(string Username, string Password, string Role, bool IsActive = true);
public record UpdateUserRequest(string? Password, string Role, bool IsActive);
public record UserResponse(string Id, string Username, string Role, bool IsActive);
