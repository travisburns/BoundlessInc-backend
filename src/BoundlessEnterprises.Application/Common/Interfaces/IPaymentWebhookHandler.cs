namespace BoundlessEnterprises.Application.Common.Interfaces;

/// <summary>
/// Processes inbound payment-provider webhooks, reconciling the local ledger
/// (payments, subscriptions) with the provider's authoritative events.
/// </summary>
public interface IPaymentWebhookHandler
{
    /// <summary>
    /// Handles a webhook body. <paramref name="signature"/> is the provider's
    /// signature header used to verify authenticity.
    /// </summary>
    Task HandleAsync(string payload, string? signature, CancellationToken cancellationToken = default);
}
