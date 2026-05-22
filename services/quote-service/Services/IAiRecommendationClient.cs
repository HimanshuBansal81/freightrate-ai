using quote_service.Models;

namespace quote_service.Services;

public interface IAiRecommendationClient
{
    Task<string> GetExplanationAsync(
        string preference,
        CarrierQuoteOptionDto recommendedOption,
        IReadOnlyCollection<CarrierQuoteOptionDto> options,
        CancellationToken cancellationToken);
}
