using BoundlessEnterprises.Application.Companies.Commands.CreateCompany;
using BoundlessEnterprises.Application.Companies.Commands.UpdateCompany;
using BoundlessEnterprises.Application.Companies.Queries.GetAllCompanies;
using BoundlessEnterprises.Application.Companies.Queries.GetCompanies;
using BoundlessEnterprises.Application.Companies.Queries.GetCompanyBySlug;
using MediatR;

namespace BoundlessEnterprises.Api.Endpoints.Companies;

/// <summary>
/// Public and portal endpoints for the enterprise company directory.
/// Routes stay thin: they dispatch to Application use cases and return the result.
/// </summary>
public sealed class CompaniesEndpoints : IEndpointModule
{
    public record CompanyBody(
        string Name, string? Slug, string? Code, string Type, string Status,
        string? Tagline, string? Description, string? Sector, string? AccentColor,
        string? WebsiteUrl, string? Domain, string? ContactEmail, string? ContactPhone,
        bool SupportsEmployeeLogin, bool SupportsPayments, int SortOrder);

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/companies").WithTags("Companies");

        group.MapGet("/", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetCompaniesQuery(PublicOnly: true), ct)))
            .WithName("GetPublicCompanies")
            .WithSummary("List publicly visible companies in the portfolio.");

        // Admin: every company (all statuses) with full editable detail.
        group.MapGet("/all", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetAllCompaniesQuery(), ct)))
            .RequireAuthorization()
            .WithName("GetAllCompanies")
            .WithSummary("List every company with full detail (platform admin).");

        group.MapGet("/{slug}", async (string slug, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetCompanyBySlugQuery(slug), ct)))
            .WithName("GetCompanyBySlug")
            .WithSummary("Get a single publicly visible company by its slug.");

        group.MapPost("/", async (CompanyBody body, ISender sender, CancellationToken ct) =>
        {
            var dto = await sender.Send(new CreateCompanyCommand(
                body.Name, body.Slug, body.Code, body.Type, body.Status, body.Tagline,
                body.Description, body.Sector, body.AccentColor, body.WebsiteUrl, body.Domain,
                body.ContactEmail, body.ContactPhone, body.SupportsEmployeeLogin,
                body.SupportsPayments, body.SortOrder), ct);
            return Results.Created($"/api/companies/{dto.Slug}", dto);
        })
            .RequireAuthorization()
            .WithName("CreateCompany")
            .WithSummary("Create a company in the portfolio (platform admin).");

        group.MapPut("/{id:guid}", async (Guid id, CompanyBody body, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new UpdateCompanyCommand(
                id, body.Name, body.Status, body.Tagline, body.Description, body.Sector,
                body.AccentColor, body.WebsiteUrl, body.Domain, body.ContactEmail,
                body.ContactPhone, body.SupportsEmployeeLogin, body.SupportsPayments,
                body.SortOrder), ct)))
            .RequireAuthorization()
            .WithName("UpdateCompany")
            .WithSummary("Update a company's profile, capabilities, and status (platform admin).");
    }
}
