using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Onboarding.Core.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Onboarding.Core.Commands.CompleteCoreOnboarding;

/// <summary>
/// Records the hire's agreement to the onboarding document as a whole. Every
/// stage must already be signed; this is the total-document half of the audit
/// trail that sits on top of the per-stage signatures.
/// </summary>
public record CompleteCoreOnboardingCommand(string TypedName, string Signature)
    : IRequest<CoreOnboardingProgressDto>;

public class CompleteCoreOnboardingHandler : IRequestHandler<CompleteCoreOnboardingCommand, CoreOnboardingProgressDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public CompleteCoreOnboardingHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<CoreOnboardingProgressDto> Handle(CompleteCoreOnboardingCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is not Guid userId)
            throw new ForbiddenAccessException();

        var progress = await _db.CoreOnboardingProgresses
            .Include(p => p.Signatures)
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (progress is null)
            throw new ConflictException("Sign each section of the document before agreeing to the whole.");

        var missing = CoreOnboardingDocument.RequiredStageKeys
            .Where(k => progress.Signatures.All(s => s.StageKey != k))
            .ToList();
        if (missing.Count > 0)
            throw new ConflictException(
                $"{missing.Count} section(s) still need your signature before you can agree to the whole document.");

        progress.CompleteDocument(
            request.TypedName, request.Signature,
            CoreOnboardingDocument.RequiredStageKeys, DateTime.UtcNow);

        await _db.SaveChangesAsync(cancellationToken);

        return CoreOnboardingProgressDto.FromEntity(progress);
    }
}
