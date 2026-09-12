using BoundlessEnterprises.Application.Onboarding.Core.Commands.CompleteCoreOnboarding;
using BoundlessEnterprises.Application.Onboarding.Core.Commands.SignCoreStage;
using BoundlessEnterprises.Application.Onboarding.Core.Queries.GetCoreOnboarding;
using MediatR;

namespace BoundlessEnterprises.Api.Endpoints.Onboarding;

/// <summary>
/// Boundless core onboarding document (Part 1): the hire reads each exact page
/// of the document, types their name and signs it. All authenticated — the
/// signatures are recorded against the current user.
/// </summary>
public sealed class CoreOnboardingEndpoints : IEndpointModule
{
    public record SignBody(string TypedName, string Signature);
    public record CompleteBody(string TypedName, string Signature);

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var core = app.MapGroup("/api/onboarding/core")
            .WithTags("Onboarding — Core Document")
            .RequireAuthorization();

        core.MapGet("/", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetCoreOnboardingQuery(), ct)))
            .WithName("GetCoreOnboarding")
            .WithSummary("The core onboarding document and the current user's signing progress.");

        core.MapPost("/stages/{key}/sign", async (string key, SignBody b, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new SignCoreStageCommand(key, b.TypedName, b.Signature), ct)))
            .WithName("SignCoreOnboardingStage")
            .WithSummary("Record the hire's read-and-agree signature for one document stage.");

        core.MapPost("/complete", async (CompleteBody b, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new CompleteCoreOnboardingCommand(b.TypedName, b.Signature), ct)))
            .WithName("CompleteCoreOnboarding")
            .WithSummary("Record agreement to the onboarding document as a whole (all stages must be signed).");
    }
}
