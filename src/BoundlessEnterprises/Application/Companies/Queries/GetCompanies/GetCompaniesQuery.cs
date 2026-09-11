using BoundlessEnterprises.Application.Companies.DTOs;
using MediatR;

namespace BoundlessEnterprises.Application.Companies.Queries.GetCompanies;

/// <summary>
/// Returns companies in the portfolio. When <see cref="PublicOnly"/> is true only
/// publicly visible companies (Active / ComingSoon) are returned, ordered for display.
/// </summary>
public record GetCompaniesQuery(bool PublicOnly = true) : IRequest<IReadOnlyList<CompanySummaryDto>>;
