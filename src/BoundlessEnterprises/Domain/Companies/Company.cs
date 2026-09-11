using BoundlessEnterprises.Domain.Common;

namespace BoundlessEnterprises.Domain.Companies;

/// <summary>
/// A company in the Boundless Enterprises portfolio — the holding company itself
/// or one of its subsidiaries. This is the tenant boundary of the platform: every
/// shared record that belongs to a company carries its <see cref="Entity.Id"/> as
/// the BusinessId. The central database stores only parent-level facts about a
/// company, never a subsidiary's full operating data.
/// </summary>
public class Company : AuditableEntity
{
    // EF Core needs a parameterless constructor.
    private Company() { }

    private Company(
        string name,
        string slug,
        string code,
        CompanyType type)
    {
        Name = name;
        Slug = slug;
        Code = code;
        Type = type;
        Status = CompanyStatus.ComingSoon;
    }

    /// <summary>Display name, e.g. "Firefin".</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>URL-safe public routing key, e.g. "firefin" -> /companies/firefin.</summary>
    public string Slug { get; private set; } = string.Empty;

    /// <summary>Stable short business code used in intelligence events, e.g. "FIRE-001".</summary>
    public string Code { get; private set; } = string.Empty;

    public CompanyType Type { get; private set; }
    public CompanyStatus Status { get; private set; }

    /// <summary>Short one-line positioning statement for cards and headers.</summary>
    public string? Tagline { get; private set; }

    /// <summary>Longer public description shown on the company page.</summary>
    public string? Description { get; private set; }

    /// <summary>Free-form industry label, e.g. "Food / Hospitality / Consumer Products".</summary>
    public string? Sector { get; private set; }

    /// <summary>Path or URL to the company logo asset.</summary>
    public string? LogoUrl { get; private set; }

    /// <summary>Primary brand colour (hex) used to theme the company surfaces.</summary>
    public string? AccentColor { get; private set; }

    /// <summary>External site to "Visit Company".</summary>
    public string? WebsiteUrl { get; private set; }

    /// <summary>Primary owned domain, e.g. "firefin.com".</summary>
    public string? Domain { get; private set; }

    public string? ContactEmail { get; private set; }
    public string? ContactPhone { get; private set; }

    /// <summary>Whether the operating company exposes an employee login path.</summary>
    public bool SupportsEmployeeLogin { get; private set; }

    /// <summary>Whether the shared payment experience is enabled for this company.</summary>
    public bool SupportsPayments { get; private set; }

    /// <summary>Ordering weight for portfolio listings (ascending).</summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// Creates a new company in the portfolio. Starts in <see cref="CompanyStatus.ComingSoon"/>.
    /// </summary>
    public static Company Create(string name, string slug, string code, CompanyType type)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Company name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Company slug is required.", nameof(slug));
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Company code is required.", nameof(code));

        return new Company(name.Trim(), slug.Trim().ToLowerInvariant(), code.Trim().ToUpperInvariant(), type);
    }

    public void UpdateProfile(
        string name,
        string? tagline,
        string? description,
        string? sector,
        string? logoUrl,
        string? accentColor,
        string? websiteUrl,
        string? domain,
        string? contactEmail,
        string? contactPhone)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Company name is required.", nameof(name));

        Name = name.Trim();
        Tagline = tagline;
        Description = description;
        Sector = sector;
        LogoUrl = logoUrl;
        AccentColor = accentColor;
        WebsiteUrl = websiteUrl;
        Domain = domain;
        ContactEmail = contactEmail;
        ContactPhone = contactPhone;
    }

    public void ConfigureCapabilities(bool supportsEmployeeLogin, bool supportsPayments, int sortOrder)
    {
        SupportsEmployeeLogin = supportsEmployeeLogin;
        SupportsPayments = supportsPayments;
        SortOrder = sortOrder;
    }

    public void Activate() => Status = CompanyStatus.Active;
    public void Deactivate() => Status = CompanyStatus.Inactive;
    public void Archive() => Status = CompanyStatus.Archived;
    public void MarkComingSoon() => Status = CompanyStatus.ComingSoon;

    /// <summary>Whether the company should appear on the public portfolio.</summary>
    public bool IsPubliclyVisible => Status is CompanyStatus.Active or CompanyStatus.ComingSoon;
}
