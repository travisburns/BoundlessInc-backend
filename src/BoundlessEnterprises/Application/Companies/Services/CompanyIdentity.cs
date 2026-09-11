using System.Text;

namespace BoundlessEnterprises.Application.Companies.Services;

/// <summary>
/// Derives a URL slug and a short business code from a company name, so admins
/// can just type a name and get sensible defaults they can still override.
/// </summary>
public static class CompanyIdentity
{
    public static string Slugify(string name)
    {
        var sb = new StringBuilder();
        var lastHyphen = false;
        foreach (var ch in name.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(ch))
            {
                sb.Append(ch);
                lastHyphen = false;
            }
            else if (!lastHyphen && sb.Length > 0)
            {
                sb.Append('-');
                lastHyphen = true;
            }
        }
        return sb.ToString().Trim('-');
    }

    /// <summary>A base code like "FIRE" from "Firefin" — the numeric suffix is added by the caller to keep it unique.</summary>
    public static string BaseCode(string name)
    {
        var letters = new string(name.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        if (letters.Length == 0) return "CO";
        return letters.Length <= 4 ? letters : letters[..4];
    }
}
