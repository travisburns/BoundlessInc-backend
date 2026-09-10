namespace BoundlessEnterprises.Application.Common.Interfaces;

/// <summary>
/// Provides information about the caller for the current request, resolved from
/// the authentication token. Used for auditing and authorization decisions.
/// </summary>
public interface ICurrentUser
{
    Guid? UserId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }

    /// <summary>Company ids the caller is a member of.</summary>
    IReadOnlyCollection<Guid> CompanyIds { get; }
}
