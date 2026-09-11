using System.Text.Json;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BoundlessEnterprises.Infrastructure.Payments;

/// <summary>
/// Test-mode webhook handler. Accepts a simple JSON body
/// { "externalId": "...", "succeeded": true } to simulate provider callbacks so
/// the reconciliation path is exercisable without Stripe.
/// </summary>
public sealed class StubPaymentWebhookHandler : IPaymentWebhookHandler
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<StubPaymentWebhookHandler> _logger;

    public StubPaymentWebhookHandler(ApplicationDbContext db, ILogger<StubPaymentWebhookHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    private sealed record StubEvent(string? ExternalId, bool Succeeded);

    public async Task HandleAsync(string payload, string? signature, CancellationToken cancellationToken = default)
    {
        StubEvent? evt;
        try
        {
            evt = JsonSerializer.Deserialize<StubEvent>(payload,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException)
        {
            _logger.LogWarning("[STUB] Ignored malformed webhook payload.");
            return;
        }

        if (evt?.ExternalId is null)
            return;

        var payment = await _db.Payments.FirstOrDefaultAsync(p => p.ExternalId == evt.ExternalId, cancellationToken);
        if (payment is null)
            return;

        if (evt.Succeeded)
            payment.MarkSucceeded(evt.ExternalId, "card", DateTime.UtcNow);
        else
            payment.MarkFailed(DateTime.UtcNow);

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("[STUB] Reconciled payment {ExternalId} succeeded={Succeeded}", evt.ExternalId, evt.Succeeded);
    }
}
