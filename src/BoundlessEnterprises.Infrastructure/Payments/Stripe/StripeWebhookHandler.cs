using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace BoundlessEnterprises.Infrastructure.Payments.Stripe;

/// <summary>
/// Verifies Stripe webhook signatures and reconciles payment/subscription state
/// from the received events.
/// </summary>
public sealed class StripeWebhookHandler : IPaymentWebhookHandler
{
    private readonly ApplicationDbContext _db;
    private readonly StripeOptions _options;
    private readonly ILogger<StripeWebhookHandler> _logger;

    public StripeWebhookHandler(
        ApplicationDbContext db,
        IOptions<StripeOptions> options,
        ILogger<StripeWebhookHandler> logger)
    {
        _db = db;
        _options = options.Value;
        _logger = logger;
    }

    public async Task HandleAsync(string payload, string? signature, CancellationToken cancellationToken = default)
    {
        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(payload, signature, _options.WebhookSecret);
        }
        catch (StripeException ex)
        {
            _logger.LogWarning(ex, "Rejected Stripe webhook: signature verification failed.");
            throw new UnauthorizedAccessException("Invalid webhook signature.");
        }

        switch (stripeEvent.Type)
        {
            case "payment_intent.succeeded":
                await UpdatePaymentAsync(stripeEvent, succeeded: true, cancellationToken);
                break;
            case "payment_intent.payment_failed":
                await UpdatePaymentAsync(stripeEvent, succeeded: false, cancellationToken);
                break;
            case "customer.subscription.deleted":
                await CancelSubscriptionAsync(stripeEvent, cancellationToken);
                break;
            default:
                _logger.LogInformation("Unhandled Stripe event {Type}", stripeEvent.Type);
                break;
        }
    }

    private async Task UpdatePaymentAsync(Event stripeEvent, bool succeeded, CancellationToken ct)
    {
        if (stripeEvent.Data.Object is not PaymentIntent intent)
            return;

        var payment = await _db.Payments.FirstOrDefaultAsync(p => p.ExternalId == intent.Id, ct);
        if (payment is null)
            return;

        if (succeeded)
            payment.MarkSucceeded(intent.Id, intent.PaymentMethodTypes?.FirstOrDefault(), DateTime.UtcNow);
        else
            payment.MarkFailed(DateTime.UtcNow);

        await _db.SaveChangesAsync(ct);
    }

    private async Task CancelSubscriptionAsync(Event stripeEvent, CancellationToken ct)
    {
        if (stripeEvent.Data.Object is not Subscription sub)
            return;

        var subscription = await _db.Subscriptions.FirstOrDefaultAsync(s => s.ExternalId == sub.Id, ct);
        if (subscription is null)
            return;

        subscription.Cancel(DateTime.UtcNow);
        await _db.SaveChangesAsync(ct);
    }
}
