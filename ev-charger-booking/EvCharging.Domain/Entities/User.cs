using EvCharging.Domain.Common;
using EvCharging.Domain.Enums;


namespace EvCharging.Domain.Entities;


public class User : EntityBase
{
    public string Username { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string Role { get; set; } = Roles.Backoffice; // Backoffice | Operator
    public bool IsActive { get; set; } = true;
}