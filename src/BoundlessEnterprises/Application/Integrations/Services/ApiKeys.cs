using System.Security.Cryptography;

namespace BoundlessEnterprises.Application.Integrations.Services;

/// <summary>
/// Generates integration API keys. The raw key is returned once at creation;
/// only its SHA-256 hash is stored, with a short non-secret prefix for display.
/// </summary>
public static class ApiKeys
{
    public const string Prefix = "be_live_";

    public static (string Raw, string Hash, string DisplayPrefix) Generate()
    {
        var secret = Convert.ToHexString(RandomNumberGenerator.GetBytes(24)).ToLowerInvariant();
        var raw = Prefix + secret;
        var display = raw[..Math.Min(raw.Length, 14)] + "…";
        return (raw, Hash(raw), display);
    }

    public static string Hash(string apiKey)
    {
        var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(apiKey));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
