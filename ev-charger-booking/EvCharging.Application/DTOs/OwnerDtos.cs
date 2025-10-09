namespace EvCharging.Application.DTOs;


public record CreateOwnerRequest(string Nic, string FullName, string Email, string Phone, string Password);
public record UpdateOwnerRequest(string FullName, string Email, string Phone, bool IsActive);
public record OwnerResponse(string Nic, string FullName, string Email, string Phone, bool IsActive);
public record OwnerRegistrationResponse(string Nic, string FullName, string Email, string Phone, bool IsActive, string Role);
public record CreateAdminUserRequest(string Username, string Password, string Role, string Nic, string FullName, string Email, string Phone, List<string>? AssignedStations = null);
public record UserRegistrationResponse(string Username, string Role, string Nic, string FullName, string Email, string Phone, bool IsActive, List<string> AssignedStations);
public record AssignStationToOperatorRequest(string OperatorUsername, string StationId);
public record AssignStationResponse(string OperatorUsername, List<string> AssignedStations);