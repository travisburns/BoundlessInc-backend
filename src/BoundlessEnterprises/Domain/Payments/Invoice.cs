using BoundlessEnterprises.Domain.Common;

namespace BoundlessEnterprises.Domain.Payments;

/// <summary>A company-scoped invoice with line items.</summary>
public class Invoice : AuditableEntity
{
    private readonly List<InvoiceItem> _items = new();

    private Invoice() { }

    private Invoice(Guid companyId, Guid customerId, string number, string currency)
    {
        CompanyId = companyId;
        CustomerId = customerId;
        Number = number;
        Currency = currency;
        Status = InvoiceStatus.Draft;
        IssuedAtUtc = DateTime.UtcNow;
    }

    public Guid CompanyId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string Number { get; private set; } = string.Empty;
    public string Currency { get; private set; } = "USD";
    public InvoiceStatus Status { get; private set; }
    public DateTime IssuedAtUtc { get; private set; }
    public DateTime? DueAtUtc { get; private set; }
    public DateTime? PaidAtUtc { get; private set; }
    public string? ExternalId { get; private set; }

    public IReadOnlyList<InvoiceItem> Items => _items.AsReadOnly();

    public decimal Total => _items.Sum(i => i.Amount);

    public static Invoice Create(Guid companyId, Guid customerId, string number, string currency = "USD")
    {
        if (companyId == Guid.Empty)
            throw new ArgumentException("CompanyId is required.", nameof(companyId));
        if (customerId == Guid.Empty)
            throw new ArgumentException("CustomerId is required.", nameof(customerId));
        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException("Invoice number is required.", nameof(number));

        return new Invoice(companyId, customerId, number.Trim(), currency);
    }

    public InvoiceItem AddItem(string description, int quantity, decimal unitAmount)
    {
        if (Status != InvoiceStatus.Draft)
            throw new InvalidOperationException("Items can only be added while the invoice is a draft.");

        var item = InvoiceItem.Create(Id, description, quantity, unitAmount);
        _items.Add(item);
        return item;
    }

    public void SetDueDate(DateTime dueUtc) => DueAtUtc = dueUtc;

    public void Issue()
    {
        if (_items.Count == 0)
            throw new InvalidOperationException("Cannot issue an invoice with no items.");
        Status = InvoiceStatus.Open;
    }

    public void MarkPaid(DateTime whenUtc)
    {
        Status = InvoiceStatus.Paid;
        PaidAtUtc = whenUtc;
    }

    public void Void() => Status = InvoiceStatus.Void;

    public void LinkExternal(string externalId) => ExternalId = externalId;
}
