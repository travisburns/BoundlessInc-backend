using BoundlessEnterprises.Domain.Companies;
using BoundlessEnterprises.Domain.Employees;
using BoundlessEnterprises.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoundlessEnterprises.Infrastructure.Persistence.Configurations;

public sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees", "hr");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.LastName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Email).HasMaxLength(256).IsRequired();
        builder.Property(e => e.Phone).HasMaxLength(50);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(30);

        // Tenancy: every employee belongs to a company.
        builder.HasIndex(e => e.CompanyId);
        builder.HasIndex(e => new { e.CompanyId, e.Email }).IsUnique();

        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.Employments)
            .WithOne()
            .HasForeignKey(emp => emp.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(e => e.Employments).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class EmploymentConfiguration : IEntityTypeConfiguration<Employment>
{
    public void Configure(EntityTypeBuilder<Employment> builder)
    {
        builder.ToTable("Employments", "hr");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title).HasMaxLength(150).IsRequired();
        builder.Property(e => e.Department).HasMaxLength(150);
        builder.Property(e => e.Type).HasConversion<string>().HasMaxLength(30);

        builder.HasIndex(e => e.CompanyId);
        builder.HasIndex(e => e.EmployeeId);
    }
}
