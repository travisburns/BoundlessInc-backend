namespace BoundlessEnterprises.Domain.Companies;

/// <summary>
/// Broad classification of an entity inside the Boundless Enterprises structure.
/// The specific industry (Media, Food, SaaS, Software, ...) is captured separately
/// as a free-form <c>Sector</c> so the taxonomy can grow without schema churn.
/// </summary>
public enum CompanyType
{
    /// <summary>The parent holding company itself.</summary>
    Holding = 0,

    /// <summary>An established operating subsidiary.</summary>
    Subsidiary = 1,

    /// <summary>An early-stage brand or venture under incubation.</summary>
    Venture = 2,
}
