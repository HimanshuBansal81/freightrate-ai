using quote_service.Models;
using quote_service.Services;

namespace quote_service.tests;

public sealed class FreightCalculatorServiceTests
{
    private readonly FreightCalculatorService service = new();

    [Fact]
    public void CalculateWeights_UsesVolumetricDivisorAndChargeableWeight()
    {
        var request = new QuoteCompareRequest
        {
            OriginPincode = "110001",
            DestinationPincode = "560001",
            ActualWeightKg = 8m,
            LengthCm = 40m,
            WidthCm = 30m,
            HeightCm = 25m,
            Preference = "Balanced"
        };

        var result = service.CalculateWeights(request);

        Assert.Equal(8m, result.ActualWeightKg);
        Assert.Equal(6m, result.VolumetricWeightKg);
        Assert.Equal(8m, result.ChargeableWeightKg);
    }

    [Fact]
    public void CalculateOption_ReturnsExpectedFreightBreakdown()
    {
        var rule = new CarrierRateRuleDto
        {
            Id = 1,
            Carrier = new ActiveCarrierDto
            {
                Id = 1,
                Name = "Delhivery",
                Code = "DELHIVERY",
                ServiceType = "Surface",
                IsActive = true
            },
            OriginZone = "North",
            DestinationZone = "South",
            BaseRate = 90m,
            PerKgRate = 7m,
            FuelSurchargePercent = 12m,
            GstPercent = 18m,
            EstimatedDeliveryDays = 4,
            IsActive = true
        };

        var result = service.CalculateOption(rule, 8m);

        Assert.Equal(146m, result.BaseFreight);
        Assert.Equal(17.52m, result.FuelSurcharge);
        Assert.Equal(29.43m, result.Gst);
        Assert.Equal(192.95m, result.TotalAmount);
    }
}
