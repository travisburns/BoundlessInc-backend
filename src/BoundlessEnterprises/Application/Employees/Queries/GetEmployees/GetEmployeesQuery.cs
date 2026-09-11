using BoundlessEnterprises.Application.Employees.DTOs;
using MediatR;

namespace BoundlessEnterprises.Application.Employees.Queries.GetEmployees;

/// <summary>Lists the employees of a company. Caller must have access to the company.</summary>
public record GetEmployeesQuery(Guid CompanyId) : IRequest<IReadOnlyList<EmployeeDto>>;
