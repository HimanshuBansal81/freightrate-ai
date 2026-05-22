using quote_service.Models;

namespace quote_service.Services;

public sealed class CarrierComparisonService : ICarrierComparisonService
{
    private static readonly HashSet<string> AllowedPreferences = new(StringComparer.OrdinalIgnoreCase)
    {
        "Cheapest",
        "Fastest",
        "Balanced"
    };

    public string NormalizePreference(string? preference)
    {
        if (string.IsNullOrWhiteSpace(preference))
        {
            return "Balanced";
        }

        var normalized = preference.Trim();
        if (!AllowedPreferences.Contains(normalized))
        {
            throw new QuoteValidationException(
                QuoteErrorCodes.InvalidPreference,
                "Preference must be Cheapest, Fastest, or Balanced.",
                [new ErrorDetail { Field = "preference", Issue = "Allowed values are Cheapest, Fastest, or Balanced." }],
                StatusCodes.Status400BadRequest);
        }

        return AllowedPreferences.Single(allowed =>
            string.Equals(allowed, normalized, StringComparison.OrdinalIgnoreCase));
    }

    public CarrierQuoteOptionDto SelectRecommendedOption(IReadOnlyCollection<CarrierQuoteOptionDto> options, string preference)
    {
        if (options.Count == 0)
        {
            throw new QuoteBusinessException(
                QuoteErrorCodes.NoActiveRateRules,
                "No quote options are available for comparison.");
        }

        return preference switch
        {
            "Cheapest" => SelectCheapest(options),
            "Fastest" => SelectFastest(options),
            _ => SelectBalanced(options)
        };
    }

    private static CarrierQuoteOptionDto SelectCheapest(IEnumerable<CarrierQuoteOptionDto> options)
    {
        return options
            .OrderBy(option => option.TotalAmount)
            .ThenBy(option => option.EstimatedDeliveryDays)
            .First();
    }

    private static CarrierQuoteOptionDto SelectFastest(IEnumerable<CarrierQuoteOptionDto> options)
    {
        return options
            .OrderBy(option => option.EstimatedDeliveryDays)
            .ThenBy(option => option.TotalAmount)
            .First();
    }

    private static CarrierQuoteOptionDto SelectBalanced(IReadOnlyCollection<CarrierQuoteOptionDto> options)
    {
        var cheapest = SelectCheapest(options);
        var thresholdAmount = cheapest.TotalAmount * 1.15m;
        var qualifyingFasterOptions = options
            .Where(option =>
                option.EstimatedDeliveryDays <= cheapest.EstimatedDeliveryDays - 1
                && option.TotalAmount <= thresholdAmount)
            .OrderBy(option => option.EstimatedDeliveryDays)
            .ThenBy(option => option.TotalAmount)
            .ToList();

        return qualifyingFasterOptions.Count > 0 ? qualifyingFasterOptions[0] : cheapest;
    }
}
