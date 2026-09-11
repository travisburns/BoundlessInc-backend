using BoundlessEnterprises.Application.Integrations.Commands.CreateIntegration;
using BoundlessEnterprises.Application.Integrations.Queries.GetIntegrations;
using BoundlessEnterprises.Application.Intelligence.Commands.IngestEvent;
using MediatR;

namespace BoundlessEnterprises.Api.Endpoints.Integrations;

/// <summary>
/// Company integration management plus the anonymous event-ingestion endpoint
/// used by operating applications (authenticated by integration API key).
/// </summary>
public sealed class IntegrationsEndpoints : IEndpointModule
{
    public record CreateIntegrationRequest(string Name);
    public record IngestEventRequest(
        string EventType, decimal Revenue, string? Currency, DateTime? OccurredAtUtc, string? ExternalRef);

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/companies/{companyId:guid}/integrations")
            .WithTags("Integrations")
            .RequireAuthorization();

        group.MapGet("/", async (Guid companyId, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetIntegrationsQuery(companyId), ct)))
            .WithName("GetIntegrations")
            .WithSummary("List a company's integrations.");

        group.MapPost("/", async (Guid companyId, CreateIntegrationRequest body, ISender sender, CancellationToken ct) =>
        {
            var dto = await sender.Send(new CreateIntegrationCommand(companyId, body.Name), ct);
            return Results.Created($"/api/companies/{companyId}/integrations/{dto.Integration.Id}", dto);
        })
            .WithName("CreateIntegration")
            .WithSummary("Create an integration and return its API key once.");

        // Anonymous — authenticated by the integration API key in the header.
        app.MapPost("/api/intelligence/events",
            async (IngestEventRequest body, HttpRequest req, ISender sender, CancellationToken ct) =>
            {
                var apiKey = req.Headers["X-Api-Key"].FirstOrDefault() ?? string.Empty;
                var id = await sender.Send(new IngestEventCommand(
                    apiKey, body.EventType, body.Revenue, body.Currency ?? "USD",
                    body.OccurredAtUtc, body.ExternalRef), ct);
                return Results.Ok(new { id });
            })
            .WithTags("Intelligence")
            .WithName("IngestEvent")
            .WithSummary("Ingest a normalized business event (API-key authenticated).");
    }
}
