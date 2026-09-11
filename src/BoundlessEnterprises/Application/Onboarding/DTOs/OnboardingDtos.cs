using BoundlessEnterprises.Domain.Onboarding;

namespace BoundlessEnterprises.Application.Onboarding.DTOs;

public record OnboardingStepDto
{
    public int Order { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsRequired { get; init; }
    public string Kind { get; init; } = nameof(OnboardingStepKind.Generic);
}

public record OnboardingTemplateDto
{
    public Guid Id { get; init; }
    public Guid CompanyId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public IReadOnlyList<OnboardingStepDto> Steps { get; init; } = Array.Empty<OnboardingStepDto>();

    public static OnboardingTemplateDto FromEntity(OnboardingTemplate t) => new()
    {
        Id = t.Id,
        CompanyId = t.CompanyId,
        Name = t.Name,
        Description = t.Description,
        IsActive = t.IsActive,
        Steps = t.Steps.Select(s => new OnboardingStepDto
        {
            Order = s.Order,
            Name = s.Name,
            Description = s.Description,
            IsRequired = s.IsRequired,
            Kind = s.Kind.ToString(),
        }).ToList(),
    };
}

public record OnboardingEmployeeStepDto
{
    public Guid Id { get; init; }
    public int Order { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsRequired { get; init; }
    public bool IsCompleted { get; init; }
    public DateTime? CompletedAtUtc { get; init; }
    public string Kind { get; init; } = nameof(OnboardingStepKind.Generic);
    /// <summary>The data the hire submitted for this step, as a JSON string (null until submitted).</summary>
    public string? ResponseJson { get; init; }
}

public record OnboardingProcessDto
{
    public Guid Id { get; init; }
    public Guid CompanyId { get; init; }
    public Guid EmployeeId { get; init; }
    public string TemplateName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int TotalSteps { get; init; }
    public int CompletedSteps { get; init; }
    public DateTime StartedAtUtc { get; init; }
    public DateTime? CompletedAtUtc { get; init; }
    public IReadOnlyList<OnboardingEmployeeStepDto> Steps { get; init; } = Array.Empty<OnboardingEmployeeStepDto>();

    public static OnboardingProcessDto FromEntity(OnboardingProcess p) => new()
    {
        Id = p.Id,
        CompanyId = p.CompanyId,
        EmployeeId = p.EmployeeId,
        TemplateName = p.TemplateName,
        Status = p.Status.ToString(),
        TotalSteps = p.TotalSteps,
        CompletedSteps = p.CompletedSteps,
        StartedAtUtc = p.StartedAtUtc,
        CompletedAtUtc = p.CompletedAtUtc,
        Steps = p.Steps.Select(s => new OnboardingEmployeeStepDto
        {
            Id = s.Id,
            Order = s.Order,
            Name = s.Name,
            IsRequired = s.IsRequired,
            IsCompleted = s.IsCompleted,
            CompletedAtUtc = s.CompletedAtUtc,
            Kind = s.Kind.ToString(),
            ResponseJson = s.ResponseJson,
        }).ToList(),
    };
}
