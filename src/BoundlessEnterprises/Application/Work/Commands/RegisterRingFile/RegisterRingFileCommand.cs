using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Work.DTOs;
using BoundlessEnterprises.Domain.Work;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Work.Commands.RegisterRingFile;

/// <summary>
/// Catalogs a file that has already been written to storage. The endpoint saves
/// the bytes then sends this to record the metadata. Holder or admin only.
/// </summary>
public record RegisterRingFileCommand(
    Guid RingId, Guid? AssignmentId, string FileName, string ContentType, long SizeBytes, string StorageKey)
    : IRequest<RingFileDto>;

public class RegisterRingFileHandler : IRequestHandler<RegisterRingFileCommand, RingFileDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IFileStorage _storage;

    public RegisterRingFileHandler(IApplicationDbContext db, ICurrentUser currentUser, IFileStorage storage)
    {
        _db = db;
        _currentUser = currentUser;
        _storage = storage;
    }

    public async Task<RingFileDto> Handle(RegisterRingFileCommand request, CancellationToken cancellationToken)
    {
        var ring = await _db.Rings.FirstOrDefaultAsync(r => r.Id == request.RingId, cancellationToken);
        if (ring is null || !WorkAccess.CanManage(_currentUser, ring))
        {
            // Don't leave orphaned bytes behind on a rejected upload.
            await _storage.DeleteAsync(request.StorageKey, cancellationToken);
            if (ring is null) throw new NotFoundException("Ring", request.RingId);
            throw new ForbiddenAccessException();
        }

        if (request.AssignmentId is Guid aid &&
            !await _db.Assignments.AnyAsync(a => a.Id == aid && a.RingId == ring.Id, cancellationToken))
        {
            await _storage.DeleteAsync(request.StorageKey, cancellationToken);
            throw new NotFoundException("Assignment", aid);
        }

        var file = RingFile.Create(request.RingId, request.AssignmentId, request.FileName, request.ContentType,
            request.SizeBytes, request.StorageKey, RingFileKinds.FromFileName(request.FileName),
            _currentUser.Email ?? ring.HolderName);

        _db.RingFiles.Add(file);
        await _db.SaveChangesAsync(cancellationToken);

        return RingFileDto.FromEntity(file);
    }
}
