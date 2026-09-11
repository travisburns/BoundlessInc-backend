namespace BoundlessEnterprises.Domain.Common;

/// <summary>
/// Marker base for domain events raised by entities and dispatched after
/// a unit of work is persisted. Keeps side effects out of the domain model itself.
/// </summary>
public abstract record DomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
