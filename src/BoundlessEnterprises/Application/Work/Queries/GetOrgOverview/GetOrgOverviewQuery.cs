using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Work.DTOs;
using BoundlessEnterprises.Domain.Work;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Work.Queries.GetOrgOverview;

/// <summary>
/// The organization-wide overview for the Crown (or platform admin): every ring's
/// status, the companies, cross-cutting totals, and recent activity.
/// </summary>
public record GetOrgOverviewQuery : IRequest<OrgOverviewDto>;

public class GetOrgOverviewHandler : IRequestHandler<GetOrgOverviewQuery, OrgOverviewDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetOrgOverviewHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<OrgOverviewDto> Handle(GetOrgOverviewQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated) throw new ForbiddenAccessException();

        if (!_currentUser.IsPlatformAdmin)
        {
            var crown = await _db.Rings.AsNoTracking()
                .FirstOrDefaultAsync(r => r.Domain == RingDomain.Crown, cancellationToken);
            if (crown?.HolderUserId is null || crown.HolderUserId != _currentUser.UserId)
                throw new ForbiddenAccessException("The organization overview is for the Crown.");
        }

        var rings = await _db.Rings.AsNoTracking().OrderBy(r => r.Domain).ToListAsync(cancellationToken);

        // Small dataset — aggregate assignment states in memory.
        var assignmentStates = await _db.Assignments.AsNoTracking()
            .Select(a => new { a.RingId, a.Status })
            .ToListAsync(cancellationToken);

        var rows = rings.Select(r =>
        {
            var mine = assignmentStates.Where(a => a.RingId == r.Id).ToList();
            return new RingOverviewRow
            {
                Domain = r.Domain.ToString(),
                Slug = r.Domain.ToString().ToLowerInvariant(),
                Name = r.Name,
                HolderName = r.HolderName,
                Held = r.HolderUserId is not null,
                AccentColor = r.AccentColor,
                ActiveAssignments = mine.Count(a => a.Status != AssignmentStatus.Complete && a.Status != AssignmentStatus.Archived),
                InReview = mine.Count(a => a.Status == AssignmentStatus.Review),
                Blocked = mine.Count(a => a.Status == AssignmentStatus.Blocked),
            };
        }).ToList();

        var companies = await _db.Companies.AsNoTracking()
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
            .Select(c => new OrgCompanyRow { Name = c.Name, Status = c.Status.ToString(), Type = c.Type.ToString() })
            .ToListAsync(cancellationToken);

        var activity = await _db.RingActivities.AsNoTracking()
            .OrderByDescending(a => a.CreatedAtUtc)
            .Take(10)
            .ToListAsync(cancellationToken);

        return new OrgOverviewDto
        {
            Totals = new OrgTotalsDto
            {
                TotalRings = rows.Count,
                HeldRings = rows.Count(r => r.Held),
                ActiveAssignments = rows.Sum(r => r.ActiveAssignments),
                InReview = rows.Sum(r => r.InReview),
                Blocked = rows.Sum(r => r.Blocked),
                Companies = companies.Count,
            },
            Rings = rows,
            Companies = companies,
            RecentActivity = activity.Select(RingActivityDto.FromEntity).ToList(),
        };
    }
}
