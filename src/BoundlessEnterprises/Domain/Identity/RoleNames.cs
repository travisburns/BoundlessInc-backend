namespace BoundlessEnterprises.Domain.Identity;

/// <summary>Well-known system role names seeded into every environment.</summary>
public static class RoleNames
{
    public const string Owner = "Owner";
    public const string Executive = "Executive";
    public const string Manager = "Manager";
    public const string Employee = "Employee";

    public static readonly IReadOnlyDictionary<string, string> Descriptions =
        new Dictionary<string, string>
        {
            [Owner] = "Full control of a company within the platform.",
            [Executive] = "Cross-company visibility and executive intelligence.",
            [Manager] = "Manages employees, onboarding, and day-to-day operations.",
            [Employee] = "Standard employee access to assigned resources.",
        };

    public static readonly string[] All = { Owner, Executive, Manager, Employee };
}
