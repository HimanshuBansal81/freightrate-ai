namespace quote_service.Entities;

public sealed class QuoteRequest
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public required string OriginPincode { get; set; }
    public required string DestinationPincode { get; set; }
    public required string OriginZone { get; set; }
    public required string DestinationZone { get; set; }
    public decimal ActualWeightKg { get; set; }
    public decimal VolumetricWeightKg { get; set; }
    public decimal ChargeableWeightKg { get; set; }
    public decimal LengthCm { get; set; }
    public decimal WidthCm { get; set; }
    public decimal HeightCm { get; set; }
    public required string Preference { get; set; }
    public string? RecommendedCarrier { get; set; }
    public decimal? RecommendedAmount { get; set; }
    public string? AiExplanation { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<QuoteOption> QuoteOptions { get; set; } = new List<QuoteOption>();
}
