using BoundlessEnterprises.Application.Companies.Queries.GetCompanies;
using MediatR;

namespace BoundlessEnterprises.Api.Endpoints.Companies;

/// <summary>
/// Public and portal endpoints for the enterprise company directory.
/// Routes stay thin: they dispatch to Application use cases and return the result.
/// </summary>
public sealed class CompaniesEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/companies").WithTags("Companies");

        group.MapGet("/", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetCompaniesQuery(PublicOnly: true), ct)))
            .WithName("GetPublicCompanies")
            .WithSummary("List publicly visible companies in the portfolio.");
    }
}
