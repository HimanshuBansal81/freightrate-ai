using Microsoft.EntityFrameworkCore;
using quote_service.Data;
using quote_service.Entities;
using quote_service.Models;
using quote_service.Services;

namespace quote_service.tests;

public sealed class RedisFallbackTests
{
    [Fact]
    public async Task CompareAndSaveAsync_UsesDatabase_WhenCacheMisses()
    {
        var options = new DbContextOptionsBuilder<QuoteDbContext>()
            .UseInMemoryDatabase($"quote-test-{Guid.NewGuid()}")
            .Options;
        await using var dbContext = new QuoteDbContext(options);
        Seed(dbContext);
        var service = new QuoteHistoryService(
            dbContext,
            new FreightCalculatorService(),
            new CarrierComparisonService(),
            new TestCurrentUserService(),
            new FailingRedisCacheService(),
            new TestAiRecommendationClient());

        var result = await service.CompareAndSaveAsync(
            new QuoteCompareRequest
            {
                OriginPincode = "110001",
                DestinationPincode = "560001",
                ActualWeightKg = 8m,
                LengthCm = 40m,
                WidthCm = 30m,
                HeightCm = 25m,
                Preference = "Balanced"
            },
            CancellationToken.None);

        Assert.Equal("Xpressbees", result.RecommendedCarrier);
        Assert.Equal(166.14m, result.RecommendedAmount);
        Assert.Equal(42, dbContext.QuoteRequests.Single().UserId);
    }

    private static void Seed(QuoteDbContext dbContext)
    {
        var carrier = new Carrier
        {
            Name = "Xpressbees",
            Code = "XPRESSBEES",
            ServiceType = "Surface",
            IsActive = true
        };
        dbContext.Carriers.Add(carrier);
        dbContext.ZoneMappings.AddRange(
            new ZoneMapping
            {
                Pincode = "110001",
                ZoneName = "North",
                City = "New Delhi",
                State = "Delhi",
                IsActive = true
            },
            new ZoneMapping
            {
                Pincode = "560001",
                ZoneName = "South",
                City = "Bengaluru",
                State = "Karnataka",
                IsActive = true
            });
        dbContext.CarrierRateRules.Add(new CarrierRateRule
        {
            Carrier = carrier,
            OriginZone = "North",
            DestinationZone = "South",
            BaseRate = 80m,
            PerKgRate = 6m,
            FuelSurchargePercent = 10m,
            GstPercent = 18m,
            EstimatedDeliveryDays = 5,
            IsActive = true
        });
        dbContext.SaveChanges();
    }

    private sealed class FailingRedisCacheService : IRedisCacheService
    {
        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken)
        {
            return Task.FromResult<T?>(default);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class TestCurrentUserService : ICurrentUserService
    {
        public int UserId => 42;
        public string? Email => "shipper@example.com";
        public IReadOnlyCollection<string> Roles => ["Shipper"];
        public bool IsAdmin => false;
    }

    private sealed class TestAiRecommendationClient : IAiRecommendationClient
    {
        public Task<string> GetExplanationAsync(
            string preference,
            CarrierQuoteOptionDto recommendedOption,
            IReadOnlyCollection<CarrierQuoteOptionDto> options,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(AiRecommendationClient.BuildFallbackExplanation(recommendedOption.Carrier, preference));
        }
    }
}
