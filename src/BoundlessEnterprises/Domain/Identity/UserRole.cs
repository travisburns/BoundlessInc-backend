namespace BoundlessEnterprises.Domain.Identity;

/// <summary>
/// Assignment of a <see cref="Role"/> to a user within a specific company
/// membership (<see cref="UserCompany"/>). Maps to the identity.UserRoles table.
/// </summary>
public class UserRole
{
    private UserRole() { }

    public UserRole(Guid userCompanyId, Guid roleId)
    {
        UserCompanyId = userCompanyId;
        RoleId = roleId;
    }

    public Guid UserCompanyId { get; private set; }
    public Guid RoleId { get; private set; }
}
