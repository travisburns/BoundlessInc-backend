using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Companies.DTOs;
using BoundlessEnterprises.Domain.Companies;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Companies.Queries.GetCompanyBySlug;

public class GetCompanyBySlugHandler : IRequestHandler<GetCompanyBySlugQuery, CompanyDetailDto>
{
    private readonly IApplicationDbContext _db;

    public GetCompanyBySlugHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<CompanyDetailDto> Handle(GetCompanyBySlugQuery request, CancellationToken cancellationToken)
    {
        var slug = request.Slug.Trim().ToLowerInvariant();

        var company = await _db.Companies.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken);

        // Archived / inactive companies are not exposed on the public site.
        if (company is null ||
            company.Status is CompanyStatus.Archived or CompanyStatus.Inactive)
        {
            throw new NotFoundException("Company", request.Slug);
        }

        return CompanyDetailDto.FromEntity(company);
    }
}
