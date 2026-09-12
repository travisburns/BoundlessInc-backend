using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Onboarding.Core.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Onboarding.Core.Queries.GetCoreOnboarding;

/// <summary>The core onboarding document plus the current user's signing progress.</summary>
public record GetCoreOnboardingQuery : IRequest<CoreOnboardingDocumentDto>;

public class GetCoreOnboardingHandler : IRequestHandler<GetCoreOnboardingQuery, CoreOnboardingDocumentDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetCoreOnboardingHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<CoreOnboardingDocumentDto> Handle(GetCoreOnboardingQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is not Guid userId)
            throw new ForbiddenAccessException();

        var progress = await _db.CoreOnboardingProgresses.AsNoTracking()
            .Include(p => p.Signatures)
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        var stages = CoreOnboardingDocument.Stages
            .Select(s => new CoreOnboardingStageDto(s.Key, s.Order, s.PageNumber, s.Title, s.Subtitle, s.ImageUrl))
            .ToList();

        return new CoreOnboardingDocumentDto(
            CoreOnboardingDocument.Version,
            stages,
            progress is null ? CoreOnboardingProgressDto.Empty : CoreOnboardingProgressDto.FromEntity(progress));
    }
}
