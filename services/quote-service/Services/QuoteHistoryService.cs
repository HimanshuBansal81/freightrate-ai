using Microsoft.EntityFrameworkCore;
using quote_service.Data;
using quote_service.Entities;
using quote_service.Models;

namespace quote_service.Services;

public sealed class QuoteHistoryService(
    QuoteDbContext dbContext,
    IFreightCalculatorService freightCalculator,
    ICarrierComparisonService carrierComparison,
    ICurrentUserService currentUser,
    IRedisCacheService cache,
    IAiRecommendationClient aiRecommendationClient) : IQuoteHistoryService
{
    public async Task<QuoteCompareResponse> CompareAndSaveAsync(
        QuoteCompareRequest request,
        CancellationToken cancellationToken)
    {
        var preference = carrierComparison.NormalizePreference(request.Preference);
        Validate(request);

        var originPincode = request.OriginPincode!.Trim();
        var destinationPincode = request.DestinationPincode!.Trim();

        var originZone = await GetZoneMappingAsync(originPincode, cancellationToken);
        if (originZone is null)
        {
            throw new QuoteBusinessException(
                QuoteErrorCodes.InvalidPincode,
                "Origin pincode is not serviceable.",
                [new ErrorDetail { Field = "originPincode", Issue = $"No zone mapping found for pincode {originPincode}." }]);
        }

        var destinationZone = await GetZoneMappingAsync(destinationPincode, cancellationToken);
        if (destinationZone is null)
        {
            throw new QuoteBusinessException(
                QuoteErrorCodes.InvalidPincode,
                "Destination pincode is not serviceable.",
                [new ErrorDetail { Field = "destinationPincode", Issue = $"No zone mapping found for pincode {destinationPincode}." }]);
        }

        var rateRules = await GetRateRulesAsync(originZone.ZoneName, destinationZone.ZoneName, cancellationToken);

        if (rateRules.Count == 0)
        {
            throw new QuoteBusinessException(
                QuoteErrorCodes.NoActiveRateRules,
                $"No active carrier rate rules found for {originZone.ZoneName} to {destinationZone.ZoneName}.",
                [
                    new ErrorDetail
                    {
                        Field = "route",
                        Issue = $"No active rate rules found for {originZone.ZoneName} to {destinationZone.ZoneName}."
                    }
                ]);
        }

        var weights = freightCalculator.CalculateWeights(request);
        var options = rateRules
            .Select(rule => freightCalculator.CalculateOption(rule, weights.ChargeableWeightKg))
            .OrderBy(option => option.TotalAmount)
            .ThenBy(option => option.EstimatedDeliveryDays)
            .ToList();
        var recommendedOption = carrierComparison.SelectRecommendedOption(options, preference);
        var aiExplanation = await aiRecommendationClient.GetExplanationAsync(
            preference,
            recommendedOption,
            options,
            cancellationToken);

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

    private async Task<ZoneMappingDto?> GetZoneMappingAsync(string pincode, CancellationToken cancellationToken)
    {
        var cacheKey = $"zone:pincode:{pincode}";
        var cachedZone = await cache.GetAsync<ZoneMappingDto>(cacheKey, cancellationToken);
        if (cachedZone is not null)
        {
            return cachedZone;
        }

        var zone = await dbContext.ZoneMappings
            .AsNoTracking()
            .Where(existingZone => existingZone.IsActive && existingZone.Pincode == pincode)
            .Select(existingZone => new ZoneMappingDto
            {
                Id = existingZone.Id,
                Pincode = existingZone.Pincode,
                ZoneName = existingZone.ZoneName,
                City = existingZone.City,
                State = existingZone.State,
                IsActive = existingZone.IsActive
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (zone is not null)
        {
            await cache.SetAsync(cacheKey, zone, TimeSpan.FromHours(6), cancellationToken);
        }

        return zone;
    }

    private async Task<IReadOnlyCollection<CarrierRateRuleDto>> GetRateRulesAsync(
        string originZone,
        string destinationZone,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"rate-rules:{originZone}:{destinationZone}";
        var cachedRules = await cache.GetAsync<IReadOnlyCollection<CarrierRateRuleDto>>(cacheKey, cancellationToken);
        if (cachedRules is not null)
        {
            return cachedRules;
        }

        var rateRules = await dbContext.CarrierRateRules
            .AsNoTracking()
            .Include(rule => rule.Carrier)
            .Where(rule =>
                rule.IsActive
                && rule.Carrier.IsActive
                && rule.OriginZone == originZone
                && rule.DestinationZone == destinationZone)
            .Select(rule => new CarrierRateRuleDto
            {
                Id = rule.Id,
                Carrier = new ActiveCarrierDto
                {
                    Id = rule.Carrier.Id,
                    Name = rule.Carrier.Name,
                    Code = rule.Carrier.Code,
                    ServiceType = rule.Carrier.ServiceType,
                    IsActive = rule.Carrier.IsActive
                },
                OriginZone = rule.OriginZone,
                DestinationZone = rule.DestinationZone,
                BaseRate = rule.BaseRate,
                PerKgRate = rule.PerKgRate,
                FuelSurchargePercent = rule.FuelSurchargePercent,
                GstPercent = rule.GstPercent,
                EstimatedDeliveryDays = rule.EstimatedDeliveryDays,
                IsActive = rule.IsActive
            })
            .ToListAsync(cancellationToken);

        await cache.SetAsync(cacheKey, rateRules, TimeSpan.FromMinutes(30), cancellationToken);
        return rateRules;
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
        var pincodeErrors = new List<ErrorDetail>();
        var weightErrors = new List<ErrorDetail>();
        var dimensionErrors = new List<ErrorDetail>();

        if (string.IsNullOrWhiteSpace(request.OriginPincode))
        {
            pincodeErrors.Add(new ErrorDetail { Field = "originPincode", Issue = "OriginPincode is required." });
        }

        if (string.IsNullOrWhiteSpace(request.DestinationPincode))
        {
            pincodeErrors.Add(new ErrorDetail { Field = "destinationPincode", Issue = "DestinationPincode is required." });
        }

        if (request.ActualWeightKg <= 0)
        {
            weightErrors.Add(new ErrorDetail { Field = "actualWeightKg", Issue = "ActualWeightKg must be greater than 0." });
        }

        if (request.LengthCm <= 0)
        {
            dimensionErrors.Add(new ErrorDetail { Field = "lengthCm", Issue = "LengthCm must be greater than 0." });
        }

        if (request.WidthCm <= 0)
        {
            dimensionErrors.Add(new ErrorDetail { Field = "widthCm", Issue = "WidthCm must be greater than 0." });
        }

        if (request.HeightCm <= 0)
        {
            dimensionErrors.Add(new ErrorDetail { Field = "heightCm", Issue = "HeightCm must be greater than 0." });
        }

        if (pincodeErrors.Count > 0)
        {
            throw new QuoteValidationException(
                QuoteErrorCodes.InvalidPincode,
                "Pincode validation failed.",
                pincodeErrors);
        }

        if (weightErrors.Count > 0)
        {
            throw new QuoteValidationException(
                QuoteErrorCodes.InvalidWeight,
                "Weight validation failed.",
                weightErrors);
        }

        if (dimensionErrors.Count > 0)
        {
            throw new QuoteValidationException(
                QuoteErrorCodes.InvalidDimensions,
                "Dimension validation failed.",
                dimensionErrors);
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
