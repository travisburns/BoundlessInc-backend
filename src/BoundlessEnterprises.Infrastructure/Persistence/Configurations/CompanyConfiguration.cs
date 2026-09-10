using BoundlessEnterprises.Domain.Companies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoundlessEnterprises.Infrastructure.Persistence.Configurations;

public sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies", "core");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Slug).HasMaxLength(120).IsRequired();
        builder.Property(c => c.Code).HasMaxLength(20).IsRequired();
        builder.Property(c => c.Tagline).HasMaxLength(300);
        builder.Property(c => c.Description).HasMaxLength(4000);
        builder.Property(c => c.Sector).HasMaxLength(200);
        builder.Property(c => c.LogoUrl).HasMaxLength(500);
        builder.Property(c => c.AccentColor).HasMaxLength(20);
        builder.Property(c => c.WebsiteUrl).HasMaxLength(500);
        builder.Property(c => c.Domain).HasMaxLength(200);
        builder.Property(c => c.ContactEmail).HasMaxLength(200);
        builder.Property(c => c.ContactPhone).HasMaxLength(50);

        builder.Property(c => c.Type).HasConversion<string>().HasMaxLength(30);
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(30);

        builder.HasIndex(c => c.Slug).IsUnique();
        builder.HasIndex(c => c.Code).IsUnique();
    }
}
