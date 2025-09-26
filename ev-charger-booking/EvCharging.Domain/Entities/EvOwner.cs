using EvCharging.Domain.Common;


namespace EvCharging.Domain.Entities;


public class EvOwner : EntityBase
{
    // Business rule: NIC acts as unique identifier (we also store it as Id for convenience)
    public string Nic { get; set; } = default!; // also used as Id
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public bool IsActive { get; set; } = true;
}