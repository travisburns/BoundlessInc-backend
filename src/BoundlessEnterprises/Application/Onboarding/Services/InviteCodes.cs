using System.Security.Cryptography;

namespace BoundlessEnterprises.Application.Onboarding.Services;

/// <summary>
/// Generates human-typable onboarding invite codes (e.g. ONB-7K3F-9Q2P) and their
/// storable hashes. The raw code is shown once at creation; only its SHA-256 hash
/// is persisted, so a leaked database yields no usable codes.
/// </summary>
public static class InviteCodes
{
    // Unambiguous alphabet (no O/0, I/1).
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public static (string Raw, string Hash) Generate()
    {
        var g1 = RandomBlock(4);
        var g2 = RandomBlock(4);
        var raw = $"ONB-{g1}-{g2}";
        return (raw, Hash(raw));
    }

    public static string Normalize(string code) => code.Trim().ToUpperInvariant();

    public static string Hash(string code)
    {
        var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(Normalize(code)));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string RandomBlock(int length)
    {
        var chars = new char[length];
        for (var i = 0; i < length; i++)
            chars[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
        return new string(chars);
    }
}
