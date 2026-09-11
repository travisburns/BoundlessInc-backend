using BoundlessEnterprises.Domain.Common;

namespace BoundlessEnterprises.Domain.Work;

public enum RingHolderInvitationStatus
{
    Pending = 0,
    Accepted = 1,
    Revoked = 2,
}

/// <summary>
/// An invitation for a person to become the holder of a ring. They redeem the
/// code publicly, set a password (creating their login), and become the ring's
/// holder — the ring's own onboarding.
/// </summary>
public class RingHolderInvitation : AuditableEntity
{
    private RingHolderInvitation() { }

    private RingHolderInvitation(Guid ringId, string firstName, string lastName, string email, string codeHash, DateTime expiresAtUtc)
    {
        RingId = ringId;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        CodeHash = codeHash;
        ExpiresAtUtc = expiresAtUtc;
        Status = RingHolderInvitationStatus.Pending;
    }

    public Guid RingId { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string CodeHash { get; private set; } = string.Empty;
    public RingHolderInvitationStatus Status { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public Guid? AcceptedUserId { get; private set; }

    public static RingHolderInvitation Create(Guid ringId, string firstName, string lastName, string email, string codeHash, DateTime expiresAtUtc)
    {
        if (ringId == Guid.Empty) throw new ArgumentException("RingId is required.", nameof(ringId));
        if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("First name is required.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Last name is required.", nameof(lastName));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(email));

        return new RingHolderInvitation(ringId, firstName.Trim(), lastName.Trim(),
            email.Trim().ToLowerInvariant(), codeHash, expiresAtUtc);
    }

    public bool IsRedeemable(DateTime nowUtc) =>
        Status == RingHolderInvitationStatus.Pending && ExpiresAtUtc > nowUtc;

    public void Accept(Guid userId)
    {
        AcceptedUserId = userId;
        Status = RingHolderInvitationStatus.Accepted;
    }

    public void Revoke() => Status = RingHolderInvitationStatus.Revoked;
}
