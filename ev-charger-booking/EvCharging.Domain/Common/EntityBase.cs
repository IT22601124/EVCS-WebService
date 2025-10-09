namespace EvCharging.Domain.Common;

public abstract class EntityBase
{
    public string Id { get; set; } = default!; // Mongo string id
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}