using BoundlessEnterprises.Domain.Work;

namespace BoundlessEnterprises.Application.Work.DTOs;

public record RingResourceDto
{
    public string Label { get; init; } = string.Empty;
    public string? Sublabel { get; init; }
    public string? Href { get; init; }
}

public record RingSummaryDto
{
    public Guid Id { get; init; }
    public string Domain { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Disciplines { get; init; }
    public string HolderName { get; init; } = string.Empty;
    public string? AccentColor { get; init; }
    public string CodePrefix { get; init; } = string.Empty;

    public static RingSummaryDto FromEntity(Ring r) => new()
    {
        Id = r.Id,
        Domain = r.Domain.ToString(),
        Slug = r.Domain.ToString().ToLowerInvariant(),
        Name = r.Name,
        Disciplines = r.Disciplines,
        HolderName = r.HolderName,
        AccentColor = r.AccentColor,
        CodePrefix = r.CodePrefix,
    };
}

public record RingDto
{
    public Guid Id { get; init; }
    public string Domain { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Disciplines { get; init; }
    public string HolderName { get; init; } = string.Empty;
    public Guid? HolderUserId { get; init; }
    public string? HeroTitle { get; init; }
    public string? HeroSubtitle { get; init; }
    public string? Focus { get; init; }
    public string? Motto { get; init; }
    public string? AccentColor { get; init; }
    public string? HeroImageUrl { get; init; }
    public string CodePrefix { get; init; } = string.Empty;
    public IReadOnlyList<RingResourceDto> Resources { get; init; } = Array.Empty<RingResourceDto>();

    public static RingDto FromEntity(Ring r) => new()
    {
        Id = r.Id,
        Domain = r.Domain.ToString(),
        Slug = r.Domain.ToString().ToLowerInvariant(),
        Name = r.Name,
        Disciplines = r.Disciplines,
        HolderName = r.HolderName,
        HolderUserId = r.HolderUserId,
        HeroTitle = r.HeroTitle,
        HeroSubtitle = r.HeroSubtitle,
        Focus = r.Focus,
        Motto = r.Motto,
        AccentColor = r.AccentColor,
        HeroImageUrl = r.HeroImageUrl,
        CodePrefix = r.CodePrefix,
        Resources = r.Resources.Select(x => new RingResourceDto { Label = x.Label, Sublabel = x.Sublabel, Href = x.Href }).ToList(),
    };
}

public record AssignmentUpdateDto
{
    public Guid Id { get; init; }
    public string Author { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
}

public record RingEventDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public DateOnly Date { get; init; }
    public string? TimeLabel { get; init; }
    public string? Detail { get; init; }

    public static RingEventDto FromEntity(RingEvent e) => new()
    {
        Id = e.Id, Title = e.Title, Date = e.Date, TimeLabel = e.TimeLabel, Detail = e.Detail,
    };
}

public record RingActivityDto
{
    public Guid Id { get; init; }
    public string Kind { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }

    public static RingActivityDto FromEntity(RingActivity a) => new()
    {
        Id = a.Id, Kind = a.Kind.ToString(), Text = a.Text, CreatedAtUtc = a.CreatedAtUtc,
    };
}

public record AssignmentSummaryDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public Guid RingId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Summary { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
    public int ProgressPercent { get; init; }
    public DateOnly? DueDate { get; init; }

    public static AssignmentSummaryDto FromEntity(Assignment a) => new()
    {
        Id = a.Id,
        Code = a.Code,
        RingId = a.RingId,
        Title = a.Title,
        Summary = a.Summary,
        Type = a.Type.ToString(),
        Status = a.Status.ToString(),
        Priority = a.Priority.ToString(),
        ProgressPercent = a.ProgressPercent,
        DueDate = a.DueDate,
    };
}

public record AssignmentDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public Guid RingId { get; init; }
    public string RingName { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Summary { get; init; }
    public string? Objective { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
    public string? AssigneeName { get; init; }
    public string? IssuedBy { get; init; }
    public Guid? CompanyId { get; init; }
    public DateTime? AssignedAtUtc { get; init; }
    public DateOnly? StartDate { get; init; }
    public DateOnly? DueDate { get; init; }
    public DateTime? CompletedAtUtc { get; init; }
    public string? Deliverable { get; init; }
    public IReadOnlyList<string> AcceptanceCriteria { get; init; } = Array.Empty<string>();
    public string? Dependencies { get; init; }
    public string? Blockers { get; init; }
    public int ProgressPercent { get; init; }
    public string? NextStep { get; init; }
    public bool ReviewRequired { get; init; }
    public string? ReviewedBy { get; init; }
    public IReadOnlyList<string> References { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
    public string? DomainDataJson { get; init; }
    public IReadOnlyList<AssignmentUpdateDto> Updates { get; init; } = Array.Empty<AssignmentUpdateDto>();

    public static AssignmentDto FromEntity(Assignment a, string ringName) => new()
    {
        Id = a.Id,
        Code = a.Code,
        RingId = a.RingId,
        RingName = ringName,
        Title = a.Title,
        Summary = a.Summary,
        Objective = a.Objective,
        Type = a.Type.ToString(),
        Status = a.Status.ToString(),
        Priority = a.Priority.ToString(),
        AssigneeName = a.AssigneeName,
        IssuedBy = a.IssuedBy,
        CompanyId = a.CompanyId,
        AssignedAtUtc = a.AssignedAtUtc,
        StartDate = a.StartDate,
        DueDate = a.DueDate,
        CompletedAtUtc = a.CompletedAtUtc,
        Deliverable = a.Deliverable,
        AcceptanceCriteria = a.AcceptanceCriteria.ToList(),
        Dependencies = a.Dependencies,
        Blockers = a.Blockers,
        ProgressPercent = a.ProgressPercent,
        NextStep = a.NextStep,
        ReviewRequired = a.ReviewRequired,
        ReviewedBy = a.ReviewedBy,
        References = a.References.ToList(),
        Tags = a.Tags.ToList(),
        DomainDataJson = a.DomainDataJson,
        Updates = a.Updates.Select(u => new AssignmentUpdateDto
        {
            Id = u.Id, Author = u.Author, Body = u.Body, CreatedAtUtc = u.CreatedAtUtc,
        }).ToList(),
    };
}
