using BoundlessEnterprises.Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoundlessEnterprises.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers", "billing");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Email).HasMaxLength(256).IsRequired();
        builder.Property(c => c.ExternalId).HasMaxLength(100);
        builder.HasIndex(c => c.CompanyId);
    }
}

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices", "billing");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Number).HasMaxLength(50).IsRequired();
        builder.Property(i => i.Currency).HasMaxLength(3).IsRequired();
        builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.ExternalId).HasMaxLength(100);

        builder.HasIndex(i => i.CompanyId);
        builder.HasIndex(i => new { i.CompanyId, i.Number }).IsUnique();

        builder.HasMany(i => i.Items)
            .WithOne()
            .HasForeignKey(item => item.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(i => i.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.ToTable("InvoiceItems", "billing");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Description).HasMaxLength(300).IsRequired();
        builder.Property(i => i.UnitAmount).HasPrecision(18, 2);
    }
}

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments", "billing");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Amount).HasPrecision(18, 2);
        builder.Property(p => p.Currency).HasMaxLength(3).IsRequired();
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.ExternalId).HasMaxLength(100);
        builder.Property(p => p.Method).HasMaxLength(50);
        builder.HasIndex(p => p.CompanyId);
        builder.HasIndex(p => p.InvoiceId);
    }
}

public sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions", "billing");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.PlanName).HasMaxLength(150).IsRequired();
        builder.Property(s => s.Tier).HasMaxLength(80);
        builder.Property(s => s.Amount).HasPrecision(18, 2);
        builder.Property(s => s.Currency).HasMaxLength(3).IsRequired();
        builder.Property(s => s.Interval).HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.ExternalId).HasMaxLength(100);
        builder.HasIndex(s => s.CompanyId);
    }
}
