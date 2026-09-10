using BoundlessEnterprises.Domain.Employees;

namespace BoundlessEnterprises.Application.Employees.DTOs;

/// <summary>Read model for an employee within a company, flattening the primary employment.</summary>
public record EmployeeDto
{
    public Guid Id { get; init; }
    public Guid CompanyId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? Title { get; init; }
    public string? Department { get; init; }
    public string? EmploymentType { get; init; }
    public DateOnly? StartDate { get; init; }

    public static EmployeeDto FromEntity(Employee e)
    {
        var primary = e.PrimaryEmployment;
        return new EmployeeDto
        {
            Id = e.Id,
            CompanyId = e.CompanyId,
            FirstName = e.FirstName,
            LastName = e.LastName,
            FullName = e.FullName,
            Email = e.Email,
            Phone = e.Phone,
            Status = e.Status.ToString(),
            Title = primary?.Title,
            Department = primary?.Department,
            EmploymentType = primary?.Type.ToString(),
            StartDate = primary?.StartDate,
        };
    }
}
