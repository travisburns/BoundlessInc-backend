using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Companies.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Companies.Queries.GetAllCompanies;

/// <summary>
/// Admin listing of every company (all statuses) with full editable detail.
/// Platform-admin only.
/// </summary>
public record GetAllCompaniesQuery : IRequest<IReadOnlyList<CompanyDetailDto>>;

public class GetAllCompaniesHandler : IRequestHandler<GetAllCompaniesQuery, IReadOnlyList<CompanyDetailDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetAllCompaniesHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<CompanyDetailDto>> Handle(GetAllCompaniesQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsPlatformAdmin)
            throw new ForbiddenAccessException("Only platform administrators can list all companies.");

        return await _db.Companies.AsNoTracking()
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
            .Select(c => CompanyDetailDto.FromEntity(c))
            .ToListAsync(cancellationToken);
    }
}
