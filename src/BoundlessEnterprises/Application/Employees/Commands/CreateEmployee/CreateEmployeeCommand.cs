using BoundlessEnterprises.Application.Employees.DTOs;
using BoundlessEnterprises.Domain.Employees;
using MediatR;

namespace BoundlessEnterprises.Application.Employees.Commands.CreateEmployee;

public record CreateEmployeeCommand(
    Guid CompanyId,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string Title,
    string? Department,
    EmploymentType EmploymentType,
    DateOnly StartDate) : IRequest<EmployeeDto>;
