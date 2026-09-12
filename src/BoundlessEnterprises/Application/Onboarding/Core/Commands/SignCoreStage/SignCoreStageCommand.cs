using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Onboarding.Core.DTOs;
using BoundlessEnterprises.Domain.Onboarding.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Onboarding.Core.Commands.SignCoreStage;

/// <summary>Records the current user's read-and-agree signature for one document stage.</summary>
public record SignCoreStageCommand(string StageKey, string TypedName, string Signature)
    : IRequest<CoreOnboardingProgressDto>;

public class SignCoreStageHandler : IRequestHandler<SignCoreStageCommand, CoreOnboardingProgressDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public SignCoreStageHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<CoreOnboardingProgressDto> Handle(SignCoreStageCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is not Guid userId)
            throw new ForbiddenAccessException();

        var stage = CoreOnboardingDocument.FindStage(request.StageKey)
            ?? throw new NotFoundException("Onboarding stage", request.StageKey);

        var progress = await _db.CoreOnboardingProgresses
            .Include(p => p.Signatures)
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (progress is null)
        {
            progress = CoreOnboardingProgress.Start(userId);
            _db.CoreOnboardingProgresses.Add(progress);
        }

        progress.SignStage(
            stage.Key, stage.Order, stage.Title,
            request.TypedName, request.Signature, DateTime.UtcNow);

        await _db.SaveChangesAsync(cancellationToken);

        return CoreOnboardingProgressDto.FromEntity(progress);
    }
}
