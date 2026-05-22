using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using quote_service.Models;
using quote_service.Services;

namespace quote_service.tests;

public sealed class AiRecommendationClientTests
{
    [Fact]
    public async Task GetExplanationAsync_ReturnsFallback_WhenAiServiceUnavailable()
    {
        using var httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://127.0.0.1:9")
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AiService:TimeoutSeconds"] = "1"
            })
            .Build();
        var client = new AiRecommendationClient(
            httpClient,
            configuration,
            NullLogger<AiRecommendationClient>.Instance);
        var recommendedOption = new CarrierQuoteOptionDto
        {
            Carrier = "Xpressbees",
            ServiceType = "Surface",
            BaseFreight = 128m,
            FuelSurcharge = 12.80m,
            Gst = 25.34m,
            TotalAmount = 166.14m,
            EstimatedDeliveryDays = 5
        };

        var result = await client.GetExplanationAsync(
            "Balanced",
            recommendedOption,
            [recommendedOption],
            CancellationToken.None);

        Assert.Equal(
            "Xpressbees was selected because it best matches your Balanced preference based on the calculated price and delivery time.",
            result);
    }
}
