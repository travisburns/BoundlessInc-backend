namespace BoundlessEnterprises.Infrastructure.Payments;

/// <summary>
/// Bound from the "Stripe" configuration section. When no SecretKey is set the
/// platform falls back to the stub gateway so it runs without credentials.
/// </summary>
public sealed class StripeOptions
{
    public const string SectionName = "Stripe";

    public string? SecretKey { get; set; }
    public string? PublishableKey { get; set; }
    public string? WebhookSecret { get; set; }

    /// <summary>True when a real Stripe secret key is configured.</summary>
    public bool IsConfigured => !string.IsNullOrWhiteSpace(SecretKey);
}
