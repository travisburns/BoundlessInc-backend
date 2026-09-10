using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Onboarding.DTOs;
using BoundlessEnterprises.Domain.Onboarding;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Onboarding.Commands.StartOnboarding;

public record StartOnboardingCommand(Guid CompanyId, Guid EmployeeId, Guid TemplateId)
    : IRequest<OnboardingProcessDto>;

public sealed class StartOnboardingValidator : AbstractValidator<StartOnboardingCommand>
{
    public StartOnboardingValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.TemplateId).NotEmpty();
    }
}

public class StartOnboardingHandler : IRequestHandler<StartOnboardingCommand, OnboardingProcessDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public StartOnboardingHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<OnboardingProcessDto> Handle(StartOnboardingCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.CanAccessCompany(request.CompanyId))
            throw new ForbiddenAccessException();

        var template = await _db.OnboardingTemplates
            .Include(t => t.Steps)
            .FirstOrDefaultAsync(t => t.Id == request.TemplateId && t.CompanyId == request.CompanyId, cancellationToken)
            ?? throw new NotFoundException("OnboardingTemplate", request.TemplateId);

        var employeeExists = await _db.Employees
            .AnyAsync(e => e.Id == request.EmployeeId && e.CompanyId == request.CompanyId, cancellationToken);
        if (!employeeExists)
            throw new NotFoundException("Employee", request.EmployeeId);

        var process = OnboardingProcess.Start(template, request.EmployeeId);
        _db.OnboardingProcesses.Add(process);
        await _db.SaveChangesAsync(cancellationToken);

        return OnboardingProcessDto.FromEntity(process);
    }
}
