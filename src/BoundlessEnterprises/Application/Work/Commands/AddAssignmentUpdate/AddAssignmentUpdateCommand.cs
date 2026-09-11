using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Work.DTOs;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Work.Commands.AddAssignmentUpdate;

/// <summary>Appends a work-log entry to an assignment. Admin or ring holder.</summary>
public record AddAssignmentUpdateCommand(Guid Id, string Body) : IRequest<AssignmentDto>;

public sealed class AddAssignmentUpdateValidator : AbstractValidator<AddAssignmentUpdateCommand>
{
    public AddAssignmentUpdateValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Body).NotEmpty().MaximumLength(4000);
    }
}

public class AddAssignmentUpdateHandler : IRequestHandler<AddAssignmentUpdateCommand, AssignmentDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _clock;

    public AddAssignmentUpdateHandler(IApplicationDbContext db, ICurrentUser currentUser, IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<AssignmentDto> Handle(AddAssignmentUpdateCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _db.Assignments.Include(a => a.Updates)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Assignment", request.Id);

        var ring = await _db.Rings.FirstOrDefaultAsync(r => r.Id == assignment.RingId, cancellationToken)
            ?? throw new NotFoundException("Ring", assignment.RingId);

        if (!WorkAccess.CanManage(_currentUser, ring))
            throw new ForbiddenAccessException();

        var author = _currentUser.Email ?? ring.HolderName;
        assignment.AddUpdate(author, request.Body, _clock.UtcNow);
        _db.RingActivities.Add(Domain.Work.RingActivity.Create(
            ring.Id, Domain.Work.RingActivityKind.Update, $"Update posted on {assignment.Code}", _clock.UtcNow));

        await _db.SaveChangesAsync(cancellationToken);
        return AssignmentDto.FromEntity(assignment, ring.Name);
    }
}
