namespace quote_service.Entities;

public sealed class CarrierRateRule
{
    public int Id { get; set; }
    public int CarrierId { get; set; }
    public required string OriginZone { get; set; }
    public required string DestinationZone { get; set; }
    public decimal BaseRate { get; set; }
    public decimal PerKgRate { get; set; }
    public decimal FuelSurchargePercent { get; set; }
    public decimal GstPercent { get; set; }
    public int EstimatedDeliveryDays { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Carrier Carrier { get; set; } = null!;
}
