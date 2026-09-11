using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Work.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Work.Commands.SetRingHolder;

/// <summary>
/// Admin: assigns an existing user (by email, or the caller when omitted) as a
/// ring's holder, or clears the holder when <see cref="Clear"/> is true.
/// </summary>
public record SetRingHolderCommand(Guid RingId, string? Email, bool Clear = false) : IRequest<RingDto>;

public class SetRingHolderHandler : IRequestHandler<SetRingHolderCommand, RingDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public SetRingHolderHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<RingDto> Handle(SetRingHolderCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsPlatformAdmin)
            throw new ForbiddenAccessException("Only platform administrators can assign ring holders.");

        var ring = await _db.Rings.FirstOrDefaultAsync(r => r.Id == request.RingId, cancellationToken)
            ?? throw new NotFoundException("Ring", request.RingId);

        if (request.Clear)
        {
            ring.ClearHolder();
            await _db.SaveChangesAsync(cancellationToken);
            return RingDto.FromEntity(ring);
        }

        var user = string.IsNullOrWhiteSpace(request.Email)
            ? await _db.Users.FirstOrDefaultAsync(u => u.Id == _currentUser.UserId, cancellationToken)
            : await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email!.Trim().ToLowerInvariant(), cancellationToken);

        if (user is null)
            throw new NotFoundException("User", request.Email ?? "current");

        ring.AssignHolder(user.FullName, user.Id);
        await _db.SaveChangesAsync(cancellationToken);
        return RingDto.FromEntity(ring);
    }
}
