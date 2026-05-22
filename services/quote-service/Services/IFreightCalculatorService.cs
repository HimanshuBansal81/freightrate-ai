using quote_service.Models;

namespace quote_service.Services;

public interface IFreightCalculatorService
{
    QuoteBreakdownDto CalculateWeights(QuoteCompareRequest request);
    CarrierQuoteOptionDto CalculateOption(CarrierRateRuleDto rule, decimal chargeableWeightKg);
}
