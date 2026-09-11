using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Onboarding.Services;
using BoundlessEnterprises.Application.Work.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Work.Queries.GetRingHolderInvite;

/// <summary>Public: resolve a ring-holder invitation by its code for the accept page.</summary>
public record GetRingHolderInviteQuery(string Code) : IRequest<RingHolderInviteDto>;

public class GetRingHolderInviteHandler : IRequestHandler<GetRingHolderInviteQuery, RingHolderInviteDto>
{
    private readonly IApplicationDbContext _db;

    public GetRingHolderInviteHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<RingHolderInviteDto> Handle(GetRingHolderInviteQuery request, CancellationToken cancellationToken)
    {
        var hash = InviteCodes.Hash(request.Code);

        var invite = await _db.RingHolderInvitations
            .FirstOrDefaultAsync(i => i.CodeHash == hash, cancellationToken)
            ?? throw new NotFoundException("RingHolderInvitation", request.Code);

        var ring = await _db.Rings.FirstOrDefaultAsync(r => r.Id == invite.RingId, cancellationToken)
            ?? throw new NotFoundException("Ring", invite.RingId);

        return new RingHolderInviteDto
        {
            RingName = ring.Name,
            RingSlug = ring.Domain.ToString().ToLowerInvariant(),
            Disciplines = ring.Disciplines,
            AccentColor = ring.AccentColor,
            FirstName = invite.FirstName,
            LastName = invite.LastName,
            Email = invite.Email,
            Status = invite.Status.ToString(),
        };
    }
}
