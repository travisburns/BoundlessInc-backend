using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Domain.Onboarding;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Onboarding.Commands.DeclineRequest;

/// <summary>Admin: declines a pending onboarding request.</summary>
public record DeclineOnboardingRequestCommand(Guid CompanyId, Guid RequestId) : IRequest;

public class DeclineOnboardingRequestHandler : IRequestHandler<DeclineOnboardingRequestCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _clock;

    public DeclineOnboardingRequestHandler(IApplicationDbContext db, ICurrentUser currentUser, IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task Handle(DeclineOnboardingRequestCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.CanAccessCompany(request.CompanyId))
            throw new ForbiddenAccessException();

        var onboardingRequest = await _db.OnboardingRequests
            .FirstOrDefaultAsync(r => r.Id == request.RequestId && r.CompanyId == request.CompanyId, cancellationToken)
            ?? throw new NotFoundException("OnboardingRequest", request.RequestId);

        if (onboardingRequest.Status != OnboardingRequestStatus.Pending)
            throw new ConflictException("This request has already been reviewed.");

        onboardingRequest.Decline(_clock.UtcNow);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
