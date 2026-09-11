using BoundlessEnterprises.Domain.Common;

namespace BoundlessEnterprises.Domain.Payments;

/// <summary>A single line on an invoice.</summary>
public class InvoiceItem : Entity
{
    private InvoiceItem() { }

    internal InvoiceItem(Guid invoiceId, string description, int quantity, decimal unitAmount)
    {
        InvoiceId = invoiceId;
        Description = description;
        Quantity = quantity;
        UnitAmount = unitAmount;
    }

    public Guid InvoiceId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitAmount { get; private set; }

    public decimal Amount => Quantity * UnitAmount;

    internal static InvoiceItem Create(Guid invoiceId, string description, int quantity, decimal unitAmount)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Item description is required.", nameof(description));
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.", nameof(quantity));
        if (unitAmount < 0)
            throw new ArgumentException("Unit amount cannot be negative.", nameof(unitAmount));

        return new InvoiceItem(invoiceId, description.Trim(), quantity, unitAmount);
    }
}
