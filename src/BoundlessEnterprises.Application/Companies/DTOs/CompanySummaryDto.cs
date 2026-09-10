using BoundlessEnterprises.Domain.Companies;

namespace BoundlessEnterprises.Application.Companies.DTOs;

/// <summary>
/// Public-facing projection of a company for portfolio listings and cards.
/// Contains only parent-level, non-sensitive fields.
/// </summary>
public record CompanySummaryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? Tagline { get; init; }
    public string? Sector { get; init; }
    public string? LogoUrl { get; init; }
    public string? AccentColor { get; init; }
    public int SortOrder { get; init; }

    public static CompanySummaryDto FromEntity(Company c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Slug = c.Slug,
        Code = c.Code,
        Type = c.Type.ToString(),
        Status = c.Status.ToString(),
        Tagline = c.Tagline,
        Sector = c.Sector,
        LogoUrl = c.LogoUrl,
        AccentColor = c.AccentColor,
        SortOrder = c.SortOrder,
    };
}
