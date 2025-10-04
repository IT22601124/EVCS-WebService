namespace EvCharging.Application.DTOs;


public record LoginRequest(string Username, string Password);
public record LoginResponse(
    string Nic,
    string FullName,
    string Email,
    string Phone,
    bool IsActive,
    string Role,
    string Token,
    DateTime ExpiresAt,
    string Username,
    bool IsOwner,
    string? OwnerNic
);