using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Domain.Companies;
using BoundlessEnterprises.Domain.Documents;
using BoundlessEnterprises.Domain.Identity;
using BoundlessEnterprises.Domain.Onboarding;
using BoundlessEnterprises.Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BoundlessEnterprises.Infrastructure.Persistence.Seed;

/// <summary>
/// Seeds baseline data: the portfolio (holding company + subsidiaries), the
/// system roles and permissions, and a platform administrator account. Every
/// step is idempotent so it can run safely on each startup.
/// </summary>
public sealed class ApplicationDbSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<ApplicationDbSeeder> _logger;

    public ApplicationDbSeeder(
        ApplicationDbContext db,
        IPasswordHasher passwordHasher,
        ILogger<ApplicationDbSeeder> logger)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedCompaniesAsync(cancellationToken);
        await SeedRolesAndPermissionsAsync(cancellationToken);
        await SeedPlatformAdminAsync(cancellationToken);
        await SeedOnboardingTemplatesAsync(cancellationToken);
        await SeedBillingAsync(cancellationToken);
        await SeedDocumentsAsync(cancellationToken);
    }

    private async Task SeedCompaniesAsync(CancellationToken cancellationToken)
    {
        var existingCodes = await _db.Companies
            .Select(c => c.Code)
            .ToListAsync(cancellationToken);

        var seeds = new[]
        {
            Build("Boundless Enterprises", "boundless-enterprises", "BE-000", CompanyType.Holding,
                "Building what comes after.", "Holding Company",
                "#0B1020", employeeLogin: true, payments: false, order: 0,
                description: "The holding company and central digital operating layer for the portfolio."),
            Build("Boundless", "boundless", "BND-001", CompanyType.Subsidiary,
                "Media, IP, and worldbuilding.", "Media / IP / Worldbuilding",
                "#6D28D9", employeeLogin: true, payments: true, order: 1,
                description: "Original media, intellectual property, and worldbuilding ventures."),
            Build("Firefin", "firefin", "FIRE-001", CompanyType.Subsidiary,
                "Food, hospitality, and consumer products.", "Food / Hospitality / Consumer Products",
                "#DC2626", employeeLogin: true, payments: true, order: 2,
                description: "Restaurants, hospitality, and consumer product brands."),
            Build("SkaffaldOS", "skaffaldos", "SKAF-001", CompanyType.Subsidiary,
                "Contractor operating software.", "Contractor SaaS",
                "#0891B2", employeeLogin: true, payments: true, order: 3,
                description: "SaaS that runs the day-to-day of contracting businesses."),
            Build("DigitalHeavyWeights", "digitalheavyweights", "DHW-001", CompanyType.Subsidiary,
                "Technology and software.", "Technology / Software",
                "#2563EB", employeeLogin: true, payments: true, order: 4,
                description: "Software engineering and technology product development."),
        };

        var toAdd = seeds.Where(c => !existingCodes.Contains(c.Code)).ToList();
        if (toAdd.Count == 0)
        {
            _logger.LogInformation("Company seed skipped; portfolio already present.");
            return;
        }

        _db.Companies.AddRange(toAdd);
        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded {Count} companies into the portfolio.", toAdd.Count);
    }

    private static Company Build(
        string name, string slug, string code, CompanyType type,
        string tagline, string sector, string accent,
        bool employeeLogin, bool payments, int order, string description)
    {
        var company = Company.Create(name, slug, code, type);
        company.UpdateProfile(name, tagline, description, sector,
            logoUrl: $"/brand/logos/{slug}.svg",
            accentColor: accent,
            websiteUrl: null, domain: null, contactEmail: null, contactPhone: null);
        company.ConfigureCapabilities(employeeLogin, payments, order);
        company.Activate();
        return company;
    }

    private static readonly string[] Modules =
    {
        "companies", "employees", "onboarding", "payments",
        "documents", "integrations", "intelligence", "identity",
    };

    private async Task SeedRolesAndPermissionsAsync(CancellationToken cancellationToken)
    {
        // Permissions: <module>.read and <module>.write for each capability.
        var existingPermissionNames = await _db.Permissions
            .Select(p => p.Name)
            .ToListAsync(cancellationToken);

        var newPermissions = new List<Permission>();
        foreach (var module in Modules)
        {
            foreach (var action in new[] { "read", "write" })
            {
                var name = $"{module}.{action}";
                if (!existingPermissionNames.Contains(name))
                    newPermissions.Add(Permission.Create(name, module, $"Can {action} {module}."));
            }
        }

        if (newPermissions.Count > 0)
        {
            _db.Permissions.AddRange(newPermissions);
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Seeded {Count} permissions.", newPermissions.Count);
        }

        var allPermissions = await _db.Permissions.ToListAsync(cancellationToken);

        var existingRoleNames = await _db.Roles
            .Select(r => r.NormalizedName)
            .ToListAsync(cancellationToken);

        foreach (var roleName in RoleNames.All)
        {
            if (existingRoleNames.Contains(roleName.ToUpperInvariant()))
                continue;

            var role = Role.Create(roleName, RoleNames.Descriptions[roleName], isSystem: true);

            // Owner gets every permission; other roles get read across modules.
            var grants = roleName == RoleNames.Owner
                ? allPermissions
                : allPermissions.Where(p => p.Name.EndsWith(".read", StringComparison.Ordinal));

            foreach (var permission in grants)
                role.AssignPermission(permission.Id);

            _db.Roles.Add(role);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedPlatformAdminAsync(CancellationToken cancellationToken)
    {
        const string adminEmail = "admin@boundless.enterprises";

        if (await _db.Users.AnyAsync(u => u.Email == adminEmail, cancellationToken))
        {
            _logger.LogInformation("Admin seed skipped; platform admin already present.");
            return;
        }

        var admin = User.Create(
            adminEmail,
            _passwordHasher.Hash("ChangeMe!123"),
            "Platform",
            "Administrator");
        admin.GrantPlatformAdmin();

        var holding = await _db.Companies.FirstOrDefaultAsync(c => c.Code == "BE-000", cancellationToken);
        var ownerRole = await _db.Roles
            .FirstOrDefaultAsync(r => r.NormalizedName == RoleNames.Owner.ToUpperInvariant(), cancellationToken);

        if (holding is not null)
        {
            var membership = admin.AddMembership(holding.Id, isPrimary: true);
            if (ownerRole is not null)
                membership.AssignRole(ownerRole.Id);
        }

        _db.Users.Add(admin);
        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded platform admin {Email} (default password ChangeMe!123).", adminEmail);
    }

    private async Task SeedOnboardingTemplatesAsync(CancellationToken cancellationToken)
    {
        var companiesByCode = await _db.Companies
            .Where(c => c.Code == "FIRE-001" || c.Code == "BND-001")
            .ToDictionaryAsync(c => c.Code, c => c.Id, cancellationToken);

        var existingNames = await _db.OnboardingTemplates
            .Select(t => t.Name)
            .ToListAsync(cancellationToken);

        var added = 0;

        if (companiesByCode.TryGetValue("FIRE-001", out var firefinId) &&
            !existingNames.Contains("Firefin — Kitchen Employee"))
        {
            var t = OnboardingTemplate.Create(firefinId, "Firefin — Kitchen Employee",
                "Onboarding for Firefin kitchen and hospitality staff.");
            foreach (var name in new[]
            {
                "Personal information", "Employment documents", "Tax / payroll information",
                "Emergency contact", "Policies", "Food safety training",
                "Firefin orientation", "Equipment / access", "Manager approval",
            })
            {
                t.AddStep(name);
            }
            _db.OnboardingTemplates.Add(t);
            added++;
        }

        if (companiesByCode.TryGetValue("BND-001", out var boundlessId) &&
            !existingNames.Contains("Boundless — Creative Employee"))
        {
            var t = OnboardingTemplate.Create(boundlessId, "Boundless — Creative Employee",
                "Onboarding for Boundless creative and worldbuilding staff.");
            foreach (var name in new[]
            {
                "Personal information", "NDA / IP agreement", "Core onboarding",
                "Soul Skill", "Ring assignment", "Mask / Veil / Quill",
                "Project access", "Software accounts", "First Horizon",
            })
            {
                t.AddStep(name);
            }
            _db.OnboardingTemplates.Add(t);
            added++;
        }

        if (added > 0)
        {
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Seeded {Count} onboarding templates.", added);
        }
    }

    private async Task SeedBillingAsync(CancellationToken cancellationToken)
    {
        var firefin = await _db.Companies.FirstOrDefaultAsync(c => c.Code == "FIRE-001", cancellationToken);
        if (firefin is null)
            return;

        if (await _db.Customers.AnyAsync(c => c.CompanyId == firefin.Id, cancellationToken))
            return; // Already seeded.

        var customer = Customer.Create(firefin.Id, "Harbor Table Group", "billing@harbortable.com");
        _db.Customers.Add(customer);

        var invoice1 = Invoice.Create(firefin.Id, customer.Id, "INV-1001");
        invoice1.AddItem("Catering — corporate lunch (50 covers)", 50, 24.00m);
        invoice1.AddItem("Delivery & service", 1, 150.00m);
        invoice1.SetDueDate(DateTime.UtcNow.AddDays(14));
        invoice1.Issue();
        _db.Invoices.Add(invoice1);

        var invoice2 = Invoice.Create(firefin.Id, customer.Id, "INV-1002");
        invoice2.AddItem("Private dining event", 1, 1800.00m);
        invoice2.SetDueDate(DateTime.UtcNow.AddDays(30));
        invoice2.Issue();
        _db.Invoices.Add(invoice2);

        var subscription = Subscription.Create(firefin.Id, customer.Id,
            "Firefin Provisions", "Professional", 200.00m, BillingInterval.Monthly);
        _db.Subscriptions.Add(subscription);

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded billing sample data for Firefin.");
    }

    private async Task SeedDocumentsAsync(CancellationToken cancellationToken)
    {
        var firefin = await _db.Companies.FirstOrDefaultAsync(c => c.Code == "FIRE-001", cancellationToken);
        if (firefin is null)
            return;

        if (await _db.Documents.AnyAsync(d => d.CompanyId == firefin.Id, cancellationToken))
            return;

        _db.Documents.AddRange(
            Document.Create(firefin.Id, "Firefin Employee Handbook", DocumentType.Policy,
                "Company policies, conduct, and expectations."),
            Document.Create(firefin.Id, "Food Safety & Hygiene Policy", DocumentType.Policy,
                "Kitchen safety, hygiene, and handling standards."),
            Document.Create(firefin.Id, "Confidentiality Agreement", DocumentType.Agreement,
                "Standard confidentiality and IP agreement."));

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded document library for Firefin.");
    }
}
