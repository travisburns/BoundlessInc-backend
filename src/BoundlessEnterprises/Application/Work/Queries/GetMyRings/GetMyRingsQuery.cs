using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Work.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Work.Queries.GetMyRings;

/// <summary>All rings held by the current user (a person may hold several).</summary>
public record GetMyRingsQuery : IRequest<IReadOnlyList<RingSummaryDto>>;

public class GetMyRingsHandler : IRequestHandler<GetMyRingsQuery, IReadOnlyList<RingSummaryDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetMyRingsHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<RingSummaryDto>> Handle(GetMyRingsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is not Guid userId)
            throw new ForbiddenAccessException();

        var rings = await _db.Rings.AsNoTracking()
            .Where(r => r.HolderUserId == userId)
            .OrderBy(r => r.Domain)
            .ToListAsync(cancellationToken);

        return rings.Select(RingSummaryDto.FromEntity).ToList();
    }
}
