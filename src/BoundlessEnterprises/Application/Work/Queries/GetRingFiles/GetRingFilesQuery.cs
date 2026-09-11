using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Work.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Work.Queries.GetRingFiles;

/// <summary>Lists a ring's files, optionally scoped to one assignment.</summary>
public record GetRingFilesQuery(Guid RingId, Guid? AssignmentId = null) : IRequest<IReadOnlyList<RingFileDto>>;

public class GetRingFilesHandler : IRequestHandler<GetRingFilesQuery, IReadOnlyList<RingFileDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetRingFilesHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<RingFileDto>> Handle(GetRingFilesQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated) throw new ForbiddenAccessException();

        var query = _db.RingFiles.AsNoTracking().Where(f => f.RingId == request.RingId);
        if (request.AssignmentId is Guid aid)
            query = query.Where(f => f.AssignmentId == aid);

        var files = await query.OrderByDescending(f => f.CreatedAtUtc).ToListAsync(cancellationToken);
        return files.Select(RingFileDto.FromEntity).ToList();
    }
}
