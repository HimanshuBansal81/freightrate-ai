namespace quote_service.Entities;

public sealed class ZoneMapping
{
    public int Id { get; set; }
    public required string Pincode { get; set; }
    public required string ZoneName { get; set; }
    public required string City { get; set; }
    public required string State { get; set; }
    public bool IsActive { get; set; } = true;
}
