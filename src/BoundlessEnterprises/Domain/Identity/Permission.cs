using BoundlessEnterprises.Domain.Common;

namespace BoundlessEnterprises.Domain.Identity;

/// <summary>
/// A fine-grained capability, named "module.action" (e.g. "companies.read",
/// "employees.write"). Grouped by module so authorization stays capability-first.
/// </summary>
public class Permission : Entity
{
    private Permission() { }

    private Permission(string name, string module, string? description)
    {
        Name = name;
        Module = module;
        Description = description;
    }

    public string Name { get; private set; } = string.Empty;
    public string Module { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public static Permission Create(string name, string module, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Permission name is required.", nameof(name));
        return new Permission(name.Trim().ToLowerInvariant(), module.Trim().ToLowerInvariant(), description);
    }
}
