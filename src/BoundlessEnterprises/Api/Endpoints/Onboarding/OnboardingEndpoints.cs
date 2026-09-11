using BoundlessEnterprises.Application.Onboarding.Commands.CreateInvitation;
using BoundlessEnterprises.Application.Onboarding.Commands.SetStepCompletion;
using BoundlessEnterprises.Application.Onboarding.Commands.StartOnboarding;
using BoundlessEnterprises.Application.Onboarding.Queries.GetProcesses;
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
    }
}
