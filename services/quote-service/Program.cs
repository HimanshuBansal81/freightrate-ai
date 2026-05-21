using Microsoft.EntityFrameworkCore;
using quote_service.Data;
using quote_service.Models;
using quote_service.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<QuoteDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IFreightCalculatorService, FreightCalculatorService>();
builder.Services.AddScoped<ICarrierComparisonService, CarrierComparisonService>();
builder.Services.AddScoped<IQuoteHistoryService, QuoteHistoryService>();

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

app.MapPost("/api/quotes/compare", async (
    QuoteCompareRequest request,
    IQuoteHistoryService quoteHistoryService,
    CancellationToken cancellationToken) =>
{
    try
    {
        return Results.Ok(await quoteHistoryService.CompareAndSaveAsync(request, cancellationToken));
    }
    catch (QuoteValidationException exception)
    {
        return Results.BadRequest(new
        {
            message = exception.Message,
            errors = exception.Errors
        });
    }
    catch (QuoteBusinessException exception)
    {
        return Results.UnprocessableEntity(new { message = exception.Message });
    }
});

app.MapGet("/api/quotes/history", async (
    IQuoteHistoryService quoteHistoryService,
    CancellationToken cancellationToken) =>
    Results.Ok(await quoteHistoryService.GetRecentQuotesAsync(cancellationToken)));

app.MapGet("/api/quotes/{id:int}", async (
    int id,
    IQuoteHistoryService quoteHistoryService,
    CancellationToken cancellationToken) =>
{
    var quote = await quoteHistoryService.GetQuoteAsync(id, cancellationToken);
    return quote is null ? Results.NotFound(new { message = $"Quote {id} was not found." }) : Results.Ok(quote);
});

await QuoteDatabaseInitializer.InitializeAsync(app);

app.Run();
