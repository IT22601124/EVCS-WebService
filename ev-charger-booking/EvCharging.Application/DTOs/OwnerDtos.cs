namespace EvCharging.Application.DTOs;


public record CreateOwnerRequest(string Nic, string FullName, string Email, string Phone, string Password);
public record UpdateOwnerRequest(string FullName, string Email, string Phone, bool IsActive);
public record OwnerResponse(string Nic, string FullName, string Email, string Phone, bool IsActive);
public record OwnerRegistrationResponse(string Nic, string FullName, string Email, string Phone, bool IsActive, string Role);