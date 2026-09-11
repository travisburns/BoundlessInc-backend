using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Work.DTOs;
using BoundlessEnterprises.Domain.Work;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Work.Commands.CreateRingEvent;

/// <summary>Adds a calendar entry to a ring. Holder or admin.</summary>
public record CreateRingEventCommand(Guid RingId, string Title, DateOnly Date, string? TimeLabel, string? Detail)
    : IRequest<RingEventDto>;

public sealed class CreateRingEventValidator : AbstractValidator<CreateRingEventCommand>
{
    public CreateRingEventValidator()
    {
        RuleFor(x => x.RingId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TimeLabel).MaximumLength(60);
        RuleFor(x => x.Detail).MaximumLength(200);
    }
}

public class CreateRingEventHandler : IRequestHandler<CreateRingEventCommand, RingEventDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public CreateRingEventHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<RingEventDto> Handle(CreateRingEventCommand request, CancellationToken cancellationToken)
    {
        var ring = await _db.Rings.FirstOrDefaultAsync(r => r.Id == request.RingId, cancellationToken)
            ?? throw new NotFoundException("Ring", request.RingId);

        if (!WorkAccess.CanManage(_currentUser, ring))
            throw new ForbiddenAccessException();

        var ev = RingEvent.Create(ring.Id, request.Title, request.Date, request.TimeLabel, request.Detail);
        _db.RingEvents.Add(ev);
        await _db.SaveChangesAsync(cancellationToken);

        return RingEventDto.FromEntity(ev);
    }
}
