using quote_service.Entities;
using quote_service.Models;

namespace quote_service.Services;

public sealed class FreightCalculatorService : IFreightCalculatorService
{
    private const decimal VolumetricDivisor = 5000m;

    public QuoteBreakdownDto CalculateWeights(QuoteCompareRequest request)
    {
        var volumetricWeight = RoundWeight(request.LengthCm * request.WidthCm * request.HeightCm / VolumetricDivisor);
        var actualWeight = RoundWeight(request.ActualWeightKg);
        var chargeableWeight = Math.Max(actualWeight, volumetricWeight);

        return new QuoteBreakdownDto
        {
            ActualWeightKg = actualWeight,
            VolumetricWeightKg = volumetricWeight,
            ChargeableWeightKg = chargeableWeight
        };
    }

    public CarrierQuoteOptionDto CalculateOption(CarrierRateRule rule, decimal chargeableWeightKg)
    {
        var baseFreight = RoundMoney(rule.BaseRate + chargeableWeightKg * rule.PerKgRate);
        var fuelSurcharge = RoundMoney(baseFreight * rule.FuelSurchargePercent / 100m);
        var taxableAmount = baseFreight + fuelSurcharge;
        var gst = RoundMoney(taxableAmount * rule.GstPercent / 100m);
        var totalAmount = RoundMoney(baseFreight + fuelSurcharge + gst);

        return new CarrierQuoteOptionDto
        {
            Carrier = rule.Carrier.Name,
            ServiceType = rule.Carrier.ServiceType,
            BaseFreight = baseFreight,
            FuelSurcharge = fuelSurcharge,
            Gst = gst,
            TotalAmount = totalAmount,
            EstimatedDeliveryDays = rule.EstimatedDeliveryDays
        };
    }

    private static decimal RoundMoney(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    private static decimal RoundWeight(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}
