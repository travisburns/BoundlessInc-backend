namespace BoundlessEnterprises.Domain.Identity;

/// <summary>Join between a <see cref="Role"/> and a <see cref="Permission"/>.</summary>
public class RolePermission
{
    private RolePermission() { }

    public RolePermission(Guid roleId, Guid permissionId)
    {
        RoleId = roleId;
        PermissionId = permissionId;
    }

    public Guid RoleId { get; private set; }
    public Guid PermissionId { get; private set; }
}
