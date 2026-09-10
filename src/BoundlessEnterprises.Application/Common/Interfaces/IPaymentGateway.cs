namespace BoundlessEnterprises.Application.Common.Interfaces;

/// <summary>A charge request routed to a company's payment configuration.</summary>
public record GatewayChargeRequest(
    Guid CompanyId,
    decimal Amount,
    string Currency,
    string CustomerEmail,
    string Description);

/// <summary>Outcome of a gateway charge.</summary>
public record GatewayChargeResult(string ExternalId, bool Succeeded, string? Method);

public record GatewaySubscriptionRequest(
    Guid CompanyId,
    decimal Amount,
    string Currency,
    string CustomerEmail,
    string PlanName);

/// <summary>
/// Abstraction over the external payment provider (Stripe). The concrete
/// implementation is selected per environment; a stub is used when no real
/// credentials are configured, so the platform runs end-to-end without keys.
/// </summary>
public interface IPaymentGateway
{
    /// <summary>Provider name (e.g. "stripe" or "stub"), surfaced for diagnostics.</summary>
    string Provider { get; }

    Task<GatewayChargeResult> ChargeAsync(GatewayChargeRequest request, CancellationToken cancellationToken = default);

    Task<string> CreateSubscriptionAsync(GatewaySubscriptionRequest request, CancellationToken cancellationToken = default);
}
