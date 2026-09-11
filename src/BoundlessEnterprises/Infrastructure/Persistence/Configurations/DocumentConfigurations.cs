using BoundlessEnterprises.Domain.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoundlessEnterprises.Infrastructure.Persistence.Configurations;

public sealed class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents", "documents");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Title).HasMaxLength(200).IsRequired();
        builder.Property(d => d.Description).HasMaxLength(2000);
        builder.Property(d => d.Url).HasMaxLength(1000);
        builder.Property(d => d.Type).HasConversion<string>().HasMaxLength(30);

        builder.HasIndex(d => d.CompanyId);
    }
}

public sealed class DocumentAssignmentConfiguration : IEntityTypeConfiguration<DocumentAssignment>
{
    public void Configure(EntityTypeBuilder<DocumentAssignment> builder)
    {
        builder.ToTable("DocumentAssignments", "documents");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(a => a.CompanyId);
        builder.HasIndex(a => a.EmployeeId);
        builder.HasIndex(a => new { a.DocumentId, a.EmployeeId }).IsUnique();
    }
}
