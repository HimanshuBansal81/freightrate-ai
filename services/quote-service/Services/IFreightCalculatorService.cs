using quote_service.Entities;
using quote_service.Models;

namespace quote_service.Services;

public interface IFreightCalculatorService
{
    QuoteBreakdownDto CalculateWeights(QuoteCompareRequest request);
    CarrierQuoteOptionDto CalculateOption(CarrierRateRule rule, decimal chargeableWeightKg);
}
