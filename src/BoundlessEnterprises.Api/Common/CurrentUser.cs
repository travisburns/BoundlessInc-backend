using System.Security.Claims;
using BoundlessEnterprises.Application.Common.Interfaces;

namespace BoundlessEnterprises.Api.Common;

/// <summary>
/// Resolves the caller from the current HTTP context's claims principal.
/// Returns an unauthenticated view when there is no valid token.
/// </summary>
public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUser(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId
    {
        get
        {
            var value = Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? Principal?.FindFirstValue("sub");
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public string? Email =>
        Principal?.FindFirstValue(ClaimTypes.Email)
        ?? Principal?.FindFirstValue("email");

    public IReadOnlyCollection<Guid> CompanyIds =>
        Principal?.FindAll("company")
            .Select(c => Guid.TryParse(c.Value, out var id) ? id : Guid.Empty)
            .Where(id => id != Guid.Empty)
            .ToArray()
        ?? Array.Empty<Guid>();
}
