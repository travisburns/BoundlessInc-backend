namespace BoundlessEnterprises.Application.Onboarding.DTOs;

/// <summary>Returned to a manager when an invitation is created — includes the raw code once.</summary>
public record InvitationCreatedDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string TemplateName { get; init; } = string.Empty;
    public DateTime ExpiresAtUtc { get; init; }
}

/// <summary>Public view of an invitation for the self-serve wizard.</summary>
public record InviteDetailDto
{
    public string Status { get; init; } = string.Empty;
    public string CompanyName { get; init; } = string.Empty;
    public string TemplateName { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    /// <summary>Present once the invitation has been started.</summary>
    public OnboardingProcessDto? Process { get; init; }
}
