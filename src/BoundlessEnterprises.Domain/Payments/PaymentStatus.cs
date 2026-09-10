namespace BoundlessEnterprises.Domain.Payments;

public enum PaymentStatus
{
    Pending = 0,
    Succeeded = 1,
    Failed = 2,
    Refunded = 3,
}

public enum InvoiceStatus
{
    Draft = 0,
    Open = 1,
    Paid = 2,
    Void = 3,
}

public enum SubscriptionStatus
{
    Active = 0,
    PastDue = 1,
    Canceled = 2,
}

public enum BillingInterval
{
    Monthly = 0,
    Yearly = 1,
}
