using BoundlessEnterprises.Domain.Companies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BoundlessEnterprises.Infrastructure.Persistence.Seed;

/// <summary>
/// Seeds the baseline portfolio: the holding company plus the founding
/// subsidiaries described in the platform architecture. Idempotent — only
/// inserts companies whose code is not already present.
/// </summary>
public sealed class ApplicationDbSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<ApplicationDbSeeder> _logger;

    public ApplicationDbSeeder(ApplicationDbContext db, ILogger<ApplicationDbSeeder> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedCompaniesAsync(cancellationToken);
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
}
