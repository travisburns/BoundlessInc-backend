using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Documents.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Documents.Commands.AcknowledgeAssignment;

/// <summary>Marks a document assignment acknowledged by the employee.</summary>
public record AcknowledgeAssignmentCommand(Guid CompanyId, Guid AssignmentId)
    : IRequest<DocumentAssignmentDto>;

public class AcknowledgeAssignmentHandler : IRequestHandler<AcknowledgeAssignmentCommand, DocumentAssignmentDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _clock;

    public AcknowledgeAssignmentHandler(IApplicationDbContext db, ICurrentUser currentUser, IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<DocumentAssignmentDto> Handle(AcknowledgeAssignmentCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.CanAccessCompany(request.CompanyId))
            throw new ForbiddenAccessException();

        var assignment = await _db.DocumentAssignments
            .FirstOrDefaultAsync(a => a.Id == request.AssignmentId && a.CompanyId == request.CompanyId, cancellationToken)
            ?? throw new NotFoundException("DocumentAssignment", request.AssignmentId);

        assignment.Acknowledge(_clock.UtcNow);
        await _db.SaveChangesAsync(cancellationToken);

        var document = await _db.Documents.FirstOrDefaultAsync(d => d.Id == assignment.DocumentId, cancellationToken);
        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Id == assignment.EmployeeId, cancellationToken);

        return DocumentAssignmentDto.FromEntity(assignment, document?.Title ?? "Document", employee?.FullName);
    }
}
