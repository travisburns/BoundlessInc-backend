using BoundlessEnterprises.Domain.Common;

namespace BoundlessEnterprises.Domain.Work;

/// <summary>A calendar entry on a ring's dashboard — a deadline or a meeting.</summary>
public class RingEvent : AuditableEntity
{
    private RingEvent() { }

    private RingEvent(Guid ringId, string title, DateOnly date, string? timeLabel, string? detail)
    {
        RingId = ringId;
        Title = title;
        Date = date;
        TimeLabel = timeLabel;
        Detail = detail;
    }

    public Guid RingId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public DateOnly Date { get; private set; }

    /// <summary>e.g. "10:00 AM" or "End of Day".</summary>
    public string? TimeLabel { get; private set; }

    /// <summary>e.g. "Internal Review" or "Team Call".</summary>
    public string? Detail { get; private set; }

    public static RingEvent Create(Guid ringId, string title, DateOnly date, string? timeLabel, string? detail)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required.", nameof(title));
        return new RingEvent(ringId, title.Trim(), date, timeLabel, detail);
    }
}
