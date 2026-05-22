namespace quote_service.Models;

public sealed class CarrierRateRuleDto
{
    public int Id { get; set; }
    public required ActiveCarrierDto Carrier { get; set; }
    public required string OriginZone { get; set; }
    public required string DestinationZone { get; set; }
    public decimal BaseRate { get; set; }
    public decimal PerKgRate { get; set; }
    public decimal FuelSurchargePercent { get; set; }
    public decimal GstPercent { get; set; }
    public int EstimatedDeliveryDays { get; set; }
    public bool IsActive { get; set; }
}
