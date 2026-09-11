using BoundlessEnterprises.Domain.Common;

namespace BoundlessEnterprises.Domain.Documents;

/// <summary>
/// A company document — policy, agreement, onboarding doc, or internal resource.
/// Documents belong to a company and can be assigned to employees for
/// acknowledgment.
/// </summary>
public class Document : AuditableEntity
{
    private Document() { }

    private Document(Guid companyId, string title, DocumentType type, string? description, string? url)
    {
        CompanyId = companyId;
        Title = title;
        Type = type;
        Description = description;
        Url = url;
        Version = 1;
        IsActive = true;
        PublishedAtUtc = DateTime.UtcNow;
    }

    public Guid CompanyId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public DocumentType Type { get; private set; }
    public string? Description { get; private set; }

    /// <summary>Location of the document content (URL or storage key).</summary>
    public string? Url { get; private set; }

    public int Version { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime PublishedAtUtc { get; private set; }

    public static Document Create(Guid companyId, string title, DocumentType type, string? description = null, string? url = null)
    {
        if (companyId == Guid.Empty)
            throw new ArgumentException("CompanyId is required.", nameof(companyId));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Document title is required.", nameof(title));

        return new Document(companyId, title.Trim(), type, description, url);
    }

    public void Update(string title, string? description, string? url)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Document title is required.", nameof(title));
        Title = title.Trim();
        Description = description;
        Url = url;
        Version++;
    }

    public void Archive() => IsActive = false;
    public void Restore() => IsActive = true;
}
