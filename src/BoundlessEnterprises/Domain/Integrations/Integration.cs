using BoundlessEnterprises.Domain.Common;

namespace BoundlessEnterprises.Domain.Integrations;

public enum IntegrationStatus
{
    Active = 0,
    Disabled = 1,
}

/// <summary>
/// A company operating application's connection to the central platform. It
/// holds the API key (hashed) that authenticates inbound events/webhooks, so
/// company systems push normalized data upward without the center querying them.
/// </summary>
public class Integration : AuditableEntity
{
    private Integration() { }

    private Integration(Guid companyId, string name, string apiKeyHash, string apiKeyPrefix)
    {
        CompanyId = companyId;
        Name = name;
        ApiKeyHash = apiKeyHash;
        ApiKeyPrefix = apiKeyPrefix;
        Status = IntegrationStatus.Active;
    }

    public Guid CompanyId { get; private set; }
    public string Name { get; private set; } = string.Empty;

    /// <summary>SHA-256 hash of the API key; the raw key is shown only once.</summary>
    public string ApiKeyHash { get; private set; } = string.Empty;

    /// <summary>Non-secret prefix for display (e.g. "be_live_ab12…").</summary>
    public string ApiKeyPrefix { get; private set; } = string.Empty;

    public IntegrationStatus Status { get; private set; }
    public DateTime? LastEventAtUtc { get; private set; }
    public int EventCount { get; private set; }

    public static Integration Create(Guid companyId, string name, string apiKeyHash, string apiKeyPrefix)
    {
        if (companyId == Guid.Empty)
            throw new ArgumentException("CompanyId is required.", nameof(companyId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Integration name is required.", nameof(name));

        return new Integration(companyId, name.Trim(), apiKeyHash, apiKeyPrefix);
    }

    public void RecordEvent(DateTime whenUtc)
    {
        LastEventAtUtc = whenUtc;
        EventCount++;
    }

    public void Disable() => Status = IntegrationStatus.Disabled;
    public void Enable() => Status = IntegrationStatus.Active;
    public bool IsActive => Status == IntegrationStatus.Active;
}
