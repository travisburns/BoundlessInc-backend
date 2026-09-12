using BoundlessEnterprises.Domain.Onboarding.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoundlessEnterprises.Infrastructure.Persistence.Configurations;

public sealed class CoreOnboardingProgressConfiguration : IEntityTypeConfiguration<CoreOnboardingProgress>
{
    public void Configure(EntityTypeBuilder<CoreOnboardingProgress> builder)
    {
        builder.ToTable("CoreOnboardingProgress", "onboarding");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.CompletedName).HasMaxLength(160);
        builder.Property(p => p.CompletedSignature).HasMaxLength(160);

        // One journey through the document per person.
        builder.HasIndex(p => p.UserId).IsUnique();

        builder.HasMany(p => p.Signatures)
            .WithOne()
            .HasForeignKey(s => s.ProgressId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(CoreOnboardingProgress.Signatures))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class CoreOnboardingStageSignatureConfiguration : IEntityTypeConfiguration<CoreOnboardingStageSignature>
{
    public void Configure(EntityTypeBuilder<CoreOnboardingStageSignature> builder)
    {
        builder.ToTable("CoreOnboardingStageSignatures", "onboarding");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.StageKey).HasMaxLength(40).IsRequired();
        builder.Property(s => s.StageTitle).HasMaxLength(160).IsRequired();
        builder.Property(s => s.TypedName).HasMaxLength(160).IsRequired();
        builder.Property(s => s.Signature).HasMaxLength(160).IsRequired();

        // A person signs each stage at most once (re-signing updates the row).
        builder.HasIndex(s => new { s.ProgressId, s.StageKey }).IsUnique();
    }
}
