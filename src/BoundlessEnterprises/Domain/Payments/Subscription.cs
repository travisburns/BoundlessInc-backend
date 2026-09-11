using BoundlessEnterprises.Domain.Common;

namespace BoundlessEnterprises.Domain.Payments;

/// <summary>A recurring subscription billed to a company's customer.</summary>
public class Subscription : AuditableEntity
{
    private Subscription() { }

    private Subscription(
        Guid companyId, Guid customerId, string planName, string? tier,
        decimal amount, string currency, BillingInterval interval)
    {
        CompanyId = companyId;
        CustomerId = customerId;
        PlanName = planName;
        Tier = tier;
        Amount = amount;
        Currency = currency;
        Interval = interval;
        Status = SubscriptionStatus.Active;
        StartedAtUtc = DateTime.UtcNow;
    }

    public Guid CompanyId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string PlanName { get; private set; } = string.Empty;
    public string? Tier { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "USD";
    public BillingInterval Interval { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public DateTime StartedAtUtc { get; private set; }
    public DateTime? CanceledAtUtc { get; private set; }
    public string? ExternalId { get; private set; }

    /// <summary>Monthly recurring revenue contribution, normalized from the interval.</summary>
    public decimal MonthlyRecurringRevenue =>
        Interval == BillingInterval.Yearly ? Math.Round(Amount / 12m, 2) : Amount;

    public static Subscription Create(
        Guid companyId, Guid customerId, string planName, string? tier,
        decimal amount, BillingInterval interval, string currency = "USD")
    {
        if (companyId == Guid.Empty)
            throw new ArgumentException("CompanyId is required.", nameof(companyId));
        if (string.IsNullOrWhiteSpace(planName))
            throw new ArgumentException("Plan name is required.", nameof(planName));
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));

        return new Subscription(companyId, customerId, planName.Trim(), tier, amount, currency, interval);
    }

    public void Cancel(DateTime whenUtc)
    {
        Status = SubscriptionStatus.Canceled;
        CanceledAtUtc = whenUtc;
    }

    public void MarkPastDue() => Status = SubscriptionStatus.PastDue;
    public void Reactivate() => Status = SubscriptionStatus.Active;
    public void LinkExternal(string externalId) => ExternalId = externalId;
}
