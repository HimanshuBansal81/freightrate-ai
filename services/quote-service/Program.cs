using Microsoft.EntityFrameworkCore;
using quote_service.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<QuoteDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "quote-service" }))
.WithName("HealthCheck")
.WithOpenApi();

app.MapGet("/api/carriers", async (QuoteDbContext dbContext) =>
    await dbContext.Carriers
        .Where(carrier => carrier.IsActive)
        .OrderBy(carrier => carrier.Name)
        .Select(carrier => new
        {
            carrier.Id,
            carrier.Name,
            carrier.Code,
            carrier.ServiceType,
            carrier.IsActive
        })
        .ToListAsync());

app.MapGet("/api/zones", async (QuoteDbContext dbContext) =>
    await dbContext.ZoneMappings
        .Where(zone => zone.IsActive)
        .OrderBy(zone => zone.Pincode)
        .Select(zone => new
        {
            zone.Id,
            zone.Pincode,
            zone.ZoneName,
            zone.City,
            zone.State,
            zone.IsActive
        })
        .ToListAsync());

app.MapGet("/api/admin/rate-rules", async (QuoteDbContext dbContext) =>
    await dbContext.CarrierRateRules
        .Where(rule => rule.IsActive)
        .OrderBy(rule => rule.Carrier.Name)
        .ThenBy(rule => rule.OriginZone)
        .ThenBy(rule => rule.DestinationZone)
        .Select(rule => new
        {
            rule.Id,
            Carrier = new
            {
                rule.Carrier.Id,
                rule.Carrier.Name,
                rule.Carrier.Code,
                rule.Carrier.ServiceType
            },
            rule.OriginZone,
            rule.DestinationZone,
            rule.BaseRate,
            rule.PerKgRate,
            rule.FuelSurchargePercent,
            rule.GstPercent,
            rule.EstimatedDeliveryDays,
            rule.IsActive
        })
        .ToListAsync());

await QuoteDatabaseInitializer.InitializeAsync(app);

app.Run();
