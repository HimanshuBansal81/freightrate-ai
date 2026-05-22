using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using quote_service.Data;
using quote_service.Models;
using quote_service.Services;
using quote_service.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter a valid JWT bearer token."
    });
    options.OperationFilter<AuthorizeOperationFilter>();
});
builder.Services.AddDbContext<QuoteDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret is required.");
var jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = jwtKey,
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ShipperOrAdmin", policy =>
        policy.RequireAuthenticatedUser().RequireRole("Shipper", "Admin"));
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireAuthenticatedUser().RequireRole("Admin"));
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IFreightCalculatorService, FreightCalculatorService>();
builder.Services.AddScoped<ICarrierComparisonService, CarrierComparisonService>();
builder.Services.AddScoped<IQuoteHistoryService, QuoteHistoryService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();
builder.Services.AddHttpClient<IAiRecommendationClient, AiRecommendationClient>((serviceProvider, client) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var baseUrl = configuration["AiService:BaseUrl"] ?? "http://ai-recommendation-service:8000";
    client.BaseAddress = new Uri(baseUrl);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "quote-service" }))
.WithName("HealthCheck")
.WithOpenApi();

app.MapGet("/api/carriers", async Task<IReadOnlyCollection<ActiveCarrierDto>> (
    QuoteDbContext dbContext,
    IRedisCacheService cache,
    CancellationToken cancellationToken) =>
{
    const string cacheKey = "carriers:active";
    var cachedCarriers = await cache.GetAsync<IReadOnlyCollection<ActiveCarrierDto>>(cacheKey, cancellationToken);
    if (cachedCarriers is not null)
    {
        return cachedCarriers;
    }

    var carriers = await dbContext.Carriers
        .Where(carrier => carrier.IsActive)
        .OrderBy(carrier => carrier.Name)
        .Select(carrier => new ActiveCarrierDto
        {
            Id = carrier.Id,
            Name = carrier.Name,
            Code = carrier.Code,
            ServiceType = carrier.ServiceType,
            IsActive = carrier.IsActive
        })
        .ToListAsync(cancellationToken);

    await cache.SetAsync(cacheKey, carriers, TimeSpan.FromHours(1), cancellationToken);
    return carriers;
});

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
        .ToListAsync())
    .RequireAuthorization("AdminOnly");

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
})
.RequireAuthorization("ShipperOrAdmin");

app.MapGet("/api/quotes/history", async (
    IQuoteHistoryService quoteHistoryService,
    CancellationToken cancellationToken) =>
    Results.Ok(await quoteHistoryService.GetRecentQuotesAsync(cancellationToken)))
    .RequireAuthorization("ShipperOrAdmin");

app.MapGet("/api/quotes/{id:int}", async (
    int id,
    IQuoteHistoryService quoteHistoryService,
    CancellationToken cancellationToken) =>
{
    var quote = await quoteHistoryService.GetQuoteAsync(id, cancellationToken);
    return quote is null ? Results.NotFound(new { message = $"Quote {id} was not found." }) : Results.Ok(quote);
})
.RequireAuthorization("ShipperOrAdmin");

await QuoteDatabaseInitializer.InitializeAsync(app);

app.Run();
