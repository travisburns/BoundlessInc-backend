using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Onboarding.DTOs;
using BoundlessEnterprises.Domain.Onboarding;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Onboarding.Queries.GetRequests;

/// <summary>Admin: lists onboarding requests for a company, newest first (pending by default).</summary>
public record GetOnboardingRequestsQuery(Guid CompanyId, bool PendingOnly = true)
    : IRequest<IReadOnlyList<OnboardingRequestDto>>;

public class GetOnboardingRequestsHandler : IRequestHandler<GetOnboardingRequestsQuery, IReadOnlyList<OnboardingRequestDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetOnboardingRequestsHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<OnboardingRequestDto>> Handle(GetOnboardingRequestsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.CanAccessCompany(request.CompanyId))
            throw new ForbiddenAccessException();

        var query = _db.OnboardingRequests.AsNoTracking()
            .Where(r => r.CompanyId == request.CompanyId);

        if (request.PendingOnly)
            query = query.Where(r => r.Status == OnboardingRequestStatus.Pending);

        return await query
            .OrderByDescending(r => r.CreatedAtUtc)
            .Select(r => new OnboardingRequestDto
            {
                Id = r.Id,
                CompanyId = r.CompanyId,
                FirstName = r.FirstName,
                LastName = r.LastName,
                Email = r.Email,
                DesiredRole = r.DesiredRole,
                Status = r.Status.ToString(),
                CreatedAtUtc = r.CreatedAtUtc,
            })
            .ToListAsync(cancellationToken);
    }
}
