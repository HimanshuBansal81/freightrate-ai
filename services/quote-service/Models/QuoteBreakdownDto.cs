namespace quote_service.Models;

public sealed class QuoteBreakdownDto
{
    public decimal ActualWeightKg { get; set; }
    public decimal VolumetricWeightKg { get; set; }
    public decimal ChargeableWeightKg { get; set; }
}
