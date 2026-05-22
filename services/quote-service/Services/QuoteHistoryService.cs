using Microsoft.EntityFrameworkCore;
using quote_service.Data;
using quote_service.Entities;
using quote_service.Models;

namespace quote_service.Services;

public sealed class QuoteHistoryService(
    QuoteDbContext dbContext,
    IFreightCalculatorService freightCalculator,
    ICarrierComparisonService carrierComparison,
    ICurrentUserService currentUser) : IQuoteHistoryService
{
    public async Task<QuoteCompareResponse> CompareAndSaveAsync(
        QuoteCompareRequest request,
        CancellationToken cancellationToken)
    {
        var preference = carrierComparison.NormalizePreference(request.Preference);
        Validate(request);

        var originPincode = request.OriginPincode!.Trim();
        var destinationPincode = request.DestinationPincode!.Trim();

        var originZone = await dbContext.ZoneMappings
            .AsNoTracking()
            .SingleOrDefaultAsync(zone => zone.IsActive && zone.Pincode == originPincode, cancellationToken);
        if (originZone is null)
        {
            throw new QuoteBusinessException($"No active zone mapping found for origin pincode {originPincode}.");
        }

        var destinationZone = await dbContext.ZoneMappings
            .AsNoTracking()
            .SingleOrDefaultAsync(zone => zone.IsActive && zone.Pincode == destinationPincode, cancellationToken);
        if (destinationZone is null)
        {
            throw new QuoteBusinessException($"No active zone mapping found for destination pincode {destinationPincode}.");
        }

        var rateRules = await dbContext.CarrierRateRules
            .AsNoTracking()
            .Include(rule => rule.Carrier)
            .Where(rule =>
                rule.IsActive
                && rule.Carrier.IsActive
                && rule.OriginZone == originZone.ZoneName
                && rule.DestinationZone == destinationZone.ZoneName)
            .ToListAsync(cancellationToken);

        if (rateRules.Count == 0)
        {
            throw new QuoteBusinessException(
                $"No active carrier rate rules found for {originZone.ZoneName} to {destinationZone.ZoneName}.");
        }

        var weights = freightCalculator.CalculateWeights(request);
        var options = rateRules
            .Select(rule => freightCalculator.CalculateOption(rule, weights.ChargeableWeightKg))
            .OrderBy(option => option.TotalAmount)
            .ThenBy(option => option.EstimatedDeliveryDays)
            .ToList();
        var recommendedOption = carrierComparison.SelectRecommendedOption(options, preference);
        var aiExplanation =
            $"{recommendedOption.Carrier} was selected because it best matches your {preference} preference based on calculated price and delivery time.";

        var quoteRequest = new QuoteRequest
        {
            UserId = currentUser.UserId,
            OriginPincode = originPincode,
            DestinationPincode = destinationPincode,
            OriginZone = originZone.ZoneName,
            DestinationZone = destinationZone.ZoneName,
            ActualWeightKg = weights.ActualWeightKg,
            VolumetricWeightKg = weights.VolumetricWeightKg,
            ChargeableWeightKg = weights.ChargeableWeightKg,
            LengthCm = request.LengthCm,
            WidthCm = request.WidthCm,
            HeightCm = request.HeightCm,
            Preference = preference,
            RecommendedCarrier = recommendedOption.Carrier,
            RecommendedAmount = recommendedOption.TotalAmount,
            AiExplanation = aiExplanation,
            QuoteOptions = options.Select(option => new QuoteOption
            {
                CarrierName = option.Carrier,
                ServiceType = option.ServiceType,
                BaseFreight = option.BaseFreight,
                FuelSurcharge = option.FuelSurcharge,
                Gst = option.Gst,
                TotalAmount = option.TotalAmount,
                EstimatedDeliveryDays = option.EstimatedDeliveryDays
            }).ToList()
        };

        dbContext.QuoteRequests.Add(quoteRequest);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new QuoteCompareResponse
        {
            QuoteId = quoteRequest.Id,
            OriginZone = quoteRequest.OriginZone,
            DestinationZone = quoteRequest.DestinationZone,
            ActualWeightKg = quoteRequest.ActualWeightKg,
            VolumetricWeightKg = quoteRequest.VolumetricWeightKg,
            ChargeableWeightKg = quoteRequest.ChargeableWeightKg,
            Preference = quoteRequest.Preference,
            RecommendedCarrier = quoteRequest.RecommendedCarrier!,
            RecommendedAmount = quoteRequest.RecommendedAmount!.Value,
            AiExplanation = quoteRequest.AiExplanation!,
            Options = options
        };
    }

    public async Task<IReadOnlyCollection<QuoteHistoryResponse>> GetRecentQuotesAsync(CancellationToken cancellationToken)
    {
        var query = dbContext.QuoteRequests
            .AsNoTracking()
            .Include(request => request.QuoteOptions)
            .AsQueryable();

        if (!currentUser.IsAdmin)
        {
            var userId = currentUser.UserId;
            query = query.Where(request => request.UserId == userId);
        }

        var quoteRequests = await query
            .OrderByDescending(request => request.CreatedAt)
            .ToListAsync(cancellationToken);

        return quoteRequests.Select(ToHistoryResponse).ToList();
    }

    public async Task<QuoteHistoryResponse?> GetQuoteAsync(int id, CancellationToken cancellationToken)
    {
        var query = dbContext.QuoteRequests
            .AsNoTracking()
            .Include(request => request.QuoteOptions)
            .Where(request => request.Id == id);

        if (!currentUser.IsAdmin)
        {
            var userId = currentUser.UserId;
            query = query.Where(request => request.UserId == userId);
        }

        var quoteRequest = await query.SingleOrDefaultAsync(cancellationToken);

        return quoteRequest is null ? null : ToHistoryResponse(quoteRequest);
    }

    private static void Validate(QuoteCompareRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.OriginPincode))
        {
            errors["OriginPincode"] = ["OriginPincode is required."];
        }

        if (string.IsNullOrWhiteSpace(request.DestinationPincode))
        {
            errors["DestinationPincode"] = ["DestinationPincode is required."];
        }

        if (request.ActualWeightKg <= 0)
        {
            errors["ActualWeightKg"] = ["ActualWeightKg must be greater than 0."];
        }

        if (request.LengthCm <= 0)
        {
            errors["LengthCm"] = ["LengthCm must be greater than 0."];
        }

        if (request.WidthCm <= 0)
        {
            errors["WidthCm"] = ["WidthCm must be greater than 0."];
        }

        if (request.HeightCm <= 0)
        {
            errors["HeightCm"] = ["HeightCm must be greater than 0."];
        }

        if (errors.Count > 0)
        {
            throw new QuoteValidationException(errors);
        }
    }

    private static QuoteHistoryResponse ToHistoryResponse(QuoteRequest request)
    {
        return new QuoteHistoryResponse
        {
            QuoteId = request.Id,
            OriginPincode = request.OriginPincode,
            DestinationPincode = request.DestinationPincode,
            OriginZone = request.OriginZone,
            DestinationZone = request.DestinationZone,
            ActualWeightKg = request.ActualWeightKg,
            VolumetricWeightKg = request.VolumetricWeightKg,
            ChargeableWeightKg = request.ChargeableWeightKg,
            Preference = request.Preference,
            RecommendedCarrier = request.RecommendedCarrier,
            RecommendedAmount = request.RecommendedAmount,
            AiExplanation = request.AiExplanation,
            CreatedAt = request.CreatedAt,
            Options = request.QuoteOptions
                .OrderBy(option => option.TotalAmount)
                .ThenBy(option => option.EstimatedDeliveryDays)
                .Select(option => new CarrierQuoteOptionDto
                {
                    Carrier = option.CarrierName,
                    ServiceType = option.ServiceType,
                    BaseFreight = option.BaseFreight,
                    FuelSurcharge = option.FuelSurcharge,
                    Gst = option.Gst,
                    TotalAmount = option.TotalAmount,
                    EstimatedDeliveryDays = option.EstimatedDeliveryDays
                })
                .ToList()
        };
    }
}
