using quote_service.Models;

namespace quote_service.Services;

public interface ICarrierComparisonService
{
    string NormalizePreference(string? preference);
    CarrierQuoteOptionDto SelectRecommendedOption(IReadOnlyCollection<CarrierQuoteOptionDto> options, string preference);
}
