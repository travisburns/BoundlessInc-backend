namespace BoundlessEnterprises.Domain.Companies;

/// <summary>
/// Lifecycle state of a subsidiary within the holding company portfolio.
/// Drives public visibility and whether shared services accept its records.
/// </summary>
public enum CompanyStatus
{
    /// <summary>Announced but not yet operating; shown on the portfolio as upcoming.</summary>
    ComingSoon = 0,

    /// <summary>Fully operating and participating in shared enterprise services.</summary>
    Active = 1,

    /// <summary>Temporarily paused; hidden from most public surfaces.</summary>
    Inactive = 2,

    /// <summary>Retired from the portfolio; retained for history and intelligence only.</summary>
    Archived = 3,
}
