using Microsoft.EntityFrameworkCore;
using quote_service.Entities;

namespace quote_service.Data;

public sealed class QuoteDbContext(DbContextOptions<QuoteDbContext> options) : DbContext(options)
{
    public DbSet<Carrier> Carriers => Set<Carrier>();
    public DbSet<ZoneMapping> ZoneMappings => Set<ZoneMapping>();
    public DbSet<CarrierRateRule> CarrierRateRules => Set<CarrierRateRule>();
    public DbSet<QuoteRequest> QuoteRequests => Set<QuoteRequest>();
    public DbSet<QuoteOption> QuoteOptions => Set<QuoteOption>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Carrier>(entity =>
        {
            entity.HasKey(carrier => carrier.Id);
            entity.Property(carrier => carrier.Name).HasMaxLength(100).IsRequired();
            entity.Property(carrier => carrier.Code).HasMaxLength(50).IsRequired();
            entity.Property(carrier => carrier.ServiceType).HasMaxLength(50).IsRequired();
            entity.Property(carrier => carrier.IsActive).IsRequired();
            entity.Property(carrier => carrier.CreatedAt).IsRequired();
            entity.HasIndex(carrier => carrier.Code).IsUnique();
        });

        modelBuilder.Entity<ZoneMapping>(entity =>
        {
            entity.HasKey(zone => zone.Id);
            entity.Property(zone => zone.Pincode).HasMaxLength(10).IsRequired();
            entity.Property(zone => zone.ZoneName).HasMaxLength(50).IsRequired();
            entity.Property(zone => zone.City).HasMaxLength(100).IsRequired();
            entity.Property(zone => zone.State).HasMaxLength(100).IsRequired();
            entity.Property(zone => zone.IsActive).IsRequired();
            entity.HasIndex(zone => zone.Pincode).IsUnique();
        });

        modelBuilder.Entity<CarrierRateRule>(entity =>
        {
            entity.HasKey(rule => rule.Id);
            entity.Property(rule => rule.OriginZone).HasMaxLength(50).IsRequired();
            entity.Property(rule => rule.DestinationZone).HasMaxLength(50).IsRequired();
            entity.Property(rule => rule.BaseRate).HasPrecision(12, 2);
            entity.Property(rule => rule.PerKgRate).HasPrecision(12, 2);
            entity.Property(rule => rule.FuelSurchargePercent).HasPrecision(5, 2);
            entity.Property(rule => rule.GstPercent).HasPrecision(5, 2);
            entity.Property(rule => rule.EstimatedDeliveryDays).IsRequired();
            entity.Property(rule => rule.IsActive).IsRequired();
            entity.Property(rule => rule.CreatedAt).IsRequired();
            entity.HasIndex(rule => new { rule.CarrierId, rule.OriginZone, rule.DestinationZone }).IsUnique();

            entity.HasOne(rule => rule.Carrier)
                .WithMany(carrier => carrier.RateRules)
                .HasForeignKey(rule => rule.CarrierId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QuoteRequest>(entity =>
        {
            entity.HasKey(request => request.Id);
            entity.Property(request => request.OriginPincode).HasMaxLength(10).IsRequired();
            entity.Property(request => request.DestinationPincode).HasMaxLength(10).IsRequired();
            entity.Property(request => request.OriginZone).HasMaxLength(50).IsRequired();
            entity.Property(request => request.DestinationZone).HasMaxLength(50).IsRequired();
            entity.Property(request => request.ActualWeightKg).HasPrecision(10, 2);
            entity.Property(request => request.VolumetricWeightKg).HasPrecision(10, 2);
            entity.Property(request => request.ChargeableWeightKg).HasPrecision(10, 2);
            entity.Property(request => request.LengthCm).HasPrecision(10, 2);
            entity.Property(request => request.WidthCm).HasPrecision(10, 2);
            entity.Property(request => request.HeightCm).HasPrecision(10, 2);
            entity.Property(request => request.Preference).HasMaxLength(50).IsRequired();
            entity.Property(request => request.RecommendedCarrier).HasMaxLength(100);
            entity.Property(request => request.RecommendedAmount).HasPrecision(12, 2);
            entity.Property(request => request.AiExplanation);
            entity.Property(request => request.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<QuoteOption>(entity =>
        {
            entity.HasKey(option => option.Id);
            entity.Property(option => option.CarrierName).HasMaxLength(100).IsRequired();
            entity.Property(option => option.ServiceType).HasMaxLength(50).IsRequired();
            entity.Property(option => option.BaseFreight).HasPrecision(12, 2);
            entity.Property(option => option.FuelSurcharge).HasPrecision(12, 2);
            entity.Property(option => option.Gst).HasPrecision(12, 2);
            entity.Property(option => option.TotalAmount).HasPrecision(12, 2);
            entity.Property(option => option.EstimatedDeliveryDays).IsRequired();

            entity.HasOne(option => option.QuoteRequest)
                .WithMany(request => request.QuoteOptions)
                .HasForeignKey(option => option.QuoteRequestId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
