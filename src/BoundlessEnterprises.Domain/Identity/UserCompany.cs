using BoundlessEnterprises.Domain.Common;

namespace BoundlessEnterprises.Domain.Identity;

/// <summary>
/// A user's membership in a company. This is where tenancy lives for identity:
/// the same user can hold different roles at different companies, and "no access"
/// is simply the absence of a membership. Roles are assigned per membership.
/// </summary>
public class UserCompany : AuditableEntity
{
    private readonly List<UserRole> _roles = new();

    private UserCompany() { }

    private UserCompany(Guid userId, Guid companyId, bool isPrimary)
    {
        UserId = userId;
        CompanyId = companyId;
        IsPrimary = isPrimary;
        JoinedAtUtc = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }
    public Guid CompanyId { get; private set; }

    /// <summary>The user's home company for default context after login.</summary>
    public bool IsPrimary { get; private set; }

    public DateTime JoinedAtUtc { get; private set; }

    public IReadOnlyCollection<UserRole> Roles => _roles.AsReadOnly();

    public static UserCompany Create(Guid userId, Guid companyId, bool isPrimary = false) =>
        new(userId, companyId, isPrimary);

    public void MakePrimary() => IsPrimary = true;
    public void ClearPrimary() => IsPrimary = false;

    public void AssignRole(Guid roleId)
    {
        if (_roles.All(r => r.RoleId != roleId))
            _roles.Add(new UserRole(Id, roleId));
    }

    public void RemoveRole(Guid roleId)
    {
        var existing = _roles.FirstOrDefault(r => r.RoleId == roleId);
        if (existing is not null)
            _roles.Remove(existing);
    }
}
