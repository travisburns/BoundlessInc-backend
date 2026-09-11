using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Work.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Work.Queries.GetMyRing;

/// <summary>Returns the ring held by the current user, or null if they don't hold one.</summary>
public record GetMyRingQuery : IRequest<RingDto?>;

public class GetMyRingHandler : IRequestHandler<GetMyRingQuery, RingDto?>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetMyRingHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<RingDto?> Handle(GetMyRingQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is not Guid userId)
            throw new ForbiddenAccessException();

        var ring = await _db.Rings.AsNoTracking()
            .FirstOrDefaultAsync(r => r.HolderUserId == userId, cancellationToken);

        return ring is null ? null : RingDto.FromEntity(ring);
    }
}
