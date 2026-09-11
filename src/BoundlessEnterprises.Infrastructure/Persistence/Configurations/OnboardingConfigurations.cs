using BoundlessEnterprises.Domain.Onboarding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoundlessEnterprises.Infrastructure.Persistence.Configurations;

public sealed class OnboardingTemplateConfiguration : IEntityTypeConfiguration<OnboardingTemplate>
{
    public void Configure(EntityTypeBuilder<OnboardingTemplate> builder)
    {
        builder.ToTable("Templates", "onboarding");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Description).HasMaxLength(1000);
        builder.HasIndex(t => t.CompanyId);

        builder.HasMany(t => t.Steps)
            .WithOne()
            .HasForeignKey(s => s.TemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(t => t.Steps).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class OnboardingStepConfiguration : IEntityTypeConfiguration<OnboardingStep>
{
    public void Configure(EntityTypeBuilder<OnboardingStep> builder)
    {
        builder.ToTable("Steps", "onboarding");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Description).HasMaxLength(1000);
    }
}

public sealed class OnboardingProcessConfiguration : IEntityTypeConfiguration<OnboardingProcess>
{
    public void Configure(EntityTypeBuilder<OnboardingProcess> builder)
    {
        builder.ToTable("Processes", "onboarding");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.TemplateName).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(30);

        builder.HasIndex(p => p.CompanyId);
        builder.HasIndex(p => p.EmployeeId);

        builder.HasMany(p => p.Steps)
            .WithOne()
            .HasForeignKey(s => s.ProcessId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Steps).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class OnboardingEmployeeStepConfiguration : IEntityTypeConfiguration<OnboardingEmployeeStep>
{
    public void Configure(EntityTypeBuilder<OnboardingEmployeeStep> builder)
    {
        builder.ToTable("EmployeeSteps", "onboarding");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name).HasMaxLength(200).IsRequired();
    }
}

public sealed class OnboardingInvitationConfiguration : IEntityTypeConfiguration<OnboardingInvitation>
{
    public void Configure(EntityTypeBuilder<OnboardingInvitation> builder)
    {
        builder.ToTable("Invitations", "onboarding");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Email).HasMaxLength(256).IsRequired();
        builder.Property(i => i.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(i => i.LastName).HasMaxLength(100).IsRequired();
        builder.Property(i => i.Title).HasMaxLength(150).IsRequired();
        builder.Property(i => i.CodeHash).HasMaxLength(100).IsRequired();
        builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(i => i.CompanyId);
        builder.HasIndex(i => i.CodeHash).IsUnique();
    }
}
