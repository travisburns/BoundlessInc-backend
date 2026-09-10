using BoundlessEnterprises.Application.Companies.DTOs;
using MediatR;

namespace BoundlessEnterprises.Application.Companies.Queries.GetCompanyBySlug;

public record GetCompanyBySlugQuery(string Slug) : IRequest<CompanyDetailDto>;
