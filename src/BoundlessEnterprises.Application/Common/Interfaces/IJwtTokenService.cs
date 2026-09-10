using BoundlessEnterprises.Domain.Identity;

namespace BoundlessEnterprises.Application.Common.Interfaces;

public record AccessToken(string Token, DateTime ExpiresAtUtc);

/// <summary>Issues signed JWT access tokens for authenticated users.</summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Creates an access token embedding the user's identity and the company ids
    /// and roles they carry, so authorization can be enforced from the token.
    /// </summary>
    AccessToken CreateToken(User user, IEnumerable<UserCompany> memberships, IEnumerable<string> roleNames);
}
