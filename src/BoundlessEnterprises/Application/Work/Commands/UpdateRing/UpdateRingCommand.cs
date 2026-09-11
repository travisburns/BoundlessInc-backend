using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Work.Commands.CreateRing;
using BoundlessEnterprises.Application.Work.DTOs;
using BoundlessEnterprises.Domain.Work;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Work.Commands.UpdateRing;

/// <summary>Updates a ring's profile, holder, and resources. Platform-admin only.</summary>
public record UpdateRingCommand(
    Guid Id,
    string Name,
    string HolderName,
    Guid? HolderUserId,
    string? Disciplines,
    string? HeroTitle,
    string? HeroSubtitle,
    string? Focus,
    string? Motto,
    string? AccentColor,
    string? HeroImageUrl,
    IReadOnlyList<RingResourceInput>? Resources) : IRequest<RingDto>;

public sealed class UpdateRingValidator : AbstractValidator<UpdateRingCommand>
{
    public UpdateRingValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.HolderName).NotEmpty().MaximumLength(160);
    }
}

public class UpdateRingHandler : IRequestHandler<UpdateRingCommand, RingDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public UpdateRingHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<RingDto> Handle(UpdateRingCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsPlatformAdmin)
            throw new ForbiddenAccessException("Only platform administrators can edit rings.");

        var ring = await _db.Rings.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Ring", request.Id);

        ring.UpdateProfile(request.Name, request.Disciplines, request.HolderName, request.HeroTitle,
            request.HeroSubtitle, request.Focus, request.Motto, request.AccentColor, request.HeroImageUrl);
        ring.SetHolderUser(request.HolderUserId);
        ring.SetResources((request.Resources ?? Array.Empty<RingResourceInput>())
            .Select(x => new RingResource { Label = x.Label, Sublabel = x.Sublabel, Href = x.Href }));

        await _db.SaveChangesAsync(cancellationToken);
        return RingDto.FromEntity(ring);
    }
}
