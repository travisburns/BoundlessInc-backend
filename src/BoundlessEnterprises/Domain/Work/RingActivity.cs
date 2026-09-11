using BoundlessEnterprises.Domain.Common;

namespace BoundlessEnterprises.Domain.Work;

/// <summary>The kind of activity — drives the icon in the ring's activity feed.</summary>
public enum RingActivityKind
{
    General = 0,
    FileUpload = 1,
    Update = 2,
    Comment = 3,
    Draft = 4,
    Status = 5,
}

/// <summary>An entry in a ring's recent-activity feed.</summary>
public class RingActivity : Entity
{
    private RingActivity() { }

    private RingActivity(Guid ringId, RingActivityKind kind, string text, DateTime whenUtc)
    {
        RingId = ringId;
        Kind = kind;
        Text = text;
        CreatedAtUtc = whenUtc;
    }

    public Guid RingId { get; private set; }
    public RingActivityKind Kind { get; private set; }
    public string Text { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }

    public static RingActivity Create(Guid ringId, RingActivityKind kind, string text, DateTime whenUtc)
    {
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Text is required.", nameof(text));
        return new RingActivity(ringId, kind, text.Trim(), whenUtc);
    }
}
