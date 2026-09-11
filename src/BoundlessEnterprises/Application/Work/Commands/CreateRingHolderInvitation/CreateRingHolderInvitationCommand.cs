using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Onboarding.Services;
using BoundlessEnterprises.Application.Work.DTOs;
using BoundlessEnterprises.Domain.Work;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Work.Commands.CreateRingHolderInvitation;

/// <summary>Admin: invites a person to become a ring's holder. Returns the code once.</summary>
public record CreateRingHolderInvitationCommand(
    Guid RingId, string FirstName, string LastName, string Email, int ExpiresInDays = 14)
    : IRequest<RingHolderInviteCreatedDto>;

public sealed class CreateRingHolderInvitationValidator : AbstractValidator<CreateRingHolderInvitationCommand>
{
    public CreateRingHolderInvitationValidator()
    {
        RuleFor(x => x.RingId).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.ExpiresInDays).InclusiveBetween(1, 90);
    }
}

public class CreateRingHolderInvitationHandler : IRequestHandler<CreateRingHolderInvitationCommand, RingHolderInviteCreatedDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _clock;

    public CreateRingHolderInvitationHandler(IApplicationDbContext db, ICurrentUser currentUser, IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<RingHolderInviteCreatedDto> Handle(CreateRingHolderInvitationCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsPlatformAdmin)
            throw new ForbiddenAccessException("Only platform administrators can invite ring holders.");

        var ring = await _db.Rings.FirstOrDefaultAsync(r => r.Id == request.RingId, cancellationToken)
            ?? throw new NotFoundException("Ring", request.RingId);

        if (ring.HolderUserId is not null)
            throw new ConflictException($"{ring.Name} already has a holder.");

        var (raw, hash) = InviteCodes.Generate();
        var invite = RingHolderInvitation.Create(ring.Id, request.FirstName, request.LastName, request.Email,
            hash, _clock.UtcNow.AddDays(request.ExpiresInDays));

        _db.RingHolderInvitations.Add(invite);
        await _db.SaveChangesAsync(cancellationToken);

        return new RingHolderInviteCreatedDto
        {
            Code = raw,
            RingName = ring.Name,
            RingSlug = ring.Domain.ToString().ToLowerInvariant(),
            Email = invite.Email,
            ExpiresAtUtc = invite.ExpiresAtUtc,
        };
    }
}
