using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Integrations.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Integrations.Queries.GetIntegrations;

public record GetIntegrationsQuery(Guid CompanyId) : IRequest<IReadOnlyList<IntegrationDto>>;

public class GetIntegrationsHandler : IRequestHandler<GetIntegrationsQuery, IReadOnlyList<IntegrationDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetIntegrationsHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<IntegrationDto>> Handle(GetIntegrationsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.CanAccessCompany(request.CompanyId))
            throw new ForbiddenAccessException();

        var integrations = await _db.Integrations
            .AsNoTracking()
            .Where(i => i.CompanyId == request.CompanyId)
            .OrderByDescending(i => i.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return integrations.Select(IntegrationDto.FromEntity).ToList();
    }
}
