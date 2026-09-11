using BoundlessEnterprises.Domain.Common;

namespace BoundlessEnterprises.Domain.Work;

/// <summary>A quick-link shown in a ring's "Helpful Resources".</summary>
public class RingResource
{
    public string Label { get; set; } = string.Empty;
    public string? Sublabel { get; set; }
    public string? Href { get; set; }
}

/// <summary>
/// A Ring: a founding, top-tier employee who heads one of the thirteen domains of
/// Boundless Enterprises and leads the team beneath them. Enterprise-level — one
/// Ring per domain across the whole enterprise. Created and configured by admin;
/// the holder logs in to their own work-management dashboard.
/// </summary>
public class Ring : AuditableEntity
{
    private Ring() { }

    private Ring(RingDomain domain, string name, string codePrefix)
    {
        Domain = domain;
        Name = name;
        CodePrefix = codePrefix;
        NextSequence = 1;
    }

    public RingDomain Domain { get; private set; }

    /// <summary>Display name, e.g. "The Resonance".</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Discipline line, e.g. "Music · Audio · Sound Design · Emotion".</summary>
    public string? Disciplines { get; private set; }

    /// <summary>The founder who holds this ring.</summary>
    public string HolderName { get; private set; } = string.Empty;

    /// <summary>Optional link to the holder's login account.</summary>
    public Guid? HolderUserId { get; private set; }

    // Dashboard presentation.
    public string? HeroTitle { get; private set; }
    public string? HeroSubtitle { get; private set; }
    public string? Focus { get; private set; }
    public string? Motto { get; private set; }
    public string? AccentColor { get; private set; }
    public string? HeroImageUrl { get; private set; }

    /// <summary>Prefix for this ring's assignment codes, e.g. "RES" → RES-0042.</summary>
    public string CodePrefix { get; private set; } = string.Empty;

    /// <summary>Next number in the assignment code sequence.</summary>
    public int NextSequence { get; private set; }

    public List<RingResource> Resources { get; private set; } = new();

    public static Ring Create(RingDomain domain, string name, string codePrefix, string holderName)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Ring name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(codePrefix)) throw new ArgumentException("Code prefix is required.", nameof(codePrefix));

        var ring = new Ring(domain, name.Trim(), codePrefix.Trim().ToUpperInvariant());
        ring.HolderName = string.IsNullOrWhiteSpace(holderName) ? "Unassigned" : holderName.Trim();
        return ring;
    }

    public void UpdateProfile(
        string name, string? disciplines, string holderName, string? heroTitle, string? heroSubtitle,
        string? focus, string? motto, string? accentColor, string? heroImageUrl)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Ring name is required.", nameof(name));
        Name = name.Trim();
        Disciplines = disciplines;
        HolderName = string.IsNullOrWhiteSpace(holderName) ? HolderName : holderName.Trim();
        HeroTitle = heroTitle;
        HeroSubtitle = heroSubtitle;
        Focus = focus;
        Motto = motto;
        AccentColor = accentColor;
        HeroImageUrl = heroImageUrl;
    }

    public void SetHolderUser(Guid? userId) => HolderUserId = userId;

    public void SetResources(IEnumerable<RingResource> resources)
    {
        Resources = resources.ToList();
    }

    /// <summary>Reserves and returns the next assignment code, e.g. "RES-0042".</summary>
    public string NextCode()
    {
        var code = $"{CodePrefix}-{NextSequence:0000}";
        NextSequence++;
        return code;
    }

    /// <summary>Seeding helper: fast-forward the code sequence past pre-numbered demo assignments.</summary>
    internal void SetNextSequence(int next) => NextSequence = next;

    /// <summary>Seeding helper: set the holder-name directly.</summary>
    internal void SeedHolder(string holderName, Guid? holderUserId)
    {
        HolderName = holderName;
        HolderUserId = holderUserId;
    }
}
