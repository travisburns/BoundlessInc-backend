using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Identity.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Identity.Services;

/// <summary>
/// Assembles a <see cref="UserProfileDto"/> — the user plus their company
/// memberships and role names — from the central database. Shared by login and
/// the "current user" query so the shape stays consistent.
/// </summary>
public static class UserProfileReader
{
    public static async Task<UserProfileDto?> BuildAsync(
        IApplicationDbContext db, Guid userId, CancellationToken ct)
    {
        var user = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null)
            return null;

        // Membership rows (user_company) joined to company for name/slug.
        var memberships = await db.UserCompanies.AsNoTracking()
            .Where(uc => uc.UserId == userId)
            .Join(db.Companies.AsNoTracking(),
                uc => uc.CompanyId,
                c => c.Id,
                (uc, c) => new { uc.Id, uc.CompanyId, uc.IsPrimary, c.Name, c.Slug })
            .ToListAsync(ct);

        var membershipIds = memberships.Select(m => m.Id).ToList();

        // Role names per membership.
        var roleRows = await db.UserCompanies.AsNoTracking()
            .Where(uc => uc.UserId == userId)
            .SelectMany(uc => uc.Roles)
            .Join(db.Roles.AsNoTracking(),
                ur => ur.RoleId,
                r => r.Id,
                (ur, r) => new { ur.UserCompanyId, RoleName = r.Name })
            .ToListAsync(ct);

        var rolesByMembership = roleRows
            .GroupBy(r => r.UserCompanyId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.RoleName).ToArray());

        var companies = memberships
            .Select(m => new MembershipDto
            {
                CompanyId = m.CompanyId,
                CompanyName = m.Name,
                CompanySlug = m.Slug,
                IsPrimary = m.IsPrimary,
                Roles = rolesByMembership.TryGetValue(m.Id, out var roles)
                    ? roles
                    : Array.Empty<string>(),
            })
            .ToList();

        return new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            IsPlatformAdmin = user.IsPlatformAdmin,
            Companies = companies,
        };
    }
}
