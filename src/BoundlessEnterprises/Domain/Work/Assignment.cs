using BoundlessEnterprises.Domain.Common;

namespace BoundlessEnterprises.Domain.Work;

/// <summary>
/// The universal Ring task primitive. Every ring uses the same skeleton; each can
/// attach domain-specific fields via <see cref="DomainDataJson"/>. An assignment
/// belongs to a ring and may target a specific company (subsidiary).
/// </summary>
public class Assignment : AuditableEntity
{
    private readonly List<AssignmentUpdate> _updates = new();

    private Assignment() { }

    private Assignment(Guid ringId, string code, string title, AssignmentType type, AssignmentPriority priority)
    {
        RingId = ringId;
        Code = code;
        Title = title;
        Type = type;
        Priority = priority;
        Status = AssignmentStatus.Assigned;
    }

    public Guid RingId { get; private set; }
    public string Code { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;
    public string? Summary { get; private set; }
    public string? Objective { get; private set; }

    public AssignmentType Type { get; private set; }
    public AssignmentStatus Status { get; private set; }
    public AssignmentPriority Priority { get; private set; }

    public string? AssigneeName { get; private set; }
    public string? IssuedBy { get; private set; }

    /// <summary>Optional subsidiary the work is for.</summary>
    public Guid? CompanyId { get; private set; }

    public DateTime? AssignedAtUtc { get; private set; }
    public DateOnly? StartDate { get; private set; }
    public DateOnly? DueDate { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }

    public string? Deliverable { get; private set; }
    public List<string> AcceptanceCriteria { get; private set; } = new();

    public string? Dependencies { get; private set; }
    public string? Blockers { get; private set; }

    public int ProgressPercent { get; private set; }
    public string? NextStep { get; private set; }

    public bool ReviewRequired { get; private set; }
    public string? ReviewedBy { get; private set; }

    public List<string> References { get; private set; } = new();
    public List<string> Tags { get; private set; } = new();

    /// <summary>Ring-specific fields as a raw JSON object (the domainData extension).</summary>
    public string? DomainDataJson { get; private set; }

    public IReadOnlyList<AssignmentUpdate> Updates => _updates.OrderByDescending(u => u.CreatedAtUtc).ToList();

    public static Assignment Create(
        Ring ring, string title, AssignmentType type, AssignmentPriority priority, DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(ring);
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required.", nameof(title));

        return new Assignment(ring.Id, ring.NextCode(), title.Trim(), type, priority)
        {
            AssignedAtUtc = nowUtc,
        };
    }

    public void UpdateCore(
        string title, string? summary, string? objective, AssignmentType type, AssignmentPriority priority,
        string? assigneeName, string? issuedBy, Guid? companyId, DateOnly? startDate, DateOnly? dueDate,
        string? deliverable, IEnumerable<string> acceptanceCriteria, string? dependencies, string? blockers,
        string? nextStep, bool reviewRequired, IEnumerable<string> references, IEnumerable<string> tags,
        string? domainDataJson)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required.", nameof(title));
        Title = title.Trim();
        Summary = summary;
        Objective = objective;
        Type = type;
        Priority = priority;
        AssigneeName = assigneeName;
        IssuedBy = issuedBy;
        CompanyId = companyId;
        StartDate = startDate;
        DueDate = dueDate;
        Deliverable = deliverable;
        Dependencies = dependencies;
        Blockers = blockers;
        NextStep = nextStep;
        ReviewRequired = reviewRequired;
        DomainDataJson = domainDataJson;

        AcceptanceCriteria = acceptanceCriteria.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).ToList();
        References = references.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).ToList();
        Tags = tags.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).ToList();
    }

    public void SetStatus(AssignmentStatus status, DateTime nowUtc)
    {
        Status = status;
        if (status == AssignmentStatus.Assigned && AssignedAtUtc is null) AssignedAtUtc = nowUtc;
        if (status == AssignmentStatus.Complete)
        {
            CompletedAtUtc = nowUtc;
            ProgressPercent = 100;
        }
        else
        {
            CompletedAtUtc = null;
        }
    }

    public void SetProgress(int percent)
    {
        ProgressPercent = Math.Clamp(percent, 0, 100);
    }

    public void SetReviewedBy(string? reviewedBy) => ReviewedBy = reviewedBy;

    public AssignmentUpdate AddUpdate(string author, string body, DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(body)) throw new ArgumentException("Update body is required.", nameof(body));
        var update = new AssignmentUpdate(Id, string.IsNullOrWhiteSpace(author) ? "Unknown" : author.Trim(), body.Trim(), nowUtc);
        _updates.Add(update);
        return update;
    }

    // Seeding helper: create with a fixed pre-numbered code (RES-0042) rather than the ring sequence.
    internal static Assignment ForSeed(Guid ringId, string code, string title, AssignmentType type, AssignmentPriority priority, DateTime assignedAtUtc)
        => new(ringId, code, title, type, priority) { AssignedAtUtc = assignedAtUtc };

    // Seeding helper: set a fixed status/progress without going through the lifecycle.
    internal void SeedState(AssignmentStatus status, int progress, DateTime? completedAtUtc)
    {
        Status = status;
        ProgressPercent = progress;
        CompletedAtUtc = completedAtUtc;
    }
}
