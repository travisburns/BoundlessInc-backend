using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Documents.DTOs;
using BoundlessEnterprises.Domain.Documents;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Documents.Commands.AssignDocument;

/// <summary>Assigns a document to an employee for acknowledgment.</summary>
public record AssignDocumentCommand(Guid CompanyId, Guid DocumentId, Guid EmployeeId)
    : IRequest<DocumentAssignmentDto>;

public class AssignDocumentHandler : IRequestHandler<AssignDocumentCommand, DocumentAssignmentDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public AssignDocumentHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<DocumentAssignmentDto> Handle(AssignDocumentCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.CanAccessCompany(request.CompanyId))
            throw new ForbiddenAccessException();

        var document = await _db.Documents
            .FirstOrDefaultAsync(d => d.Id == request.DocumentId && d.CompanyId == request.CompanyId, cancellationToken)
            ?? throw new NotFoundException("Document", request.DocumentId);

        var employee = await _db.Employees
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId && e.CompanyId == request.CompanyId, cancellationToken)
            ?? throw new NotFoundException("Employee", request.EmployeeId);

        var existing = await _db.DocumentAssignments
            .FirstOrDefaultAsync(a => a.DocumentId == request.DocumentId && a.EmployeeId == request.EmployeeId, cancellationToken);
        if (existing is not null)
            return DocumentAssignmentDto.FromEntity(existing, document.Title, employee.FullName);

        var assignment = DocumentAssignment.Create(request.CompanyId, request.DocumentId, request.EmployeeId);
        _db.DocumentAssignments.Add(assignment);
        await _db.SaveChangesAsync(cancellationToken);

        return DocumentAssignmentDto.FromEntity(assignment, document.Title, employee.FullName);
    }
}
