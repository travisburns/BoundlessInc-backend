using BoundlessEnterprises.Application.Documents.Commands.AcknowledgeAssignment;
using BoundlessEnterprises.Application.Documents.Commands.AssignDocument;
using BoundlessEnterprises.Application.Documents.Commands.CreateDocument;
using BoundlessEnterprises.Application.Documents.Queries.GetAssignments;
using BoundlessEnterprises.Application.Documents.Queries.GetDocuments;
using BoundlessEnterprises.Domain.Documents;
using MediatR;

namespace BoundlessEnterprises.Api.Endpoints.Documents;

/// <summary>Company-scoped document library and assignment endpoints.</summary>
public sealed class DocumentsEndpoints : IEndpointModule
{
    public record CreateDocumentRequest(string Title, DocumentType Type, string? Description, string? Url);
    public record AssignDocumentRequest(Guid EmployeeId);

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/companies/{companyId:guid}/documents")
            .WithTags("Documents")
            .RequireAuthorization();

        group.MapGet("/", async (Guid companyId, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetDocumentsQuery(companyId), ct)))
            .WithName("GetDocuments")
            .WithSummary("List a company's documents.");

        group.MapPost("/", async (Guid companyId, CreateDocumentRequest body, ISender sender, CancellationToken ct) =>
        {
            var dto = await sender.Send(new CreateDocumentCommand(
                companyId, body.Title, body.Type, body.Description, body.Url), ct);
            return Results.Created($"/api/companies/{companyId}/documents/{dto.Id}", dto);
        })
            .WithName("CreateDocument")
            .WithSummary("Add a document to a company's library.");

        group.MapGet("/assignments", async (Guid companyId, Guid? employeeId, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetAssignmentsQuery(companyId, employeeId), ct)))
            .WithName("GetDocumentAssignments")
            .WithSummary("List document assignments, optionally filtered by employee.");

        group.MapPost("/{documentId:guid}/assignments",
            async (Guid companyId, Guid documentId, AssignDocumentRequest body, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new AssignDocumentCommand(companyId, documentId, body.EmployeeId), ct)))
            .WithName("AssignDocument")
            .WithSummary("Assign a document to an employee.");

        group.MapPost("/assignments/{assignmentId:guid}/acknowledge",
            async (Guid companyId, Guid assignmentId, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new AcknowledgeAssignmentCommand(companyId, assignmentId), ct)))
            .WithName("AcknowledgeDocumentAssignment")
            .WithSummary("Acknowledge a document assignment.");
    }
}
