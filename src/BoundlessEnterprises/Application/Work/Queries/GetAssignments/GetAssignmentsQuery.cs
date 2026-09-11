using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Work.DTOs;
using BoundlessEnterprises.Domain.Work;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Work.Queries.GetAssignments;

/// <summary>Lists a ring's assignments, optionally filtered by status.</summary>
public record GetAssignmentsQuery(Guid RingId, string? Status = null) : IRequest<IReadOnlyList<AssignmentSummaryDto>>;

public class GetAssignmentsHandler : IRequestHandler<GetAssignmentsQuery, IReadOnlyList<AssignmentSummaryDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetAssignmentsHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<AssignmentSummaryDto>> Handle(GetAssignmentsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated) throw new ForbiddenAccessException();

        var query = _db.Assignments.AsNoTracking().Where(a => a.RingId == request.RingId);

        if (!string.IsNullOrWhiteSpace(request.Status)
            && Enum.TryParse<AssignmentStatus>(request.Status, ignoreCase: true, out var status))
        {
            query = query.Where(a => a.Status == status);
        }

        var items = await query
            .OrderBy(a => a.Status)
            .ThenBy(a => a.DueDate)
            .ToListAsync(cancellationToken);

        return items.Select(AssignmentSummaryDto.FromEntity).ToList();
    }
}
