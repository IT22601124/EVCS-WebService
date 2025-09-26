namespace EvCharging.Application.DTOs;


public record CreateOwnerRequest(string Nic, string FullName, string Email, string Phone);
public record UpdateOwnerRequest(string FullName, string Email, string Phone, bool IsActive);
public record OwnerResponse(string Nic, string FullName, string Email, string Phone, bool IsActive);