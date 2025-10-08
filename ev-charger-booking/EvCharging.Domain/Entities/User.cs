using EvCharging.Domain.Common;
using EvCharging.Domain.Enums;


namespace EvCharging.Domain.Entities;


public class User : EntityBase
{
    public string Username { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string Role { get; set; } = Roles.Backoffice; // Backoffice | Operator
    public bool IsActive { get; set; } = true;
    public string Nic { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public List<string> AssignedStations { get; set; } = new(); // Station IDs for operators

    // Operator assignment (nullable). One operator -> one station
    public string? AssignedStationId { get; set; }
}