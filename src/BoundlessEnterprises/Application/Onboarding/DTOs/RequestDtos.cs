namespace BoundlessEnterprises.Application.Onboarding.DTOs;

/// <summary>Confirmation returned to a prospective hire after they submit a request.</summary>
public record RequestSubmittedDto
{
    public Guid Id { get; init; }
    public string CompanyName { get; init; } = string.Empty;
}

/// <summary>An onboarding request as an admin reviews it in the portal.</summary>
public record OnboardingRequestDto
{
    public Guid Id { get; init; }
    public Guid CompanyId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? DesiredRole { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
}
