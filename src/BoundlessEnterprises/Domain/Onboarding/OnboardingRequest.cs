using BoundlessEnterprises.Domain.Common;

namespace BoundlessEnterprises.Domain.Onboarding;

public enum OnboardingRequestStatus
{
    Pending = 0,
    Approved = 1,
    Declined = 2,
}

/// <summary>
/// A prospective hire's public request to onboard into a company. It carries no
/// code — an admin reviews the request and, on approval, an
/// <see cref="OnboardingInvitation"/> is issued and linked here. This keeps the
/// company in control of who joins while letting people ask to start.
/// </summary>
public class OnboardingRequest : AuditableEntity
{
    private OnboardingRequest() { }

    private OnboardingRequest(Guid companyId, string firstName, string lastName, string email, string? desiredRole)
    {
        CompanyId = companyId;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        DesiredRole = desiredRole;
        Status = OnboardingRequestStatus.Pending;
    }

    public Guid CompanyId { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? DesiredRole { get; private set; }

    public OnboardingRequestStatus Status { get; private set; }
    public DateTime? ReviewedAtUtc { get; private set; }

    /// <summary>The invitation issued when this request was approved.</summary>
    public Guid? InvitationId { get; private set; }

    public static OnboardingRequest Create(Guid companyId, string firstName, string lastName, string email, string? desiredRole)
    {
        if (companyId == Guid.Empty) throw new ArgumentException("CompanyId is required.", nameof(companyId));
        if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("First name is required.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Last name is required.", nameof(lastName));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(email));

        return new OnboardingRequest(companyId, firstName.Trim(), lastName.Trim(),
            email.Trim().ToLowerInvariant(), string.IsNullOrWhiteSpace(desiredRole) ? null : desiredRole.Trim());
    }

    public void Approve(Guid invitationId, DateTime whenUtc)
    {
        InvitationId = invitationId;
        Status = OnboardingRequestStatus.Approved;
        ReviewedAtUtc = whenUtc;
    }

    public void Decline(DateTime whenUtc)
    {
        Status = OnboardingRequestStatus.Declined;
        ReviewedAtUtc = whenUtc;
    }
}
