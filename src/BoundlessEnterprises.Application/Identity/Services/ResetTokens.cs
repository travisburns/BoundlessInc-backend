using System.Security.Cryptography;

namespace BoundlessEnterprises.Application.Identity.Services;

/// <summary>
/// Generates password-reset tokens and their storable hashes. The raw token is
/// emailed to the user; only its SHA-256 hash is persisted, so a database leak
/// does not expose usable tokens.
/// </summary>
public static class ResetTokens
{
    public static string Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public static string Hash(string token)
    {
        var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
