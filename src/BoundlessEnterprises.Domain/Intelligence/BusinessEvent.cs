using BoundlessEnterprises.Domain.Common;

namespace BoundlessEnterprises.Domain.Intelligence;

/// <summary>
/// A normalized cross-company event ingested from an operating application
/// (e.g. OrderCompleted, SubscriptionCreated). Reduced to a common language of
/// business, type, time, and value so the intelligence layer can compare across
/// companies without absorbing each one's domain model.
/// </summary>
public class BusinessEvent : Entity
{
    private BusinessEvent() { }

    private BusinessEvent(
        Guid companyId, Guid? integrationId, string eventType,
        decimal revenue, string currency, DateTime occurredAtUtc, string? externalRef)
    {
        CompanyId = companyId;
        IntegrationId = integrationId;
        EventType = eventType;
        Revenue = revenue;
        Currency = currency;
        OccurredAtUtc = occurredAtUtc;
        ExternalRef = externalRef;
        ReceivedAtUtc = DateTime.UtcNow;
    }

    /// <summary>The business the event belongs to (BusinessId).</summary>
    public Guid CompanyId { get; private set; }
    public Guid? IntegrationId { get; private set; }
    public string EventType { get; private set; } = string.Empty;

    /// <summary>Normalized revenue impact of the event (0 when not revenue-bearing).</summary>
    public decimal Revenue { get; private set; }
    public string Currency { get; private set; } = "USD";

    public DateTime OccurredAtUtc { get; private set; }
    public DateTime ReceivedAtUtc { get; private set; }

    /// <summary>The source system's identifier for the event (idempotency/audit).</summary>
    public string? ExternalRef { get; private set; }

    public static BusinessEvent Create(
        Guid companyId, Guid? integrationId, string eventType,
        decimal revenue, string currency, DateTime occurredAtUtc, string? externalRef)
    {
        if (companyId == Guid.Empty)
            throw new ArgumentException("CompanyId is required.", nameof(companyId));
        if (string.IsNullOrWhiteSpace(eventType))
            throw new ArgumentException("Event type is required.", nameof(eventType));

        return new BusinessEvent(companyId, integrationId, eventType.Trim(), revenue,
            string.IsNullOrWhiteSpace(currency) ? "USD" : currency.Trim().ToUpperInvariant(),
            occurredAtUtc, externalRef);
    }
}
