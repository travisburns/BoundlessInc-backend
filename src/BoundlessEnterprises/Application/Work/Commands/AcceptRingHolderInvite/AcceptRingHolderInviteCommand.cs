using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Identity.DTOs;
using BoundlessEnterprises.Application.Identity.Services;
using BoundlessEnterprises.Application.Onboarding.Services;
using BoundlessEnterprises.Domain.Identity;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Work.Commands.AcceptRingHolderInvite;

/// <summary>
/// Public: a person redeems a ring-holder invitation, setting a password. This
/// creates their login, makes them the ring's holder, and logs them in.
/// </summary>
public record AcceptRingHolderInviteCommand(string Code, string Password, string? FirstName, string? LastName)
    : IRequest<AuthResultDto>;

public sealed class AcceptRingHolderInviteValidator : AbstractValidator<AcceptRingHolderInviteCommand>
{
    public AcceptRingHolderInviteValidator()
    {
        RuleFor(x => x.Code).NotEmpty();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
    }
}

public class AcceptRingHolderInviteHandler : IRequestHandler<AcceptRingHolderInviteCommand, AuthResultDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwt;
    private readonly IDateTimeProvider _clock;

    public AcceptRingHolderInviteHandler(IApplicationDbContext db, IPasswordHasher passwordHasher, IJwtTokenService jwt, IDateTimeProvider clock)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwt = jwt;
        _clock = clock;
    }

    public async Task<AuthResultDto> Handle(AcceptRingHolderInviteCommand request, CancellationToken cancellationToken)
    {
        var hash = InviteCodes.Hash(request.Code);
        var invite = await _db.RingHolderInvitations.FirstOrDefaultAsync(i => i.CodeHash == hash, cancellationToken)
            ?? throw new NotFoundException("RingHolderInvitation", request.Code);

        if (!invite.IsRedeemable(_clock.UtcNow))
            throw new ConflictException("This invitation is no longer valid.");

        var ring = await _db.Rings.FirstOrDefaultAsync(r => r.Id == invite.RingId, cancellationToken)
            ?? throw new NotFoundException("Ring", invite.RingId);

        if (ring.HolderUserId is not null)
            throw new ConflictException($"{ring.Name} already has a holder.");

        if (await _db.Users.AnyAsync(u => u.Email == invite.Email, cancellationToken))
            throw new ConflictException("An account with this email already exists. Please sign in instead.");

        var firstName = string.IsNullOrWhiteSpace(request.FirstName) ? invite.FirstName : request.FirstName!.Trim();
        var lastName = string.IsNullOrWhiteSpace(request.LastName) ? invite.LastName : request.LastName!.Trim();

        var user = User.Create(invite.Email, _passwordHasher.Hash(request.Password), firstName, lastName);
        var holding = await _db.Companies.FirstOrDefaultAsync(c => c.Code == "BE-000", cancellationToken);
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.NormalizedName == RoleNames.Executive.ToUpperInvariant(), cancellationToken);
        if (holding is not null)
        {
            var membership = user.AddMembership(holding.Id, isPrimary: true);
            if (role is not null) membership.AssignRole(role.Id);
        }
        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        ring.AssignHolder(user.FullName, user.Id);
        invite.Accept(user.Id);
        await _db.SaveChangesAsync(cancellationToken);

        var profile = await UserProfileReader.BuildAsync(_db, user.Id, cancellationToken)
            ?? throw new ConflictException("Could not build the new account.");
        var roleNames = profile.Companies.SelectMany(c => c.Roles).Distinct();
        var token = _jwt.CreateToken(user, user.Memberships, roleNames);

        return new AuthResultDto { Token = token.Token, ExpiresAtUtc = token.ExpiresAtUtc, User = profile };
    }
}
