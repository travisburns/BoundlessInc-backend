using BoundlessEnterprises.Domain.Payments;

namespace BoundlessEnterprises.Application.Payments.DTOs;

public record InvoiceItemDto
{
    public string Description { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitAmount { get; init; }
    public decimal Amount { get; init; }
}

public record InvoiceDto
{
    public Guid Id { get; init; }
    public Guid CompanyId { get; init; }
    public Guid CustomerId { get; init; }
    public string? CustomerName { get; init; }
    public string Number { get; init; } = string.Empty;
    public string Currency { get; init; } = "USD";
    public string Status { get; init; } = string.Empty;
    public decimal Total { get; init; }
    public DateTime IssuedAtUtc { get; init; }
    public DateTime? DueAtUtc { get; init; }
    public DateTime? PaidAtUtc { get; init; }
    public IReadOnlyList<InvoiceItemDto> Items { get; init; } = Array.Empty<InvoiceItemDto>();

    public static InvoiceDto FromEntity(Invoice i, string? customerName = null) => new()
    {
        Id = i.Id,
        CompanyId = i.CompanyId,
        CustomerId = i.CustomerId,
        CustomerName = customerName,
        Number = i.Number,
        Currency = i.Currency,
        Status = i.Status.ToString(),
        Total = i.Total,
        IssuedAtUtc = i.IssuedAtUtc,
        DueAtUtc = i.DueAtUtc,
        PaidAtUtc = i.PaidAtUtc,
        Items = i.Items.Select(item => new InvoiceItemDto
        {
            Description = item.Description,
            Quantity = item.Quantity,
            UnitAmount = item.UnitAmount,
            Amount = item.Amount,
        }).ToList(),
    };
}

public record PaymentDto
{
    public Guid Id { get; init; }
    public Guid CompanyId { get; init; }
    public Guid? InvoiceId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "USD";
    public string Status { get; init; } = string.Empty;
    public string? Method { get; init; }
    public string? ExternalId { get; init; }
    public DateTime? ProcessedAtUtc { get; init; }

    public static PaymentDto FromEntity(Payment p) => new()
    {
        Id = p.Id,
        CompanyId = p.CompanyId,
        InvoiceId = p.InvoiceId,
        Amount = p.Amount,
        Currency = p.Currency,
        Status = p.Status.ToString(),
        Method = p.Method,
        ExternalId = p.ExternalId,
        ProcessedAtUtc = p.ProcessedAtUtc,
    };
}

public record SubscriptionDto
{
    public Guid Id { get; init; }
    public Guid CompanyId { get; init; }
    public Guid CustomerId { get; init; }
    public string? CustomerName { get; init; }
    public string PlanName { get; init; } = string.Empty;
    public string? Tier { get; init; }
    public decimal Amount { get; init; }
    public decimal MonthlyRecurringRevenue { get; init; }
    public string Currency { get; init; } = "USD";
    public string Interval { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime StartedAtUtc { get; init; }

    public static SubscriptionDto FromEntity(Subscription s, string? customerName = null) => new()
    {
        Id = s.Id,
        CompanyId = s.CompanyId,
        CustomerId = s.CustomerId,
        CustomerName = customerName,
        PlanName = s.PlanName,
        Tier = s.Tier,
        Amount = s.Amount,
        MonthlyRecurringRevenue = s.MonthlyRecurringRevenue,
        Currency = s.Currency,
        Interval = s.Interval.ToString(),
        Status = s.Status.ToString(),
        StartedAtUtc = s.StartedAtUtc,
    };
}
