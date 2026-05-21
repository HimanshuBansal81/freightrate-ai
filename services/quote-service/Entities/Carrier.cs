namespace quote_service.Entities;

public sealed class Carrier
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Code { get; set; }
    public required string ServiceType { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CarrierRateRule> RateRules { get; set; } = new List<CarrierRateRule>();
}
