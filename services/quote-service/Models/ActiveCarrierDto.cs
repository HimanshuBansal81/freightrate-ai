namespace quote_service.Models;

public sealed class ActiveCarrierDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Code { get; set; }
    public required string ServiceType { get; set; }
    public bool IsActive { get; set; }
}
