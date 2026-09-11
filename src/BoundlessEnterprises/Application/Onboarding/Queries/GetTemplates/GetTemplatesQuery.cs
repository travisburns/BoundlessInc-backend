using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Onboarding.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Onboarding.Queries.GetTemplates;

public record GetTemplatesQuery(Guid CompanyId) : IRequest<IReadOnlyList<OnboardingTemplateDto>>;

public class GetTemplatesHandler : IRequestHandler<GetTemplatesQuery, IReadOnlyList<OnboardingTemplateDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetTemplatesHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<OnboardingTemplateDto>> Handle(GetTemplatesQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.CanAccessCompany(request.CompanyId))
            throw new ForbiddenAccessException();

        var templates = await _db.OnboardingTemplates
            .AsNoTracking()
            .Include(t => t.Steps)
            .Where(t => t.CompanyId == request.CompanyId && t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);

        return templates.Select(OnboardingTemplateDto.FromEntity).ToList();
    }
}
