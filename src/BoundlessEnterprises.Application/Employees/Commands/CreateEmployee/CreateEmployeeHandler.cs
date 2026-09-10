using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Employees.DTOs;
using BoundlessEnterprises.Domain.Employees;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Employees.Commands.CreateEmployee;

public class CreateEmployeeHandler : IRequestHandler<CreateEmployeeCommand, EmployeeDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public CreateEmployeeHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<EmployeeDto> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.CanAccessCompany(request.CompanyId))
            throw new ForbiddenAccessException();

        var companyExists = await _db.Companies
            .AnyAsync(c => c.Id == request.CompanyId, cancellationToken);
        if (!companyExists)
            throw new NotFoundException("Company", request.CompanyId);

        var email = request.Email.Trim().ToLowerInvariant();
        var duplicate = await _db.Employees
            .AnyAsync(e => e.CompanyId == request.CompanyId && e.Email == email, cancellationToken);
        if (duplicate)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["email"] = new[] { "An employee with this email already exists at this company." },
            });
        }

        var employee = Employee.Create(
            request.CompanyId,
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone,
            request.Title,
            request.Department,
            request.EmploymentType,
            request.StartDate);
        employee.Activate();

        _db.Employees.Add(employee);
        await _db.SaveChangesAsync(cancellationToken);

        return EmployeeDto.FromEntity(employee);
    }
}
