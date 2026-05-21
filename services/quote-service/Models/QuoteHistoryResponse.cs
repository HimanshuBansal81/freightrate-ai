namespace quote_service.Models;

public sealed class QuoteHistoryResponse
{
    public int QuoteId { get; set; }
    public required string OriginPincode { get; set; }
    public required string DestinationPincode { get; set; }
    public required string OriginZone { get; set; }
    public required string DestinationZone { get; set; }
    public decimal ActualWeightKg { get; set; }
    public decimal VolumetricWeightKg { get; set; }
    public decimal ChargeableWeightKg { get; set; }
    public required string Preference { get; set; }
    public string? RecommendedCarrier { get; set; }
    public decimal? RecommendedAmount { get; set; }
    public string? AiExplanation { get; set; }
    public DateTime CreatedAt { get; set; }
    public required IReadOnlyCollection<CarrierQuoteOptionDto> Options { get; set; }
}
