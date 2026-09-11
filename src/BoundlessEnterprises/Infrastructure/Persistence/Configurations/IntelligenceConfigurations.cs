using BoundlessEnterprises.Domain.Integrations;
using BoundlessEnterprises.Domain.Intelligence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoundlessEnterprises.Infrastructure.Persistence.Configurations;

public sealed class IntegrationConfiguration : IEntityTypeConfiguration<Integration>
{
    public void Configure(EntityTypeBuilder<Integration> builder)
    {
        builder.ToTable("Integrations", "integration");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Name).HasMaxLength(150).IsRequired();
        builder.Property(i => i.ApiKeyHash).HasMaxLength(100).IsRequired();
        builder.Property(i => i.ApiKeyPrefix).HasMaxLength(40).IsRequired();
        builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(i => i.CompanyId);
        builder.HasIndex(i => i.ApiKeyHash).IsUnique();
    }
}

public sealed class BusinessEventConfiguration : IEntityTypeConfiguration<BusinessEvent>
{
    public void Configure(EntityTypeBuilder<BusinessEvent> builder)
    {
        builder.ToTable("BusinessEvents", "intelligence");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.EventType).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Revenue).HasPrecision(18, 2);
        builder.Property(e => e.Currency).HasMaxLength(3).IsRequired();
        builder.Property(e => e.ExternalRef).HasMaxLength(200);

        builder.HasIndex(e => e.CompanyId);
        builder.HasIndex(e => e.OccurredAtUtc);
        builder.HasIndex(e => new { e.CompanyId, e.EventType });
    }
}

public sealed class DailySnapshotConfiguration : IEntityTypeConfiguration<DailySnapshot>
{
    public void Configure(EntityTypeBuilder<DailySnapshot> builder)
    {
        builder.ToTable("DailySnapshots", "intelligence");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Revenue).HasPrecision(18, 2);
        builder.HasIndex(s => new { s.CompanyId, s.Date }).IsUnique();
    }
}
