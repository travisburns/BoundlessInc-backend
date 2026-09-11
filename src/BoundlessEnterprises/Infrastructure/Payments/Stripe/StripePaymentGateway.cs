using BoundlessEnterprises.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace BoundlessEnterprises.Infrastructure.Payments.Stripe;

/// <summary>
/// Real Stripe gateway. Used only when a Stripe secret key is configured;
/// otherwise the platform uses <see cref="StubPaymentGateway"/>.
/// </summary>
public sealed class StripePaymentGateway : IPaymentGateway
{
    private readonly ILogger<StripePaymentGateway> _logger;

    public StripePaymentGateway(IOptions<StripeOptions> options, ILogger<StripePaymentGateway> logger)
    {
        _logger = logger;
        StripeConfiguration.ApiKey = options.Value.SecretKey;
    }

    public string Provider => "stripe";

    public async Task<GatewayChargeResult> ChargeAsync(GatewayChargeRequest request, CancellationToken cancellationToken = default)
    {
        var service = new PaymentIntentService();
        var intent = await service.CreateAsync(new PaymentIntentCreateOptions
        {
            Amount = ToMinorUnits(request.Amount),
            Currency = request.Currency.ToLowerInvariant(),
            Description = request.Description,
            ReceiptEmail = request.CustomerEmail,
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true,
                AllowRedirects = "never",
            },
            Metadata = new Dictionary<string, string> { ["companyId"] = request.CompanyId.ToString() },
        }, cancellationToken: cancellationToken);

        var succeeded = intent.Status is "succeeded" or "requires_capture";
        _logger.LogInformation("Stripe PaymentIntent {Id} status {Status}", intent.Id, intent.Status);
        return new GatewayChargeResult(intent.Id, succeeded, intent.PaymentMethodTypes?.FirstOrDefault());
    }

    public async Task<string> CreateSubscriptionAsync(GatewaySubscriptionRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await new CustomerService().CreateAsync(new CustomerCreateOptions
        {
            Email = request.CustomerEmail,
            Metadata = new Dictionary<string, string> { ["companyId"] = request.CompanyId.ToString() },
        }, cancellationToken: cancellationToken);

        var price = await new PriceService().CreateAsync(new PriceCreateOptions
        {
            UnitAmount = ToMinorUnits(request.Amount),
            Currency = request.Currency.ToLowerInvariant(),
            Recurring = new PriceRecurringOptions { Interval = "month" },
            ProductData = new PriceProductDataOptions { Name = request.PlanName },
        }, cancellationToken: cancellationToken);

        var subscription = await new SubscriptionService().CreateAsync(new SubscriptionCreateOptions
        {
            Customer = customer.Id,
            Items = new List<SubscriptionItemOptions> { new() { Price = price.Id } },
            Metadata = new Dictionary<string, string> { ["companyId"] = request.CompanyId.ToString() },
        }, cancellationToken: cancellationToken);

        _logger.LogInformation("Stripe Subscription {Id} created", subscription.Id);
        return subscription.Id;
    }

    private static long ToMinorUnits(decimal amount) => (long)Math.Round(amount * 100m);
}
