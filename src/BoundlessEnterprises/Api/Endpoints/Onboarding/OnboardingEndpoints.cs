using BoundlessEnterprises.Application.Onboarding.Commands.ApproveRequest;
using BoundlessEnterprises.Application.Onboarding.Commands.CreateInvitation;
using BoundlessEnterprises.Application.Onboarding.Commands.DeclineRequest;
using BoundlessEnterprises.Application.Onboarding.Commands.SetStepCompletion;
using BoundlessEnterprises.Application.Onboarding.Commands.StartOnboarding;
using BoundlessEnterprises.Application.Onboarding.Queries.GetProcesses;
using BoundlessEnterprises.Application.Onboarding.Queries.GetRequests;
using BoundlessEnterprises.Application.Onboarding.Queries.GetTemplates;
using MediatR;

namespace BoundlessEnterprises.Api.Endpoints.Onboarding;

/// <summary>Company-scoped onboarding engine endpoints (portal, authenticated).</summary>
public sealed class OnboardingEndpoints : IEndpointModule
{
    public record StartOnboardingRequest(Guid EmployeeId, Guid TemplateId);
    public record SetStepRequest(bool Completed);
    public record CreateInvitationRequest(
        Guid TemplateId, string Email, string FirstName, string LastName, string? Title, int? ExpiresInDays);
    public record ApproveRequestBody(Guid TemplateId, string? Title, int? ExpiresInDays);

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/companies/{companyId:guid}/onboarding")
            .WithTags("Onboarding")
            .RequireAuthorization();

        group.MapGet("/templates", async (Guid companyId, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetTemplatesQuery(companyId), ct)))
            .WithName("GetOnboardingTemplates")
            .WithSummary("List a company's onboarding templates.");

        group.MapGet("/processes", async (Guid companyId, Guid? employeeId, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetProcessesQuery(companyId, employeeId), ct)))
            .WithName("GetOnboardingProcesses")
            .WithSummary("List onboarding processes, optionally filtered by employee.");

        group.MapPost("/processes", async (Guid companyId, StartOnboardingRequest body, ISender sender, CancellationToken ct) =>
        {
            var dto = await sender.Send(new StartOnboardingCommand(companyId, body.EmployeeId, body.TemplateId), ct);
            return Results.Created($"/api/companies/{companyId}/onboarding/processes/{dto.Id}", dto);
        })
            .WithName("StartOnboarding")
            .WithSummary("Start an onboarding process for an employee from a template.");

        group.MapPatch("/processes/{processId:guid}/steps/{stepId:guid}",
            async (Guid companyId, Guid processId, Guid stepId, SetStepRequest body, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(
                    new SetStepCompletionCommand(companyId, processId, stepId, body.Completed), ct)))
            .WithName("SetOnboardingStep")
            .WithSummary("Mark an onboarding step complete or reopen it.");

        group.MapPost("/invitations", async (Guid companyId, CreateInvitationRequest body, ISender sender, CancellationToken ct) =>
        {
            var dto = await sender.Send(new CreateInvitationCommand(
                companyId, body.TemplateId, body.Email, body.FirstName, body.LastName,
                body.Title, body.ExpiresInDays ?? 14), ct);
            return Results.Created($"/api/companies/{companyId}/onboarding/invitations/{dto.Id}", dto);
        })
            .WithName("CreateOnboardingInvitation")
            .WithSummary("Create a self-serve onboarding invitation for a new hire (returns the code once).");

        group.MapGet("/requests", async (Guid companyId, bool? pendingOnly, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetOnboardingRequestsQuery(companyId, pendingOnly ?? true), ct)))
            .WithName("GetOnboardingRequests")
            .WithSummary("List onboarding requests awaiting review.");

        group.MapPost("/requests/{requestId:guid}/approve",
            async (Guid companyId, Guid requestId, ApproveRequestBody body, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new ApproveOnboardingRequestCommand(
                    companyId, requestId, body.TemplateId, body.Title, body.ExpiresInDays ?? 14), ct)))
            .WithName("ApproveOnboardingRequest")
            .WithSummary("Approve a request and issue an invitation code (returned once).");

        group.MapPost("/requests/{requestId:guid}/decline",
            async (Guid companyId, Guid requestId, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new DeclineOnboardingRequestCommand(companyId, requestId), ct);
                return Results.NoContent();
            })
            .WithName("DeclineOnboardingRequest")
            .WithSummary("Decline a pending onboarding request.");
    }
}
