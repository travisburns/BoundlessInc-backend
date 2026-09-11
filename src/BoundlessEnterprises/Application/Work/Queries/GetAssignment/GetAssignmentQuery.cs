using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Work.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Work.Queries.GetAssignment;

/// <summary>Gets a single assignment in full, by id or code.</summary>
public record GetAssignmentQuery(Guid? Id = null, string? Code = null) : IRequest<AssignmentDto>;

public class GetAssignmentHandler : IRequestHandler<GetAssignmentQuery, AssignmentDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetAssignmentHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<AssignmentDto> Handle(GetAssignmentQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated) throw new ForbiddenAccessException();

        var query = _db.Assignments.AsNoTracking().Include(a => a.Updates).AsQueryable();
        var assignment = request.Id is Guid id
            ? await query.FirstOrDefaultAsync(a => a.Id == id, cancellationToken)
            : await query.FirstOrDefaultAsync(a => a.Code == request.Code, cancellationToken);

        if (assignment is null)
            throw new NotFoundException("Assignment", (object?)request.Id ?? request.Code ?? "unknown");

        var ringName = await _db.Rings.AsNoTracking()
            .Where(r => r.Id == assignment.RingId)
            .Select(r => r.Name)
            .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;

        return AssignmentDto.FromEntity(assignment, ringName);
    }
}
