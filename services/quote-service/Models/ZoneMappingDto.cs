namespace quote_service.Models;

public sealed class ZoneMappingDto
{
    public int Id { get; set; }
    public required string Pincode { get; set; }
    public required string ZoneName { get; set; }
    public required string City { get; set; }
    public required string State { get; set; }
    public bool IsActive { get; set; }
}
