using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Work.DTOs;
using BoundlessEnterprises.Domain.Work;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Work.Commands.CreateRing;

public record RingResourceInput(string Label, string? Sublabel, string? Href);

/// <summary>Creates a ring (a domain seat). Platform-admin only. One ring per domain.</summary>
public record CreateRingCommand(
    string Domain,
    string Name,
    string CodePrefix,
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

public sealed class CreateRingValidator : AbstractValidator<CreateRingCommand>
{
    public CreateRingValidator()
    {
        RuleFor(x => x.Domain).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.CodePrefix).NotEmpty().MaximumLength(8);
        RuleFor(x => x.HolderName).NotEmpty().MaximumLength(160);
    }
}

public class CreateRingHandler : IRequestHandler<CreateRingCommand, RingDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public CreateRingHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<RingDto> Handle(CreateRingCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsPlatformAdmin)
            throw new ForbiddenAccessException("Only platform administrators can create rings.");

        if (!Enum.TryParse<RingDomain>(request.Domain, ignoreCase: true, out var domain))
            throw new Common.Exceptions.ValidationException(new Dictionary<string, string[]>
            {
                ["Domain"] = new[] { $"'{request.Domain}' is not a valid ring domain." },
            });

        if (await _db.Rings.AnyAsync(r => r.Domain == domain, cancellationToken))
            throw new ConflictException($"The {domain} ring already exists.");

        var ring = Ring.Create(domain, request.Name, request.CodePrefix, request.HolderName);
        ring.UpdateProfile(request.Name, request.Disciplines, request.HolderName, request.HeroTitle,
            request.HeroSubtitle, request.Focus, request.Motto, request.AccentColor, request.HeroImageUrl);
        ring.SetHolderUser(request.HolderUserId);
        if (request.Resources is { Count: > 0 })
            ring.SetResources(request.Resources.Select(x => new RingResource { Label = x.Label, Sublabel = x.Sublabel, Href = x.Href }));

        _db.Rings.Add(ring);
        await _db.SaveChangesAsync(cancellationToken);

        return RingDto.FromEntity(ring);
    }
}
