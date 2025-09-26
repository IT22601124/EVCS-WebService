using EvCharging.Domain.Common;


namespace EvCharging.Domain.Entities;


public class Station : EntityBase
{
    public string Name { get; set; } = default!;
    public string Address { get; set; } = default!;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Type { get; set; } = "AC"; // AC | DC
    public int Slots { get; set; }
    public bool IsActive { get; set; } = true;
}