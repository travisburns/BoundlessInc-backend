using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Work.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Work.Queries.GetRings;

/// <summary>Lists all rings (enterprise-level).</summary>
public record GetRingsQuery : IRequest<IReadOnlyList<RingSummaryDto>>;

public class GetRingsHandler : IRequestHandler<GetRingsQuery, IReadOnlyList<RingSummaryDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetRingsHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<RingSummaryDto>> Handle(GetRingsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated) throw new ForbiddenAccessException();

        var rings = await _db.Rings.AsNoTracking()
            .OrderBy(r => r.Domain)
            .ToListAsync(cancellationToken);

        return rings.Select(RingSummaryDto.FromEntity).ToList();
    }
}
