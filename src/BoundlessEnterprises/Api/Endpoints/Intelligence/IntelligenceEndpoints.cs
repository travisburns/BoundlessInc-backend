using BoundlessEnterprises.Application.Intelligence.Queries.GetPortfolioOverview;
using MediatR;

namespace BoundlessEnterprises.Api.Endpoints.Intelligence;

/// <summary>
/// Enterprise intelligence — the private cross-company executive view. Access is
/// enforced in the handler (platform administrators only).
/// </summary>
public sealed class IntelligenceEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/intelligence")
            .WithTags("Intelligence")
            .RequireAuthorization();

        group.MapGet("/portfolio", async (int? trendDays, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetPortfolioOverviewQuery(trendDays ?? 14), ct)))
            .WithName("GetPortfolioOverview")
            .WithSummary("Cross-company executive overview (platform admins only).");
    }
}
