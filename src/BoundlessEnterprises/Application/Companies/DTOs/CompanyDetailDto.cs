using BoundlessEnterprises.Domain.Companies;

namespace BoundlessEnterprises.Application.Companies.DTOs;

/// <summary>
/// Full public projection of a company for its dedicated company page, including
/// the actions the page can expose (visit, employee login, payment, contact).
/// </summary>
public record CompanyDetailDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? Tagline { get; init; }
    public string? Description { get; init; }
    public string? Sector { get; init; }
    public string? LogoUrl { get; init; }
    public string? AccentColor { get; init; }
    public string? WebsiteUrl { get; init; }
    public string? Domain { get; init; }
    public string? ContactEmail { get; init; }
    public string? ContactPhone { get; init; }
    public bool SupportsEmployeeLogin { get; init; }
    public bool SupportsPayments { get; init; }
    public int SortOrder { get; init; }

    public static CompanyDetailDto FromEntity(Company c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Slug = c.Slug,
        Code = c.Code,
        Type = c.Type.ToString(),
        Status = c.Status.ToString(),
        Tagline = c.Tagline,
        Description = c.Description,
        Sector = c.Sector,
        LogoUrl = c.LogoUrl,
        AccentColor = c.AccentColor,
        WebsiteUrl = c.WebsiteUrl,
        Domain = c.Domain,
        ContactEmail = c.ContactEmail,
        ContactPhone = c.ContactPhone,
        SupportsEmployeeLogin = c.SupportsEmployeeLogin,
        SupportsPayments = c.SupportsPayments,
        SortOrder = c.SortOrder,
    };
}
