namespace EvCharging.Application.DTOs;


public record LoginRequest(string Username, string Password);
public record LoginResponse(string Token, DateTime ExpiresAt, string Role, string Username);