namespace BoundlessEnterprises.Application.Identity.DTOs;

/// <summary>Returned on successful authentication.</summary>
public record AuthResultDto
{
    public string Token { get; init; } = string.Empty;
    public DateTime ExpiresAtUtc { get; init; }
    public UserProfileDto User { get; init; } = new();
}
