using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Onboarding.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Onboarding.Queries.GetProcesses;

/// <summary>Lists onboarding processes for a company, optionally filtered to one employee.</summary>
public record GetProcessesQuery(Guid CompanyId, Guid? EmployeeId = null)
    : IRequest<IReadOnlyList<OnboardingProcessDto>>;

public class GetProcessesHandler : IRequestHandler<GetProcessesQuery, IReadOnlyList<OnboardingProcessDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetProcessesHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<OnboardingProcessDto>> Handle(GetProcessesQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.CanAccessCompany(request.CompanyId))
            throw new ForbiddenAccessException();

        var query = _db.OnboardingProcesses
            .AsNoTracking()
            .Include(p => p.Steps)
            .Where(p => p.CompanyId == request.CompanyId);

        if (request.EmployeeId is { } employeeId)
            query = query.Where(p => p.EmployeeId == employeeId);

        var processes = await query
            .OrderByDescending(p => p.StartedAtUtc)
            .ToListAsync(cancellationToken);

        return processes.Select(OnboardingProcessDto.FromEntity).ToList();
    }
}
