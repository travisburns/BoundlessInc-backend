using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Integrations.Services;
using BoundlessEnterprises.Application.Onboarding.Services;
using BoundlessEnterprises.Domain.Companies;
using BoundlessEnterprises.Domain.Documents;
using BoundlessEnterprises.Domain.Identity;
using BoundlessEnterprises.Domain.Integrations;
using BoundlessEnterprises.Domain.Intelligence;
using BoundlessEnterprises.Domain.Onboarding;
using BoundlessEnterprises.Domain.Payments;
using BoundlessEnterprises.Domain.Work;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using K = BoundlessEnterprises.Domain.Onboarding.OnboardingStepKind;
using WorkStatus = BoundlessEnterprises.Domain.Work.AssignmentStatus;

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
        await RetireLegacyTemplatesAsync(cancellationToken);
        await SeedOnboardingInvitationsAsync(cancellationToken);
        await SeedBillingAsync(cancellationToken);
        await SeedDocumentsAsync(cancellationToken);
        await SeedIntelligenceAsync(cancellationToken);
        await SeedRingsAsync(cancellationToken);
    }

    /// <summary>
    /// Seeds "The Resonance" ring (Aaron, head of audio) with its dashboard copy,
    /// a login for the holder, and the sample assignments from the design.
    /// </summary>
    private async Task SeedRingsAsync(CancellationToken cancellationToken)
    {
        if (await _db.Rings.AnyAsync(r => r.Domain == RingDomain.Resonance, cancellationToken))
            return;

        // Holder login.
        const string holderEmail = "aaron@boundless.enterprises";
        var aaron = await _db.Users.FirstOrDefaultAsync(u => u.Email == holderEmail, cancellationToken);
        if (aaron is null)
        {
            aaron = User.Create(holderEmail, _passwordHasher.Hash("ChangeMe!123"), "Aaron", "Cole");
            var holding = await _db.Companies.FirstOrDefaultAsync(c => c.Code == "BE-000", cancellationToken);
            var employeeRole = await _db.Roles
                .FirstOrDefaultAsync(r => r.NormalizedName == RoleNames.Employee.ToUpperInvariant(), cancellationToken);
            if (holding is not null)
            {
                var membership = aaron.AddMembership(holding.Id, isPrimary: true);
                if (employeeRole is not null) membership.AssignRole(employeeRole.Id);
            }
            _db.Users.Add(aaron);
            await _db.SaveChangesAsync(cancellationToken);
        }

        // The Resonance ring.
        var ring = Ring.Create(RingDomain.Resonance, "The Resonance", "RES", "Aaron");
        ring.UpdateProfile(
            "The Resonance",
            "Music · Audio · Sound Design · Emotion",
            "Aaron",
            heroTitle: "Create Worlds Through Sound",
            heroSubtitle: "Same notes. Greater realms.",
            focus: "Music is the bridge between emotion and reality.",
            motto: "Sound gives worlds a soul.",
            accentColor: "#C2410C",
            heroImageUrl: null);
        ring.SetHolderUser(aaron.Id);
        ring.SetResources(new[]
        {
            new RingResource { Label = "Audio Library", Sublabel = "Samples & Instruments", Href = "#" },
            new RingResource { Label = "Style Guide", Sublabel = "Musical Direction", Href = "#" },
            new RingResource { Label = "World References", Sublabel = "Regions & Cultures", Href = "#" },
            new RingResource { Label = "Tools & Software", Sublabel = "Approved Tools", Href = "#" },
        });
        _db.Rings.Add(ring);
        await _db.SaveChangesAsync(cancellationToken);

        // Sample assignments (from the design).
        var now = DateTime.UtcNow;
        DateOnly Due(int day) => new(2026, 10, day);

        Assignment Build(string code, string title, string summary, AssignmentType type,
            AssignmentPriority priority, WorkStatus status, int progress, int dueDay,
            string? objective = null, string? deliverable = null, string[]? criteria = null,
            string? nextStep = null, string? domainDataJson = null)
        {
            var a = Assignment.ForSeed(ring.Id, code, title, type, priority, now);
            a.UpdateCore(title, summary, objective, type, priority,
                assigneeName: "Aaron", issuedBy: "The Crown", companyId: null,
                startDate: null, dueDate: Due(dueDay), deliverable: deliverable,
                acceptanceCriteria: criteria ?? Array.Empty<string>(), dependencies: null, blockers: null,
                nextStep: nextStep, reviewRequired: status == WorkStatus.Review,
                references: Array.Empty<string>(), tags: Array.Empty<string>(), domainDataJson: domainDataJson);
            a.SeedState(status, progress, status == WorkStatus.Complete ? now : null);
            return a;
        }

        var res0042 = Build("RES-0042", "Establish the Sound of Avarra", "Primary theme and musical language",
            AssignmentType.Create, AssignmentPriority.High, WorkStatus.InProgress, 45, 16,
            objective: "Establish the first recognizable sonic identity for the Avarra region.",
            deliverable: "A 2–4 minute primary musical theme accompanied by a short written explanation of the musical language being established.",
            criteria: new[]
            {
                "Establishes a recognizable Avarran identity",
                "Fits established world / canon",
                "Can serve as the foundation for later variations",
                "Production quality sufficient for internal review",
                "Source project / files submitted",
            },
            nextStep: "Draft main theme motif",
            domainDataJson: "{\"audioType\":\"MUSIC\",\"region\":\"Avarra\",\"durationTarget\":\"2-4 minutes\",\"mood\":[\"ancient\",\"melancholic\",\"triumphant\"],\"format\":[\"WAV\",\"project-source\"]}");
        res0042.AddUpdate("Aaron", "Started sketching the core motif — leaning into low strings and a distant choir.", now.AddHours(-4));
        res0042.AddUpdate("Aaron", "Uploaded a first reference sketch for feedback.", now.AddHours(-2));

        var seeded = new[]
        {
            res0042,
            Build("RES-0039", "Sound Library Expansion", "New instruments and ambient textures",
                AssignmentType.Build, AssignmentPriority.Normal, WorkStatus.InProgress, 30, 12),
            Build("RES-0038", "Review Battle Theme v2", "Internal review and feedback",
                AssignmentType.Review, AssignmentPriority.Normal, WorkStatus.Review, 80, 8),
            Build("RES-0031", "Regional Themes — Valethra", "Exploration and concept drafts",
                AssignmentType.Create, AssignmentPriority.Normal, WorkStatus.InProgress, 20, 18),
            Build("RES-0028", "Final Mix — Prologue Cinematic", "Master and deliver final audio",
                AssignmentType.Deliver, AssignmentPriority.Critical, WorkStatus.InProgress, 60, 6),
            Build("RES-0041", "Character Themes — Main Cast", "Leitmotifs for Elias, Anna, Chaim",
                AssignmentType.Create, AssignmentPriority.High, WorkStatus.Assigned, 0, 20),
        };
        _db.Assignments.AddRange(seeded);

        // A couple of completed items so the "Completed" view isn't empty.
        _db.Assignments.Add(Build("RES-0018", "Main Menu Theme", "Loop and stinger",
            AssignmentType.Create, AssignmentPriority.Normal, WorkStatus.Complete, 100, 1));
        _db.Assignments.Add(Build("RES-0022", "Ambient Bed — Forest of Hollows", "Loopable ambience",
            AssignmentType.Create, AssignmentPriority.Low, WorkStatus.Complete, 100, 2));

        ring.SetNextSequence(43);
        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded The Resonance ring (holder {Email}) with sample assignments.", holderEmail);
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

    // One tailored onboarding template per company. Every company shares the
    // reusable engine; only the step list (and the forms each step renders,
    // via its kind) differs. K is a short alias for the step kind.
    private static readonly IReadOnlyList<CompanyOnboarding> OnboardingByCompany = new[]
    {
        new CompanyOnboarding("BE-000", "Boundless Enterprises — Team Member",
            "Onboarding into the holding company and central platform team.", new[]
            {
                Step("Personal information", K.PersonalInfo),
                Step("Emergency contact", K.EmergencyContact),
                Step("Tax & payroll", K.TaxPayroll),
                Step("Direct deposit", K.DirectDeposit),
                Step("Code of conduct", K.PolicyAcknowledgement),
                Step("Systems & access", K.ITAccess),
                Step("Welcome orientation", K.Generic, required: false),
            }),
        new CompanyOnboarding("BND-001", "Boundless — Creative Team Member",
            "Onboarding for Boundless media, IP, and worldbuilding staff.", new[]
            {
                Step("Personal information", K.PersonalInfo),
                Step("Emergency contact", K.EmergencyContact),
                Step("NDA & IP assignment", K.PolicyAcknowledgement),
                Step("Tax & payroll", K.TaxPayroll),
                Step("Direct deposit", K.DirectDeposit),
                Step("Creative tools & accounts", K.ITAccess),
                Step("Worldbuilding orientation", K.Generic, required: false),
            }),
        new CompanyOnboarding("FIRE-001", "Firefin — Kitchen & Hospitality",
            "Onboarding for Firefin kitchen and hospitality staff.", new[]
            {
                Step("Personal information", K.PersonalInfo),
                Step("Emergency contact", K.EmergencyContact),
                Step("Weekly availability", K.Availability),
                Step("Tax & payroll", K.TaxPayroll),
                Step("Direct deposit", K.DirectDeposit),
                Step("Food safety & hygiene policy", K.PolicyAcknowledgement),
                Step("Uniform & equipment", K.ITAccess),
                Step("Kitchen orientation", K.Generic, required: false),
            }),
        new CompanyOnboarding("SKAF-001", "SkaffaldOS — Software Team Member",
            "Onboarding for SkaffaldOS product and engineering staff.", new[]
            {
                Step("Personal information", K.PersonalInfo),
                Step("Emergency contact", K.EmergencyContact),
                Step("Tax & payroll", K.TaxPayroll),
                Step("Direct deposit", K.DirectDeposit),
                Step("Security & acceptable use policy", K.PolicyAcknowledgement),
                Step("Developer accounts & access", K.ITAccess),
                Step("Product onboarding", K.Generic, required: false),
            }),
        new CompanyOnboarding("DHW-001", "DigitalHeavyWeights — Engineer",
            "Onboarding for DigitalHeavyWeights software engineers.", new[]
            {
                Step("Personal information", K.PersonalInfo),
                Step("Emergency contact", K.EmergencyContact),
                Step("Tax & payroll", K.TaxPayroll),
                Step("Direct deposit", K.DirectDeposit),
                Step("Confidentiality & IP policy", K.PolicyAcknowledgement),
                Step("Engineering environment & access", K.ITAccess),
                Step("First sprint orientation", K.Generic, required: false),
            }),
    };

    // Per-company demo invitations, so every company's flow can be tried
    // immediately without creating an invite through the portal.
    private static readonly IReadOnlyList<DemoInvite> DemoInvites = new[]
    {
        new DemoInvite("BE-000", "ONB-DEMO-BE", "Jordan", "Vale", "Platform Operations", "jordan.vale@boundless.example"),
        new DemoInvite("BND-001", "ONB-DEMO-BND", "Wren", "Ashcroft", "Worldbuilding Writer", "wren.ashcroft@boundless.example"),
        new DemoInvite("FIRE-001", "ONB-DEMO-FIRE", "Sam", "Rivera", "Kitchen Team Member", "sam.rivera@firefin.example"),
        new DemoInvite("SKAF-001", "ONB-DEMO-SKAF", "Ezra", "Lindqvist", "Product Engineer", "ezra.lindqvist@skaffaldos.example"),
        new DemoInvite("DHW-001", "ONB-DEMO-DHW", "Nia", "Osei", "Software Engineer", "nia.osei@dhw.example"),
    };

    private async Task SeedOnboardingTemplatesAsync(CancellationToken cancellationToken)
    {
        var codes = OnboardingByCompany.Select(o => o.CompanyCode).ToList();
        var companiesByCode = await _db.Companies
            .Where(c => codes.Contains(c.Code))
            .ToDictionaryAsync(c => c.Code, c => c.Id, cancellationToken);

        var existingNames = await _db.OnboardingTemplates
            .Select(t => t.Name)
            .ToListAsync(cancellationToken);

        var added = 0;
        foreach (var def in OnboardingByCompany)
        {
            if (!companiesByCode.TryGetValue(def.CompanyCode, out var companyId)) continue;
            if (existingNames.Contains(def.TemplateName)) continue;

            var template = OnboardingTemplate.Create(companyId, def.TemplateName, def.Description);
            foreach (var step in def.Steps)
                template.AddStep(step.Name, description: null, isRequired: step.Required, kind: step.Kind);

            _db.OnboardingTemplates.Add(template);
            added++;
        }

        if (added > 0)
        {
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Seeded {Count} onboarding templates.", added);
        }
    }

    /// <summary>
    /// Seeds one demo self-serve invitation per company with a fixed, well-known
    /// code so each company's onboarding wizard can be tried immediately.
    /// </summary>
    private async Task SeedOnboardingInvitationsAsync(CancellationToken cancellationToken)
    {
        var companyIdByCode = await _db.Companies
            .Where(c => DemoInvites.Select(d => d.CompanyCode).Contains(c.Code))
            .ToDictionaryAsync(c => c.Code, c => c.Id, cancellationToken);

        // A company may have more than one template (e.g. an older seed plus the
        // tailored one), so match each demo to its specific template by name.
        var templateNameByCode = OnboardingByCompany.ToDictionary(o => o.CompanyCode, o => o.TemplateName);

        var added = 0;
        foreach (var demo in DemoInvites)
        {
            if (!companyIdByCode.TryGetValue(demo.CompanyCode, out var companyId)) continue;
            if (!templateNameByCode.TryGetValue(demo.CompanyCode, out var templateName)) continue;

            var template = await _db.OnboardingTemplates
                .FirstOrDefaultAsync(t => t.CompanyId == companyId && t.Name == templateName, cancellationToken);
            if (template is null) continue;

            var hash = InviteCodes.Hash(demo.Code);
            if (await _db.OnboardingInvitations.AnyAsync(i => i.CodeHash == hash, cancellationToken))
                continue;

            _db.OnboardingInvitations.Add(OnboardingInvitation.Create(
                companyId, template.Id, demo.Email, demo.FirstName, demo.LastName,
                demo.Title, hash, DateTime.UtcNow.AddDays(60)));
            added++;
            _logger.LogInformation("Seeded demo onboarding invitation for {Company} (code {Code}).",
                demo.CompanyCode, demo.Code);
        }

        if (added > 0)
            await _db.SaveChangesAsync(cancellationToken);
    }

    // Early checkbox-only templates, superseded by the tailored form-based ones.
    private static readonly string[] LegacyTemplateNames =
    {
        "Firefin — Kitchen Employee",
        "Boundless — Creative Employee",
    };

    /// <summary>Deactivates superseded templates so they drop out of the picker without losing history.</summary>
    private async Task RetireLegacyTemplatesAsync(CancellationToken cancellationToken)
    {
        var legacy = await _db.OnboardingTemplates
            .Where(t => LegacyTemplateNames.Contains(t.Name) && t.IsActive)
            .ToListAsync(cancellationToken);

        if (legacy.Count == 0) return;

        foreach (var t in legacy)
            t.Deactivate();

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Retired {Count} legacy onboarding templates.", legacy.Count);
    }

    private static StepDef Step(string name, OnboardingStepKind kind, bool required = true) => new(name, kind, required);

    private sealed record StepDef(string Name, OnboardingStepKind Kind, bool Required);
    private sealed record CompanyOnboarding(string CompanyCode, string TemplateName, string Description, StepDef[] Steps);
    private sealed record DemoInvite(string CompanyCode, string Code, string FirstName, string LastName, string Title, string Email);

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

    private async Task SeedIntelligenceAsync(CancellationToken cancellationToken)
    {
        if (await _db.BusinessEvents.AnyAsync(cancellationToken))
            return; // Already seeded.

        // Each operating company connects an integration and pushes normalized
        // events. Revenue profiles differ so the executive view has contrast.
        var profiles = new (string Code, string App, string EventType, decimal Min, decimal Max, int PerDay)[]
        {
            ("FIRE-001", "Firefin POS", "OrderCompleted", 40m, 120m, 6),
            ("SKAF-001", "SkaffaldOS", "SubscriptionCharged", 150m, 400m, 2),
            ("BND-001", "Boundless Studio", "LicenseSold", 200m, 900m, 1),
            ("DHW-001", "DigitalHeavyWeights", "ContractMilestone", 500m, 2500m, 1),
        };

        var codes = profiles.Select(p => p.Code).ToArray();
        var companies = await _db.Companies
            .Where(c => codes.Contains(c.Code))
            .ToDictionaryAsync(c => c.Code, c => c.Id, cancellationToken);

        var random = new Random(20260910);
        var today = DateTime.UtcNow.Date;
        var events = new List<BusinessEvent>();
        var addedIntegrations = 0;

        foreach (var profile in profiles)
        {
            if (!companies.TryGetValue(profile.Code, out var companyId))
                continue;

            var (_, hash, prefix) = ApiKeys.Generate();
            var integration = Integration.Create(companyId, profile.App, hash, prefix);
            _db.Integrations.Add(integration);
            addedIntegrations++;

            for (var dayOffset = 13; dayOffset >= 0; dayOffset--)
            {
                var day = today.AddDays(-dayOffset);
                for (var n = 0; n < profile.PerDay; n++)
                {
                    var revenue = Math.Round(
                        (decimal)((double)profile.Min + random.NextDouble() * (double)(profile.Max - profile.Min)), 2);
                    var occurred = day.AddHours(random.Next(8, 22)).AddMinutes(random.Next(0, 60));
                    var evt = BusinessEvent.Create(companyId, integration.Id, profile.EventType,
                        revenue, "USD", occurred, $"seed-{profile.Code}-{dayOffset}-{n}");
                    events.Add(evt);
                    integration.RecordEvent(occurred);
                }
            }
        }

        _db.BusinessEvents.AddRange(events);
        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded {Integrations} integrations and {Events} business events.",
            addedIntegrations, events.Count);
    }
}
