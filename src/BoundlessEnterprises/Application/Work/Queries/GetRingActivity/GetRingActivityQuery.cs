using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Work.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Work.Queries.GetRingActivity;

/// <summary>Recent activity for a ring, newest first.</summary>
public record GetRingActivityQuery(Guid RingId, int Take = 8) : IRequest<IReadOnlyList<RingActivityDto>>;

public class GetRingActivityHandler : IRequestHandler<GetRingActivityQuery, IReadOnlyList<RingActivityDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetRingActivityHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<RingActivityDto>> Handle(GetRingActivityQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated) throw new ForbiddenAccessException();

        var items = await _db.RingActivities.AsNoTracking()
            .Where(a => a.RingId == request.RingId)
            .OrderByDescending(a => a.CreatedAtUtc)
            .Take(Math.Clamp(request.Take, 1, 50))
            .ToListAsync(cancellationToken);

        return items.Select(RingActivityDto.FromEntity).ToList();
    }
}
