using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Work.Commands.DeleteRingFile;

/// <summary>Deletes a ring file (row + stored bytes). Holder or admin.</summary>
public record DeleteRingFileCommand(Guid Id) : IRequest;

public class DeleteRingFileHandler : IRequestHandler<DeleteRingFileCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IFileStorage _storage;

    public DeleteRingFileHandler(IApplicationDbContext db, ICurrentUser currentUser, IFileStorage storage)
    {
        _db = db;
        _currentUser = currentUser;
        _storage = storage;
    }

    public async Task Handle(DeleteRingFileCommand request, CancellationToken cancellationToken)
    {
        var file = await _db.RingFiles.FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("RingFile", request.Id);

        var ring = await _db.Rings.FirstOrDefaultAsync(r => r.Id == file.RingId, cancellationToken)
            ?? throw new NotFoundException("Ring", file.RingId);

        if (!WorkAccess.CanManage(_currentUser, ring))
            throw new ForbiddenAccessException();

        _db.RingFiles.Remove(file);
        await _db.SaveChangesAsync(cancellationToken);
        await _storage.DeleteAsync(file.StorageKey, cancellationToken);
    }
}
