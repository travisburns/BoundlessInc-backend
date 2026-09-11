using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Work.DTOs;
using BoundlessEnterprises.Domain.Work;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Work.Queries.GetRingByDomain;

/// <summary>Gets a single ring by its domain slug (e.g. "resonance").</summary>
public record GetRingByDomainQuery(string Slug) : IRequest<RingDto>;

public class GetRingByDomainHandler : IRequestHandler<GetRingByDomainQuery, RingDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetRingByDomainHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<RingDto> Handle(GetRingByDomainQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated) throw new ForbiddenAccessException();

        if (!Enum.TryParse<RingDomain>(request.Slug, ignoreCase: true, out var domain))
            throw new NotFoundException("Ring", request.Slug);

        var ring = await _db.Rings.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Domain == domain, cancellationToken)
            ?? throw new NotFoundException("Ring", request.Slug);

        return RingDto.FromEntity(ring);
    }
}
