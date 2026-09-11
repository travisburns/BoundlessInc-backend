using BoundlessEnterprises.Application.Onboarding.Commands.SetInviteStep;
using BoundlessEnterprises.Application.Onboarding.Commands.StartFromInvite;
using BoundlessEnterprises.Application.Onboarding.Commands.SubmitRequest;
using BoundlessEnterprises.Application.Onboarding.Queries.GetInviteByCode;
using MediatR;

namespace BoundlessEnterprises.Api.Endpoints.Onboarding;

/// <summary>
/// Public self-serve onboarding endpoints. These are anonymous: a new hire is
/// authenticated by possession of their invite code, not a JWT, so they can
/// onboard before they have a portal account.
/// </summary>
public sealed class PublicOnboardingEndpoints : IEndpointModule
{
    public record StartRequest(string? FirstName, string? LastName);
    public record SetStepRequest(bool Completed, string? ResponseJson);
    public record OnboardRequestBody(Guid CompanyId, string FirstName, string LastName, string Email, string? DesiredRole);

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        // Public: request to onboard (an admin reviews and approves).
        app.MapPost("/api/onboarding/requests", async (OnboardRequestBody body, ISender sender, CancellationToken ct) =>
        {
            var dto = await sender.Send(new SubmitOnboardingRequestCommand(
                body.CompanyId, body.FirstName, body.LastName, body.Email, body.DesiredRole), ct);
            return Results.Accepted($"/api/onboarding/requests/{dto.Id}", dto);
        })
            .AllowAnonymous()
            .WithTags("Onboarding (self-serve)")
            .WithName("SubmitOnboardingRequest")
            .WithSummary("Request to onboard into a company (creates a pending request).");

        var group = app.MapGroup("/api/onboarding/invite")
            .WithTags("Onboarding (self-serve)")
            .AllowAnonymous();

        group.MapGet("/{code}", async (string code, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetInviteByCodeQuery(code), ct)))
            .WithName("GetOnboardingInvite")
            .WithSummary("Resolve an onboarding invitation by its code.");

        group.MapPost("/{code}/start", async (string code, StartRequest body, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new StartFromInviteCommand(code, body.FirstName, body.LastName), ct)))
            .WithName("StartOnboardingFromInvite")
            .WithSummary("Redeem an invite to create the employee record and start onboarding.");

        group.MapPatch("/{code}/steps/{stepId:guid}",
            async (string code, Guid stepId, SetStepRequest body, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new SetInviteStepCommand(code, stepId, body.Completed, body.ResponseJson), ct)))
            .WithName("SetOnboardingInviteStep")
            .WithSummary("Mark one of the new hire's onboarding steps complete or reopen it.");
    }
}
