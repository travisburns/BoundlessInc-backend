using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Work.Commands.AcceptRingHolderInvite;
using BoundlessEnterprises.Application.Work.Commands.AddAssignmentUpdate;
using BoundlessEnterprises.Application.Work.Commands.CreateAssignment;
using BoundlessEnterprises.Application.Work.Commands.CreateRing;
using BoundlessEnterprises.Application.Work.Commands.CreateRingEvent;
using BoundlessEnterprises.Application.Work.Commands.CreateRingHolderInvitation;
using BoundlessEnterprises.Application.Work.Commands.DeleteRingFile;
using BoundlessEnterprises.Application.Work.Commands.RegisterRingFile;
using BoundlessEnterprises.Application.Work.Commands.SetAssignmentStatus;
using BoundlessEnterprises.Application.Work.Commands.SetRingHolder;
using BoundlessEnterprises.Application.Work.Commands.UpdateAssignment;
using BoundlessEnterprises.Application.Work.Commands.UpdateRing;
using BoundlessEnterprises.Application.Work.Queries.GetOrgOverview;
using BoundlessEnterprises.Application.Work.Queries.GetRingFile;
using BoundlessEnterprises.Application.Work.Queries.GetRingFiles;
using BoundlessEnterprises.Application.Work.Queries.GetRingHolderInvite;
using Microsoft.AspNetCore.Mvc;
using BoundlessEnterprises.Application.Work.Queries.GetAssignment;
using BoundlessEnterprises.Application.Work.Queries.GetAssignments;
using BoundlessEnterprises.Application.Work.Queries.GetMyRing;
using BoundlessEnterprises.Application.Work.Queries.GetMyRings;
using BoundlessEnterprises.Application.Work.Queries.GetRingActivity;
using BoundlessEnterprises.Application.Work.Queries.GetRingByDomain;
using BoundlessEnterprises.Application.Work.Queries.GetRingEvents;
using BoundlessEnterprises.Application.Work.Queries.GetRings;
using MediatR;

namespace BoundlessEnterprises.Api.Endpoints.Work;

/// <summary>Ring work-management endpoints: rings and their assignments. All authenticated.</summary>
public sealed class WorkEndpoints : IEndpointModule
{
    public record RingBody(
        string Domain, string Name, string CodePrefix, string HolderName, Guid? HolderUserId,
        string? Disciplines, string? HeroTitle, string? HeroSubtitle, string? Focus, string? Motto,
        string? AccentColor, string? HeroImageUrl, List<RingResourceInput>? Resources);

    public record AssignmentBody(
        string Title, string Type, string Priority, string? Summary, string? Objective,
        string? AssigneeName, string? IssuedBy, Guid? CompanyId, DateOnly? StartDate, DateOnly? DueDate,
        string? Deliverable, List<string>? AcceptanceCriteria, string? Dependencies, string? Blockers,
        string? NextStep, bool ReviewRequired, List<string>? References, List<string>? Tags, string? DomainDataJson);

