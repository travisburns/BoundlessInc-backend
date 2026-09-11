using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Work.Queries.GetRingFile;

public record RingFileContent(string StorageKey, string ContentType, string FileName);

/// <summary>Resolves a file's storage info for download. Authenticated.</summary>
public record GetRingFileForDownloadQuery(Guid Id) : IRequest<RingFileContent>;

public class GetRingFileForDownloadHandler : IRequestHandler<GetRingFileForDownloadQuery, RingFileContent>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetRingFileForDownloadHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<RingFileContent> Handle(GetRingFileForDownloadQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated) throw new ForbiddenAccessException();

        var file = await _db.RingFiles.AsNoTracking().FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("RingFile", request.Id);

        return new RingFileContent(file.StorageKey, file.ContentType, file.FileName);
    }
}
