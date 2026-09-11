using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Work.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Work.Queries.GetRingEvents;

/// <summary>Upcoming calendar entries for a ring, soonest first.</summary>
public record GetRingEventsQuery(Guid RingId) : IRequest<IReadOnlyList<RingEventDto>>;

public class GetRingEventsHandler : IRequestHandler<GetRingEventsQuery, IReadOnlyList<RingEventDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetRingEventsHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<RingEventDto>> Handle(GetRingEventsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated) throw new ForbiddenAccessException();

        var events = await _db.RingEvents.AsNoTracking()
            .Where(e => e.RingId == request.RingId)
            .OrderBy(e => e.Date)
            .ToListAsync(cancellationToken);

        return events.Select(RingEventDto.FromEntity).ToList();
    }
}
