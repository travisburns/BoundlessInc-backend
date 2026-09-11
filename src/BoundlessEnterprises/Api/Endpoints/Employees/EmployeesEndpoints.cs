using BoundlessEnterprises.Application.Employees.Commands.CreateEmployee;
using BoundlessEnterprises.Application.Employees.Queries.GetEmployees;
using BoundlessEnterprises.Domain.Employees;
using MediatR;

namespace BoundlessEnterprises.Api.Endpoints.Employees;

/// <summary>Company-scoped employee directory endpoints (portal, authenticated).</summary>
public sealed class EmployeesEndpoints : IEndpointModule
{
    public record CreateEmployeeRequest(
        string FirstName,
        string LastName,
        string Email,
        string? Phone,
        string Title,
        string? Department,
        EmploymentType EmploymentType,
        DateOnly StartDate);

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/companies/{companyId:guid}/employees")
            .WithTags("Employees")
            .RequireAuthorization();

        group.MapGet("/", async (Guid companyId, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetEmployeesQuery(companyId), ct)))
            .WithName("GetEmployees")
            .WithSummary("List the employees of a company.");

        group.MapPost("/", async (Guid companyId, CreateEmployeeRequest body, ISender sender, CancellationToken ct) =>
        {
            var dto = await sender.Send(new CreateEmployeeCommand(
                companyId, body.FirstName, body.LastName, body.Email, body.Phone,
                body.Title, body.Department, body.EmploymentType, body.StartDate), ct);
            return Results.Created($"/api/companies/{companyId}/employees/{dto.Id}", dto);
        })
            .WithName("CreateEmployee")
            .WithSummary("Add an employee to a company.");
    }
}
