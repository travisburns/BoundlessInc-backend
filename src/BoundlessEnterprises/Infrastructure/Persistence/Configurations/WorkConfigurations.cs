using BoundlessEnterprises.Domain.Work;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoundlessEnterprises.Infrastructure.Persistence.Configurations;

public sealed class RingConfiguration : IEntityTypeConfiguration<Ring>
{
    public void Configure(EntityTypeBuilder<Ring> builder)
    {
        builder.ToTable("Rings", "work");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Domain).HasConversion<string>().HasMaxLength(30);
        builder.HasIndex(r => r.Domain).IsUnique();

        builder.Property(r => r.Name).HasMaxLength(120).IsRequired();
        builder.Property(r => r.Disciplines).HasMaxLength(300);
        builder.Property(r => r.HolderName).HasMaxLength(160).IsRequired();
        builder.Property(r => r.HeroTitle).HasMaxLength(200);
        builder.Property(r => r.HeroSubtitle).HasMaxLength(200);
        builder.Property(r => r.Focus).HasMaxLength(400);
        builder.Property(r => r.Motto).HasMaxLength(200);
        builder.Property(r => r.AccentColor).HasMaxLength(20);
        builder.Property(r => r.CodePrefix).HasMaxLength(8).IsRequired();

        // Helpful-resources list stored as JSON.
        builder.OwnsMany(r => r.Resources, b => b.ToJson());
    }
}

public sealed class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> builder)
    {
        builder.ToTable("Assignments", "work");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Code).HasMaxLength(20).IsRequired();
        builder.HasIndex(a => a.Code).IsUnique();
        builder.HasIndex(a => a.RingId);

        builder.Property(a => a.Title).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Summary).HasMaxLength(500);
        builder.Property(a => a.Objective).HasMaxLength(2000);
        builder.Property(a => a.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.Priority).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.AssigneeName).HasMaxLength(160);
        builder.Property(a => a.IssuedBy).HasMaxLength(160);
        builder.Property(a => a.Deliverable).HasMaxLength(2000);
        builder.Property(a => a.Dependencies).HasMaxLength(2000);
        builder.Property(a => a.Blockers).HasMaxLength(2000);
        builder.Property(a => a.NextStep).HasMaxLength(300);
        builder.Property(a => a.ReviewedBy).HasMaxLength(160);
        builder.Property(a => a.DomainDataJson);

        // Primitive string collections → JSON columns (EF Core 8).
        builder.PrimitiveCollection(a => a.AcceptanceCriteria);
        builder.PrimitiveCollection(a => a.References);
        builder.PrimitiveCollection(a => a.Tags);

        builder.HasMany(a => a.Updates)
            .WithOne()
            .HasForeignKey(u => u.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(a => a.Updates).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class AssignmentUpdateConfiguration : IEntityTypeConfiguration<AssignmentUpdate>
{
    public void Configure(EntityTypeBuilder<AssignmentUpdate> builder)
    {
        builder.ToTable("AssignmentUpdates", "work");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Author).HasMaxLength(160).IsRequired();
        builder.Property(u => u.Body).HasMaxLength(4000).IsRequired();
    }
}

public sealed class RingEventConfiguration : IEntityTypeConfiguration<RingEvent>
{
    public void Configure(EntityTypeBuilder<RingEvent> builder)
    {
        builder.ToTable("RingEvents", "work");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Title).HasMaxLength(200).IsRequired();
        builder.Property(e => e.TimeLabel).HasMaxLength(60);
        builder.Property(e => e.Detail).HasMaxLength(200);
        builder.HasIndex(e => e.RingId);
    }
}

public sealed class RingActivityConfiguration : IEntityTypeConfiguration<RingActivity>
{
    public void Configure(EntityTypeBuilder<RingActivity> builder)
    {
        builder.ToTable("RingActivities", "work");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Kind).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.Text).HasMaxLength(500).IsRequired();
        builder.HasIndex(a => a.RingId);
    }
}
