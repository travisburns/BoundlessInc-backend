using BoundlessEnterprises.Domain.Common;

namespace BoundlessEnterprises.Domain.Identity;

/// <summary>
/// A named set of permissions that can be assigned to a user within a company
/// (e.g. Owner, Executive, Manager, Writer, Employee). System roles are seeded
/// and cannot be deleted.
/// </summary>
public class Role : AuditableEntity
{
    private readonly List<RolePermission> _permissions = new();

    private Role() { }

    private Role(string name, string? description, bool isSystem)
    {
        Name = name;
        NormalizedName = name.ToUpperInvariant();
        Description = description;
        IsSystem = isSystem;
    }

    public string Name { get; private set; } = string.Empty;
    public string NormalizedName { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsSystem { get; private set; }

    public IReadOnlyCollection<RolePermission> Permissions => _permissions.AsReadOnly();

    public static Role Create(string name, string? description = null, bool isSystem = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name is required.", nameof(name));
        return new Role(name.Trim(), description, isSystem);
    }

    public void AssignPermission(Guid permissionId)
    {
        if (_permissions.All(p => p.PermissionId != permissionId))
            _permissions.Add(new RolePermission(Id, permissionId));
    }
}
