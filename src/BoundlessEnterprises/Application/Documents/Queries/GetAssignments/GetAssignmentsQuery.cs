using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Documents.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Documents.Queries.GetAssignments;

public record GetAssignmentsQuery(Guid CompanyId, Guid? EmployeeId = null)
    : IRequest<IReadOnlyList<DocumentAssignmentDto>>;

public class GetAssignmentsHandler : IRequestHandler<GetAssignmentsQuery, IReadOnlyList<DocumentAssignmentDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetAssignmentsHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<DocumentAssignmentDto>> Handle(GetAssignmentsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.CanAccessCompany(request.CompanyId))
            throw new ForbiddenAccessException();

        var query = _db.DocumentAssignments
            .AsNoTracking()
            .Where(a => a.CompanyId == request.CompanyId);

        if (request.EmployeeId is { } employeeId)
            query = query.Where(a => a.EmployeeId == employeeId);

        var assignments = await query
            .OrderByDescending(a => a.AssignedAtUtc)
            .ToListAsync(cancellationToken);

        var docTitles = await _db.Documents
            .AsNoTracking()
            .Where(d => d.CompanyId == request.CompanyId)
            .ToDictionaryAsync(d => d.Id, d => d.Title, cancellationToken);

        var employeeNames = await _db.Employees
            .AsNoTracking()
            .Where(e => e.CompanyId == request.CompanyId)
            .ToDictionaryAsync(e => e.Id, e => e.FirstName + " " + e.LastName, cancellationToken);

        return assignments
            .Select(a => DocumentAssignmentDto.FromEntity(
                a,
                docTitles.GetValueOrDefault(a.DocumentId, "Document"),
                employeeNames.GetValueOrDefault(a.EmployeeId)))
            .ToList();
    }
}
