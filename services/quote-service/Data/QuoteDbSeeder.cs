using Microsoft.EntityFrameworkCore;
using quote_service.Entities;

namespace quote_service.Data;

public static class QuoteDbSeeder
{
    public static async Task SeedAsync(QuoteDbContext dbContext)
    {
        await SeedCarriersAsync(dbContext);
        await SeedZonesAsync(dbContext);
        await SeedRateRulesAsync(dbContext);
    }

    private static async Task SeedCarriersAsync(QuoteDbContext dbContext)
    {
        var carriers = new[]
        {
            new Carrier { Name = "Delhivery", Code = "DELHIVERY", ServiceType = "Surface", IsActive = true },
            new Carrier { Name = "BlueDart", Code = "BLUEDART", ServiceType = "Express", IsActive = true },
            new Carrier { Name = "Xpressbees", Code = "XPRESSBEES", ServiceType = "Surface", IsActive = true }
        };

        foreach (var carrier in carriers)
        {
            if (!await dbContext.Carriers.AnyAsync(existing => existing.Code == carrier.Code))
            {
                dbContext.Carriers.Add(carrier);
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedZonesAsync(QuoteDbContext dbContext)
    {
        var zones = new[]
        {
            new ZoneMapping { Pincode = "110001", ZoneName = "North", City = "New Delhi", State = "Delhi", IsActive = true },
            new ZoneMapping { Pincode = "122001", ZoneName = "North", City = "Gurugram", State = "Haryana", IsActive = true },
            new ZoneMapping { Pincode = "400001", ZoneName = "West", City = "Mumbai", State = "Maharashtra", IsActive = true },
            new ZoneMapping { Pincode = "560001", ZoneName = "South", City = "Bengaluru", State = "Karnataka", IsActive = true },
            new ZoneMapping { Pincode = "700001", ZoneName = "East", City = "Kolkata", State = "West Bengal", IsActive = true }
        };

        foreach (var zone in zones)
        {
            if (!await dbContext.ZoneMappings.AnyAsync(existing => existing.Pincode == zone.Pincode))
            {
                dbContext.ZoneMappings.Add(zone);
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedRateRulesAsync(QuoteDbContext dbContext)
    {
        var carrierRates = new[]
        {
            new RateRuleSeed("DELHIVERY", 90m, 7m, 12m, 18m, 4),
            new RateRuleSeed("BLUEDART", 130m, 10m, 15m, 18m, 2),
            new RateRuleSeed("XPRESSBEES", 80m, 6m, 10m, 18m, 5)
        };

        foreach (var carrierRate in carrierRates)
        {
            var carrier = await dbContext.Carriers.SingleAsync(existing => existing.Code == carrierRate.CarrierCode);

            await AddRateRuleIfMissingAsync(dbContext, carrier, "North", "South", carrierRate);
            await AddRateRuleIfMissingAsync(dbContext, carrier, "South", "North", carrierRate);
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task AddRateRuleIfMissingAsync(
        QuoteDbContext dbContext,
        Carrier carrier,
        string originZone,
        string destinationZone,
        RateRuleSeed seed)
    {
        var exists = await dbContext.CarrierRateRules.AnyAsync(rule =>
            rule.CarrierId == carrier.Id
            && rule.OriginZone == originZone
            && rule.DestinationZone == destinationZone);

        if (exists)
        {
            return;
        }

        dbContext.CarrierRateRules.Add(new CarrierRateRule
        {
            CarrierId = carrier.Id,
            OriginZone = originZone,
            DestinationZone = destinationZone,
            BaseRate = seed.BaseRate,
            PerKgRate = seed.PerKgRate,
            FuelSurchargePercent = seed.FuelSurchargePercent,
            GstPercent = seed.GstPercent,
            EstimatedDeliveryDays = seed.EstimatedDeliveryDays,
            IsActive = true
        });
    }

    private sealed record RateRuleSeed(
        string CarrierCode,
        decimal BaseRate,
        decimal PerKgRate,
        decimal FuelSurchargePercent,
        decimal GstPercent,
        int EstimatedDeliveryDays);
}
