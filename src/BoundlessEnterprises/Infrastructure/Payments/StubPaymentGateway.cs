using BoundlessEnterprises.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace BoundlessEnterprises.Infrastructure.Payments;

/// <summary>
/// Test-mode gateway used when no Stripe credentials are configured. Simulates a
/// successful charge/subscription with a synthetic id so the whole payment flow
/// works end-to-end without contacting Stripe.
/// </summary>
public sealed class StubPaymentGateway : IPaymentGateway
{
    private readonly ILogger<StubPaymentGateway> _logger;

    public StubPaymentGateway(ILogger<StubPaymentGateway> logger)
    {
        _logger = logger;
    }

    public string Provider => "stub";

    public Task<GatewayChargeResult> ChargeAsync(GatewayChargeRequest request, CancellationToken cancellationToken = default)
    {
        var id = $"pi_stub_{Guid.NewGuid():N}";
        _logger.LogInformation(
            "[STUB] Charged {Amount} {Currency} for company {CompanyId} → {Id}",
            request.Amount, request.Currency, request.CompanyId, id);
        return Task.FromResult(new GatewayChargeResult(id, Succeeded: true, Method: "card"));
    }

    public Task<string> CreateSubscriptionAsync(GatewaySubscriptionRequest request, CancellationToken cancellationToken = default)
    {
        var id = $"sub_stub_{Guid.NewGuid():N}";
        _logger.LogInformation(
            "[STUB] Created subscription {Plan} ({Amount} {Currency}) for company {CompanyId} → {Id}",
            request.PlanName, request.Amount, request.Currency, request.CompanyId, id);
        return Task.FromResult(id);
    }
}
