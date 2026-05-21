namespace quote_service.Models;

public sealed class QuoteCompareResponse
{
    public int QuoteId { get; set; }
    public required string OriginZone { get; set; }
    public required string DestinationZone { get; set; }
    public decimal ActualWeightKg { get; set; }
    public decimal VolumetricWeightKg { get; set; }
    public decimal ChargeableWeightKg { get; set; }
    public required string Preference { get; set; }
    public required string RecommendedCarrier { get; set; }
    public decimal RecommendedAmount { get; set; }
    public required string AiExplanation { get; set; }
    public required IReadOnlyCollection<CarrierQuoteOptionDto> Options { get; set; }
}
