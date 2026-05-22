using System.Net.Http.Json;
using quote_service.Models;

namespace quote_service.Services;

public sealed class AiRecommendationClient(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<AiRecommendationClient> logger) : IAiRecommendationClient
{
    public async Task<string> GetExplanationAsync(
        string preference,
        CarrierQuoteOptionDto recommendedOption,
        IReadOnlyCollection<CarrierQuoteOptionDto> options,
        CancellationToken cancellationToken)
    {
        var fallback = BuildFallbackExplanation(recommendedOption.Carrier, preference);

        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(configuration.GetValue("AiService:TimeoutSeconds", 3)));

            var cheapest = options.OrderBy(option => option.TotalAmount).First();
            var fastest = options
                .OrderBy(option => option.EstimatedDeliveryDays)
                .ThenBy(option => option.TotalAmount)
                .First();

            var request = new AiRecommendationRequest
            {
                Preference = preference,
                RecommendedCarrier = recommendedOption.Carrier,
                CheapestCarrier = cheapest.Carrier,
                FastestCarrier = fastest.Carrier,
                Options = options.Select(option => new AiRecommendationOption
                {
                    Carrier = option.Carrier,
                    Amount = option.TotalAmount,
                    EtaDays = option.EstimatedDeliveryDays
                }).ToList()
            };

            using var response = await httpClient.PostAsJsonAsync(
                "/api/recommendations/explain",
                request,
                timeoutCts.Token);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("AI recommendation service returned status {StatusCode}. Using fallback explanation.", response.StatusCode);
                return fallback;
            }

            var aiResponse = await response.Content.ReadFromJsonAsync<AiRecommendationResponse>(cancellationToken: timeoutCts.Token);
            var explanation = aiResponse?.Explanation?.Trim();
            if (string.IsNullOrWhiteSpace(explanation))
            {
                logger.LogWarning("AI recommendation service returned an empty explanation. Using fallback explanation.");
                return fallback;
            }

            return explanation;
        }
        catch (Exception exception) when (exception is HttpRequestException
                                            or TaskCanceledException
                                            or OperationCanceledException
                                            or System.Text.Json.JsonException)
        {
            logger.LogWarning(exception, "AI recommendation service call failed. Using fallback explanation.");
            return fallback;
        }
    }

    public static string BuildFallbackExplanation(string carrier, string preference)
    {
        return $"{carrier} was selected because it best matches your {preference} preference based on the calculated price and delivery time.";
    }
}
