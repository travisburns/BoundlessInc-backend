using BoundlessEnterprises.Domain.Common;

namespace BoundlessEnterprises.Domain.Identity;

/// <summary>
/// A person with a single Boundless Enterprises account. One user can belong to
/// multiple companies with different roles via <see cref="UserCompany"/> — there
/// are never duplicate accounts per company.
/// </summary>
public class User : AuditableEntity
{
    private readonly List<UserCompany> _memberships = new();

    private User() { }

    private User(string email, string passwordHash, string firstName, string lastName)
    {
        Email = email;
        PasswordHash = passwordHash;
        FirstName = firstName;
        LastName = lastName;
        IsActive = true;
    }

    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    /// <summary>Platform-wide administrator (holding-company operator).</summary>
    public bool IsPlatformAdmin { get; private set; }

    public DateTime? LastLoginAtUtc { get; private set; }

    // Password reset
    public string? PasswordResetTokenHash { get; private set; }
    public DateTime? PasswordResetExpiresUtc { get; private set; }

    public IReadOnlyCollection<UserCompany> Memberships => _memberships.AsReadOnly();

    public string FullName => $"{FirstName} {LastName}".Trim();

    public static User Create(string email, string passwordHash, string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));

        return new User(email.Trim().ToLowerInvariant(), passwordHash, firstName.Trim(), lastName.Trim());
    }

    public void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));
        PasswordHash = passwordHash;
        PasswordResetTokenHash = null;
        PasswordResetExpiresUtc = null;
    }

    public void GrantPlatformAdmin() => IsPlatformAdmin = true;
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
    public void RecordLogin(DateTime whenUtc) => LastLoginAtUtc = whenUtc;

    public void SetPasswordResetToken(string tokenHash, DateTime expiresUtc)
    {
        PasswordResetTokenHash = tokenHash;
        PasswordResetExpiresUtc = expiresUtc;
    }

    public bool IsResetTokenValid(string tokenHash, DateTime nowUtc) =>
        PasswordResetTokenHash is not null &&
        PasswordResetExpiresUtc is not null &&
        PasswordResetExpiresUtc > nowUtc &&
        PasswordResetTokenHash == tokenHash;

    /// <summary>Adds a company membership if one does not already exist.</summary>
    public UserCompany AddMembership(Guid companyId, bool isPrimary = false)
    {
        var existing = _memberships.FirstOrDefault(m => m.CompanyId == companyId);
        if (existing is not null)
            return existing;

        var membership = UserCompany.Create(Id, companyId, isPrimary);
        _memberships.Add(membership);
        return membership;
    }
}
