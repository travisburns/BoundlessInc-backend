namespace BoundlessEnterprises.Application.Identity.DTOs;

/// <summary>The authenticated user's profile and their company memberships.</summary>
public record UserProfileDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public bool IsPlatformAdmin { get; init; }
    public IReadOnlyList<MembershipDto> Companies { get; init; } = Array.Empty<MembershipDto>();
}

/// <summary>A company the user belongs to and the roles they hold there.</summary>
public record MembershipDto
{
    public Guid CompanyId { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public string CompanySlug { get; init; } = string.Empty;
    public bool IsPrimary { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
}