    public record StatusBody(string Status, int? ProgressPercent);
    public record UpdateBody(string Body);
    public record HolderInviteBody(string FirstName, string LastName, string Email, int? ExpiresInDays);
    public record SetHolderBody(string? Email, bool? Clear);
    public record AcceptHolderBody(string Password, string? FirstName, string? LastName);
    public record EventBody(string Title, DateOnly Date, string? TimeLabel, string? Detail);

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        // Public: ring-holder onboarding (redeem code → set password → hold the ring).
        app.MapGet("/api/ring-invite/{code}", async (string code, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetRingHolderInviteQuery(code), ct)))
            .AllowAnonymous().WithTags("Work — Rings").WithName("GetRingHolderInvite")
            .WithSummary("Resolve a ring-holder invitation by code.");

        app.MapPost("/api/ring-invite/{code}/accept", async (string code, AcceptHolderBody b, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new AcceptRingHolderInviteCommand(code, b.Password, b.FirstName, b.LastName), ct)))
            .AllowAnonymous().WithTags("Work — Rings").WithName("AcceptRingHolderInvite")
            .WithSummary("Accept a ring-holder invitation, creating the login and assigning the holder.");

        var rings = app.MapGroup("/api/rings").WithTags("Work — Rings").RequireAuthorization();

        rings.MapGet("/", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetRingsQuery(), ct)))
            .WithName("GetRings").WithSummary("List all rings.");

        rings.MapGet("/mine", async (ISender sender, CancellationToken ct) =>
        {
            var ring = await sender.Send(new GetMyRingQuery(), ct);
            return ring is null ? Results.NoContent() : Results.Ok(ring);
        }).WithName("GetMyRing").WithSummary("The ring held by the current user, if any.");

        rings.MapGet("/held", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetMyRingsQuery(), ct)))
            .WithName("GetMyRings").WithSummary("All rings held by the current user.");

        rings.MapGet("/{slug}", async (string slug, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetRingByDomainQuery(slug), ct)))
            .WithName("GetRingByDomain").WithSummary("Get a ring by its domain slug.");

        rings.MapPost("/", async (RingBody b, ISender sender, CancellationToken ct) =>
        {
            var dto = await sender.Send(new CreateRingCommand(b.Domain, b.Name, b.CodePrefix, b.HolderName,
                b.HolderUserId, b.Disciplines, b.HeroTitle, b.HeroSubtitle, b.Focus, b.Motto, b.AccentColor,
                b.HeroImageUrl, b.Resources), ct);
            return Results.Created($"/api/rings/{dto.Slug}", dto);
        }).WithName("CreateRing").WithSummary("Create a ring (platform admin).");

        rings.MapPut("/{id:guid}", async (Guid id, RingBody b, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new UpdateRingCommand(id, b.Name, b.HolderName, b.HolderUserId,
                b.Disciplines, b.HeroTitle, b.HeroSubtitle, b.Focus, b.Motto, b.AccentColor, b.HeroImageUrl,
                b.Resources), ct)))
            .WithName("UpdateRing").WithSummary("Update a ring (platform admin).");

        rings.MapGet("/{ringId:guid}/assignments", async (Guid ringId, string? status, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetAssignmentsQuery(ringId, status), ct)))
            .WithName("GetRingAssignments").WithSummary("List a ring's assignments.");

        rings.MapPost("/{ringId:guid}/assignments", async (Guid ringId, AssignmentBody b, ISender sender, CancellationToken ct) =>
        {
            var dto = await sender.Send(new CreateAssignmentCommand(ringId, b.Title, b.Type, b.Priority, b.Summary,
                b.Objective, b.AssigneeName, b.IssuedBy, b.CompanyId, b.StartDate, b.DueDate, b.Deliverable,
                b.AcceptanceCriteria, b.Dependencies, b.Blockers, b.NextStep, b.ReviewRequired, b.References,
                b.Tags, b.DomainDataJson), ct);
            return Results.Created($"/api/assignments/{dto.Id}", dto);
        }).WithName("CreateAssignment").WithSummary("Create an assignment for a ring.");

        rings.MapGet("/{ringId:guid}/events", async (Guid ringId, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetRingEventsQuery(ringId), ct)))
            .WithName("GetRingEvents").WithSummary("A ring's calendar entries.");

        rings.MapPost("/{ringId:guid}/events", async (Guid ringId, EventBody b, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new CreateRingEventCommand(ringId, b.Title, b.Date, b.TimeLabel, b.Detail), ct)))
            .WithName("CreateRingEvent").WithSummary("Add a calendar entry to a ring.");

        rings.MapGet("/{ringId:guid}/activity", async (Guid ringId, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetRingActivityQuery(ringId), ct)))
            .WithName("GetRingActivity").WithSummary("A ring's recent activity.");

        rings.MapPost("/{ringId:guid}/holder/invite", async (Guid ringId, HolderInviteBody b, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new CreateRingHolderInvitationCommand(ringId, b.FirstName, b.LastName, b.Email, b.ExpiresInDays ?? 14), ct)))
            .WithName("InviteRingHolder").WithSummary("Invite a person to hold a ring (returns the code once).");

        rings.MapPost("/{ringId:guid}/holder", async (Guid ringId, SetHolderBody b, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new SetRingHolderCommand(ringId, b.Email, b.Clear ?? false), ct)))
            .WithName("SetRingHolder").WithSummary("Assign an existing user (or the caller) as holder, or clear it.");

        rings.MapGet("/{ringId:guid}/files", async (Guid ringId, Guid? assignmentId, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetRingFilesQuery(ringId, assignmentId), ct)))
            .WithName("GetRingFiles").WithSummary("List a ring's files.");

        rings.MapPost("/{ringId:guid}/files", async (Guid ringId, IFormFile file, [FromForm] Guid? assignmentId,
            IFileStorage storage, ISender sender, CancellationToken ct) =>
        {
            if (file is null || file.Length == 0) return Results.BadRequest("No file provided.");
            await using var stream = file.OpenReadStream();
            var key = await storage.SaveAsync(stream, ct);
            var dto = await sender.Send(new RegisterRingFileCommand(ringId, assignmentId, file.FileName,
                file.ContentType ?? "application/octet-stream", file.Length, key), ct);
            return Results.Created($"/api/ring-files/{dto.Id}", dto);
        })
            .DisableAntiforgery()
            .WithName("UploadRingFile").WithSummary("Upload a file to a ring (optionally tied to an assignment).");

        app.MapGet("/api/org/overview", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetOrgOverviewQuery(), ct)))
            .RequireAuthorization().WithTags("Work — Rings").WithName("GetOrgOverview")
            .WithSummary("Organization-wide overview for the Crown.");

        var files = app.MapGroup("/api/ring-files").WithTags("Work — Files").RequireAuthorization();

        files.MapGet("/{id:guid}/download", async (Guid id, IFileStorage storage, ISender sender, CancellationToken ct) =>
        {
            var info = await sender.Send(new GetRingFileForDownloadQuery(id), ct);
            var stream = await storage.OpenReadAsync(info.StorageKey, ct);
            return stream is null ? Results.NotFound() : Results.File(stream, info.ContentType, info.FileName);
        })
            .WithName("DownloadRingFile").WithSummary("Download a ring file.");

        files.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new DeleteRingFileCommand(id), ct);
            return Results.NoContent();
        })
            .WithName("DeleteRingFile").WithSummary("Delete a ring file.");

        var assignments = app.MapGroup("/api/assignments").WithTags("Work — Assignments").RequireAuthorization();

        assignments.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetAssignmentQuery(Id: id), ct)))
            .WithName("GetAssignment").WithSummary("Get an assignment by id.");

        assignments.MapGet("/by-code/{code}", async (string code, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetAssignmentQuery(Code: code), ct)))
            .WithName("GetAssignmentByCode").WithSummary("Get an assignment by its code (e.g. RES-0042).");

        assignments.MapPut("/{id:guid}", async (Guid id, AssignmentBody b, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new UpdateAssignmentCommand(id, b.Title, b.Type, b.Priority, b.Summary,
                b.Objective, b.AssigneeName, b.IssuedBy, b.CompanyId, b.StartDate, b.DueDate, b.Deliverable,
                b.AcceptanceCriteria, b.Dependencies, b.Blockers, b.NextStep, b.ReviewRequired, b.References,
                b.Tags, b.DomainDataJson), ct)))
            .WithName("UpdateAssignment").WithSummary("Edit an assignment.");

        assignments.MapPatch("/{id:guid}/status", async (Guid id, StatusBody b, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new SetAssignmentStatusCommand(id, b.Status, b.ProgressPercent), ct)))
            .WithName("SetAssignmentStatus").WithSummary("Move an assignment through its lifecycle.");

        assignments.MapPost("/{id:guid}/updates", async (Guid id, UpdateBody b, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new AddAssignmentUpdateCommand(id, b.Body), ct)))
            .WithName("AddAssignmentUpdate").WithSummary("Append a work-log entry.");
    }
}
