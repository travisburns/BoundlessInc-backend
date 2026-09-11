using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Documents.DTOs;
using BoundlessEnterprises.Domain.Documents;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Documents.Queries.GetDocuments;

public record GetDocumentsQuery(Guid CompanyId) : IRequest<IReadOnlyList<DocumentDto>>;

public class GetDocumentsHandler : IRequestHandler<GetDocumentsQuery, IReadOnlyList<DocumentDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetDocumentsHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<DocumentDto>> Handle(GetDocumentsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.CanAccessCompany(request.CompanyId))
            throw new ForbiddenAccessException();

        var documents = await _db.Documents
            .AsNoTracking()
            .Where(d => d.CompanyId == request.CompanyId)
            .OrderByDescending(d => d.PublishedAtUtc)
            .ToListAsync(cancellationToken);

        var counts = await _db.DocumentAssignments
            .AsNoTracking()
            .Where(a => a.CompanyId == request.CompanyId)
            .GroupBy(a => a.DocumentId)
            .Select(g => new
            {
                DocumentId = g.Key,
                Assigned = g.Count(),
                Acknowledged = g.Count(a => a.Status == AssignmentStatus.Acknowledged),
            })
            .ToDictionaryAsync(x => x.DocumentId, cancellationToken);

        return documents
            .Select(d =>
            {
                counts.TryGetValue(d.Id, out var c);
                return DocumentDto.FromEntity(d, c?.Assigned ?? 0, c?.Acknowledged ?? 0);
            })
            .ToList();
    }
}
