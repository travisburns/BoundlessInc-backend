using BoundlessEnterprises.Domain.Documents;

namespace BoundlessEnterprises.Application.Documents.DTOs;

public record DocumentDto
{
    public Guid Id { get; init; }
    public Guid CompanyId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Url { get; init; }
    public int Version { get; init; }
    public bool IsActive { get; init; }
    public DateTime PublishedAtUtc { get; init; }
    public int AssignedCount { get; init; }
    public int AcknowledgedCount { get; init; }

    public static DocumentDto FromEntity(Document d, int assigned = 0, int acknowledged = 0) => new()
    {
        Id = d.Id,
        CompanyId = d.CompanyId,
        Title = d.Title,
        Type = d.Type.ToString(),
        Description = d.Description,
        Url = d.Url,
        Version = d.Version,
        IsActive = d.IsActive,
        PublishedAtUtc = d.PublishedAtUtc,
        AssignedCount = assigned,
        AcknowledgedCount = acknowledged,
    };
}

public record DocumentAssignmentDto
{
    public Guid Id { get; init; }
    public Guid DocumentId { get; init; }
    public string DocumentTitle { get; init; } = string.Empty;
    public Guid EmployeeId { get; init; }
    public string? EmployeeName { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime AssignedAtUtc { get; init; }
    public DateTime? AcknowledgedAtUtc { get; init; }

    public static DocumentAssignmentDto FromEntity(DocumentAssignment a, string documentTitle, string? employeeName) => new()
    {
        Id = a.Id,
        DocumentId = a.DocumentId,
        DocumentTitle = documentTitle,
        EmployeeId = a.EmployeeId,
        EmployeeName = employeeName,
        Status = a.Status.ToString(),
        AssignedAtUtc = a.AssignedAtUtc,
        AcknowledgedAtUtc = a.AcknowledgedAtUtc,
    };
}
