namespace quote_service.Models;

public sealed class AiRecommendationRequest
{
    public required string Preference { get; set; }
    public required string RecommendedCarrier { get; set; }
    public required string CheapestCarrier { get; set; }
    public required string FastestCarrier { get; set; }
    public required IReadOnlyCollection<AiRecommendationOption> Options { get; set; }
}

public sealed class AiRecommendationOption
{
    public required string Carrier { get; set; }
    public decimal Amount { get; set; }
    public int EtaDays { get; set; }
}

public sealed class AiRecommendationResponse
{
    public string? Explanation { get; set; }
}
