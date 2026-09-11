using BoundlessEnterprises.Domain.Common;

namespace BoundlessEnterprises.Domain.Payments;

/// <summary>
/// A payment attempt routed through the company's payment configuration. Tracks
/// the gateway id and outcome so the company ledger stays authoritative.
/// </summary>
public class Payment : AuditableEntity
{
    private Payment() { }

    private Payment(Guid companyId, Guid customerId, Guid? invoiceId, decimal amount, string currency)
    {
        CompanyId = companyId;
        CustomerId = customerId;
        InvoiceId = invoiceId;
        Amount = amount;
        Currency = currency;
        Status = PaymentStatus.Pending;
    }

    public Guid CompanyId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid? InvoiceId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "USD";
    public PaymentStatus Status { get; private set; }

    /// <summary>Gateway payment id (e.g. Stripe pi_…).</summary>
    public string? ExternalId { get; private set; }
    public string? Method { get; private set; }
    public DateTime? ProcessedAtUtc { get; private set; }

    public static Payment Create(Guid companyId, Guid customerId, Guid? invoiceId, decimal amount, string currency = "USD")
    {
        if (companyId == Guid.Empty)
            throw new ArgumentException("CompanyId is required.", nameof(companyId));
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive.", nameof(amount));

        return new Payment(companyId, customerId, invoiceId, amount, currency);
    }

    public void MarkSucceeded(string externalId, string? method, DateTime whenUtc)
    {
        Status = PaymentStatus.Succeeded;
        ExternalId = externalId;
        Method = method;
        ProcessedAtUtc = whenUtc;
    }

    public void MarkFailed(DateTime whenUtc)
    {
        Status = PaymentStatus.Failed;
        ProcessedAtUtc = whenUtc;
    }

    public void Refund() => Status = PaymentStatus.Refunded;
}
