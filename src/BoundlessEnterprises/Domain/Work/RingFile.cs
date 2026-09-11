using BoundlessEnterprises.Domain.Common;

namespace BoundlessEnterprises.Domain.Work;

/// <summary>Broad classification of an uploaded file, for icons and filtering.</summary>
public enum RingFileKind
{
    Other = 0,
    Audio = 1,
    Image = 2,
    Video = 3,
    Document = 4,
    Archive = 5,
    Project = 6,
}

/// <summary>
/// A file uploaded to a ring's workspace. Optionally tied to an assignment
/// (a "submitted" deliverable). The bytes live in file storage; this row is the
/// catalog entry.
/// </summary>
public class RingFile : AuditableEntity
{
    private RingFile() { }

    private RingFile(Guid ringId, Guid? assignmentId, string fileName, string contentType, long sizeBytes, string storageKey, RingFileKind kind, string uploadedBy)
    {
        RingId = ringId;
        AssignmentId = assignmentId;
        FileName = fileName;
        ContentType = contentType;
        SizeBytes = sizeBytes;
        StorageKey = storageKey;
        Kind = kind;
        UploadedBy = uploadedBy;
    }

    public Guid RingId { get; private set; }
    public Guid? AssignmentId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long SizeBytes { get; private set; }

    /// <summary>Opaque key into file storage (never exposed to clients).</summary>
    public string StorageKey { get; private set; } = string.Empty;

    public RingFileKind Kind { get; private set; }
    public string UploadedBy { get; private set; } = string.Empty;

    public static RingFile Create(Guid ringId, Guid? assignmentId, string fileName, string contentType, long sizeBytes, string storageKey, RingFileKind kind, string uploadedBy)
    {
        if (ringId == Guid.Empty) throw new ArgumentException("RingId is required.", nameof(ringId));
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("File name is required.", nameof(fileName));
        if (string.IsNullOrWhiteSpace(storageKey)) throw new ArgumentException("Storage key is required.", nameof(storageKey));

        return new RingFile(ringId, assignmentId, fileName.Trim(),
            string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
            sizeBytes, storageKey, kind, string.IsNullOrWhiteSpace(uploadedBy) ? "Unknown" : uploadedBy);
    }
}
