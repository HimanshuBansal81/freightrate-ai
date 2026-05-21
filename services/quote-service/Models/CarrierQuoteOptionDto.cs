namespace quote_service.Models;

public sealed class CarrierQuoteOptionDto
{
    public required string Carrier { get; set; }
    public required string ServiceType { get; set; }
    public decimal BaseFreight { get; set; }
    public decimal FuelSurcharge { get; set; }
    public decimal Gst { get; set; }
    public decimal TotalAmount { get; set; }
    public int EstimatedDeliveryDays { get; set; }
}
