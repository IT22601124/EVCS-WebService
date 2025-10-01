namespace EvCharging.Application.DTOs;


public record LoginRequest(string Username, string Password);
public record LoginResponse(
    string Token, 
    DateTime ExpiresAt, 
    string Role, 
    string Username, 
    bool IsOwner = false, 
    string? OwnerNic = null,
    string? FullName = null,
    string? Email = null,
    string? Phone = null
);