using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Companies.DTOs;
using BoundlessEnterprises.Domain.Companies;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Companies.Queries.GetCompanies;

public class GetCompaniesHandler
    : IRequestHandler<GetCompaniesQuery, IReadOnlyList<CompanySummaryDto>>
{
    private readonly IApplicationDbContext _db;

    public GetCompaniesHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<CompanySummaryDto>> Handle(
        GetCompaniesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _db.Companies.AsNoTracking();

        if (request.PublicOnly)
        {
            query = query.Where(c =>
                c.Status == CompanyStatus.Active || c.Status == CompanyStatus.ComingSoon);
        }

        return await query
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .Select(c => CompanySummaryDto.FromEntity(c))
            .ToListAsync(cancellationToken);
    }
}
