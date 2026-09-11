using BoundlessEnterprises.Domain.Common;

namespace BoundlessEnterprises.Domain.Onboarding;

public enum OnboardingInvitationStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Expired = 3,
    Revoked = 4,
}

/// <summary>
/// A self-serve onboarding invitation. A manager creates it for a new hire (email,
/// name, template); the hire redeems the code publicly to create their employee
/// record and start their onboarding process — no manager pre-provisioning.
/// </summary>
public class OnboardingInvitation : AuditableEntity
{
    private OnboardingInvitation() { }

    private OnboardingInvitation(
        Guid companyId, Guid templateId, string email, string firstName,
        string lastName, string title, string codeHash, DateTime expiresAtUtc)
    {
        CompanyId = companyId;
        TemplateId = templateId;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        Title = title;
        CodeHash = codeHash;
        ExpiresAtUtc = expiresAtUtc;
        Status = OnboardingInvitationStatus.Pending;
    }

    public Guid CompanyId { get; private set; }
    public Guid TemplateId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;

    /// <summary>SHA-256 hash of the invite code; the raw code is shown once.</summary>
    public string CodeHash { get; private set; } = string.Empty;

    public OnboardingInvitationStatus Status { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }

    /// <summary>Set when the hire redeems the invite and starts.</summary>
    public Guid? EmployeeId { get; private set; }
    public Guid? ProcessId { get; private set; }

    public static OnboardingInvitation Create(
        Guid companyId, Guid templateId, string email, string firstName,
        string lastName, string title, string codeHash, DateTime expiresAtUtc)
    {
        if (companyId == Guid.Empty) throw new ArgumentException("CompanyId is required.", nameof(companyId));
        if (templateId == Guid.Empty) throw new ArgumentException("TemplateId is required.", nameof(templateId));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(email));
        if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("First name is required.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Last name is required.", nameof(lastName));

        return new OnboardingInvitation(companyId, templateId, email.Trim().ToLowerInvariant(),
            firstName.Trim(), lastName.Trim(), string.IsNullOrWhiteSpace(title) ? "New Employee" : title.Trim(),
            codeHash, expiresAtUtc);
    }

    public bool IsRedeemable(DateTime nowUtc) =>
        Status is OnboardingInvitationStatus.Pending or OnboardingInvitationStatus.InProgress
        && ExpiresAtUtc > nowUtc;

    public void MarkStarted(Guid employeeId, Guid processId)
    {
        EmployeeId = employeeId;
        ProcessId = processId;
        Status = OnboardingInvitationStatus.InProgress;
    }

    public void MarkCompleted() => Status = OnboardingInvitationStatus.Completed;
    public void Revoke() => Status = OnboardingInvitationStatus.Revoked;

    public void UpdateDetails(string firstName, string lastName)
    {
        if (!string.IsNullOrWhiteSpace(firstName)) FirstName = firstName.Trim();
        if (!string.IsNullOrWhiteSpace(lastName)) LastName = lastName.Trim();
    }
}
