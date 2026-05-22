using quote_service.Models;
using quote_service.Services;

namespace quote_service.tests;

public sealed class CarrierComparisonServiceTests
{
    private readonly CarrierComparisonService service = new();

    [Fact]
    public void Cheapest_SelectsLowestTotalAmount()
    {
        var result = service.SelectRecommendedOption(BuildOptions(), "Cheapest");

        Assert.Equal("Xpressbees", result.Carrier);
    }

    [Fact]
    public void Fastest_SelectsLowestEta()
    {
        var result = service.SelectRecommendedOption(BuildOptions(), "Fastest");

        Assert.Equal("BlueDart", result.Carrier);
    }

    [Fact]
    public void Fastest_TieSelectsLowerAmount()
    {
        var options = new[]
        {
            Option("A", 150m, 2),
            Option("B", 120m, 2),
            Option("C", 100m, 4)
        };

        var result = service.SelectRecommendedOption(options, "Fastest");

        Assert.Equal("B", result.Carrier);
    }

    [Fact]
    public void Balanced_SelectsCheapest_WhenFasterOptionExceedsThreshold()
    {
        var result = service.SelectRecommendedOption(BuildOptions(), "Balanced");

        Assert.Equal("Xpressbees", result.Carrier);
    }

    [Fact]
    public void Balanced_SelectsFasterOption_WhenWithinThresholdAndSavesAtLeastOneDay()
    {
        var options = new[]
        {
            Option("Economy", 100m, 5),
            Option("Smart", 114m, 4),
            Option("Express", 150m, 2)
        };

        var result = service.SelectRecommendedOption(options, "Balanced");

        Assert.Equal("Smart", result.Carrier);
    }

    private static IReadOnlyCollection<CarrierQuoteOptionDto> BuildOptions()
    {
        return
        [
            Option("Xpressbees", 166.14m, 5),
            Option("Delhivery", 192.95m, 4),
            Option("BlueDart", 284.97m, 2)
        ];
    }

    private static CarrierQuoteOptionDto Option(string carrier, decimal amount, int etaDays)
    {
        return new CarrierQuoteOptionDto
        {
            Carrier = carrier,
            ServiceType = "Surface",
            BaseFreight = amount,
            FuelSurcharge = 0m,
            Gst = 0m,
            TotalAmount = amount,
            EstimatedDeliveryDays = etaDays
        };
    }
}
