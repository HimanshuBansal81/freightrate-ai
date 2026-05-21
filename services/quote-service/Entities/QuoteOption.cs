namespace quote_service.Entities;

public sealed class QuoteOption
{
    public int Id { get; set; }
    public int QuoteRequestId { get; set; }
    public required string CarrierName { get; set; }
    public required string ServiceType { get; set; }
    public decimal BaseFreight { get; set; }
    public decimal FuelSurcharge { get; set; }
    public decimal Gst { get; set; }
    public decimal TotalAmount { get; set; }
    public int EstimatedDeliveryDays { get; set; }

    public QuoteRequest QuoteRequest { get; set; } = null!;
}
