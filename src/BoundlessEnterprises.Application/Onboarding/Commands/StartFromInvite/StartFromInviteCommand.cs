using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Onboarding.DTOs;
using BoundlessEnterprises.Application.Onboarding.Services;
using BoundlessEnterprises.Domain.Employees;
using BoundlessEnterprises.Domain.Onboarding;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Onboarding.Commands.StartFromInvite;

/// <summary>
/// Public: a new hire redeems their invite code to create their own employee
/// record and start their onboarding process. They may correct the name on the
/// invitation before starting. Idempotent — if already started, returns the
/// existing process.
/// </summary>
public record StartFromInviteCommand(string Code, string? FirstName, string? LastName)
    : IRequest<InviteDetailDto>;

public sealed class StartFromInviteValidator : AbstractValidator<StartFromInviteCommand>
{
    public StartFromInviteValidator()
    {
        RuleFor(x => x.Code).NotEmpty();
        RuleFor(x => x.FirstName).MaximumLength(100);
        RuleFor(x => x.LastName).MaximumLength(100);
    }
}

public class StartFromInviteHandler : IRequestHandler<StartFromInviteCommand, InviteDetailDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;

    public StartFromInviteHandler(IApplicationDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<InviteDetailDto> Handle(StartFromInviteCommand request, CancellationToken cancellationToken)
    {
        var hash = InviteCodes.Hash(request.Code);

        var invitation = await _db.OnboardingInvitations
            .FirstOrDefaultAsync(i => i.CodeHash == hash, cancellationToken)
            ?? throw new NotFoundException("OnboardingInvitation", request.Code);

        if (!invitation.IsRedeemable(_clock.UtcNow))
            throw new ConflictException("This invitation is no longer redeemable.");

        invitation.UpdateDetails(request.FirstName ?? string.Empty, request.LastName ?? string.Empty);

        var company = await _db.Companies
            .FirstOrDefaultAsync(c => c.Id == invitation.CompanyId, cancellationToken)
            ?? throw new NotFoundException("Company", invitation.CompanyId);

        var template = await _db.OnboardingTemplates
            .Include(t => t.Steps)
            .FirstOrDefaultAsync(t => t.Id == invitation.TemplateId, cancellationToken)
            ?? throw new NotFoundException("OnboardingTemplate", invitation.TemplateId);

        OnboardingProcess process;

        if (invitation.ProcessId is Guid existingId)
        {
            process = await _db.OnboardingProcesses
                .Include(p => p.Steps)
                .FirstOrDefaultAsync(p => p.Id == existingId, cancellationToken)
                ?? throw new NotFoundException("OnboardingProcess", existingId);
        }
        else
        {
            var employee = Employee.Create(
                invitation.CompanyId,
                invitation.FirstName,
                invitation.LastName,
                invitation.Email,
                phone: null,
                title: invitation.Title,
                department: null,
                type: EmploymentType.FullTime,
                startDate: DateOnly.FromDateTime(_clock.UtcNow));

            _db.Employees.Add(employee);

            process = OnboardingProcess.Start(template, employee.Id);
            _db.OnboardingProcesses.Add(process);

            invitation.MarkStarted(employee.Id, process.Id);
        }

        await _db.SaveChangesAsync(cancellationToken);

        return new InviteDetailDto
        {
            Status = invitation.Status.ToString(),
            CompanyName = company.Name,
            TemplateName = template.Name,
            FirstName = invitation.FirstName,
            LastName = invitation.LastName,
            Email = invitation.Email,
            Title = invitation.Title,
            Process = OnboardingProcessDto.FromEntity(process),
        };
    }
}
