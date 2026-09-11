using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Work.DTOs;
using BoundlessEnterprises.Domain.Work;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Work.Commands.SetAssignmentStatus;

/// <summary>Moves an assignment through its lifecycle and optionally sets progress. Admin or ring holder.</summary>
public record SetAssignmentStatusCommand(Guid Id, string Status, int? ProgressPercent) : IRequest<AssignmentDto>;

public class SetAssignmentStatusHandler : IRequestHandler<SetAssignmentStatusCommand, AssignmentDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _clock;

    public SetAssignmentStatusHandler(IApplicationDbContext db, ICurrentUser currentUser, IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<AssignmentDto> Handle(SetAssignmentStatusCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _db.Assignments.Include(a => a.Updates)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Assignment", request.Id);

        var ring = await _db.Rings.FirstOrDefaultAsync(r => r.Id == assignment.RingId, cancellationToken)
            ?? throw new NotFoundException("Ring", assignment.RingId);

        if (!WorkAccess.CanManage(_currentUser, ring))
            throw new ForbiddenAccessException();

        if (!Enum.TryParse<AssignmentStatus>(request.Status, ignoreCase: true, out var status))
            throw new Common.Exceptions.ValidationException(new Dictionary<string, string[]>
            {
                ["Status"] = new[] { $"'{request.Status}' is not a valid status." },
            });

        assignment.SetStatus(status, _clock.UtcNow);
        if (request.ProgressPercent is int pct)
            assignment.SetProgress(pct);

        await _db.SaveChangesAsync(cancellationToken);
        return AssignmentDto.FromEntity(assignment, ring.Name);
    }
}
