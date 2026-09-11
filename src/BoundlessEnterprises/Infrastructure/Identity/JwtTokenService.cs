using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Domain.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BoundlessEnterprises.Infrastructure.Identity;

/// <summary>Issues signed JWT access tokens with identity, company, and role claims.</summary>
public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public AccessToken CreateToken(
        User user,
        IEnumerable<UserCompany> memberships,
        IEnumerable<string> roleNames)
    {
        var expires = DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new("name", user.FullName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        if (user.IsPlatformAdmin)
            claims.Add(new Claim("platform_admin", "true"));

        foreach (var companyId in memberships.Select(m => m.CompanyId).Distinct())
            claims.Add(new Claim("company", companyId.ToString()));

        foreach (var role in roleNames.Distinct())
            claims.Add(new Claim(ClaimTypes.Role, role));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return new AccessToken(jwt, expires);
    }
}
