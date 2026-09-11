using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Onboarding.DTOs;
using BoundlessEnterprises.Application.Onboarding.Services;
using BoundlessEnterprises.Domain.Onboarding;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Onboarding.Commands.SetInviteStep;

/// <summary>
/// Public: the new hire marks one of their own onboarding steps complete (or
/// reopens it) from the self-serve wizard, authenticated by their invite code.
/// When every required step is done, the invitation itself is marked completed
/// and the employee is activated.
/// </summary>
public record SetInviteStepCommand(string Code, Guid StepId, bool Completed, string? ResponseJson = null)
    : IRequest<InviteDetailDto>;

public class SetInviteStepHandler : IRequestHandler<SetInviteStepCommand, InviteDetailDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;

    public SetInviteStepHandler(IApplicationDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<InviteDetailDto> Handle(SetInviteStepCommand request, CancellationToken cancellationToken)
    {
        var hash = InviteCodes.Hash(request.Code);

        var invitation = await _db.OnboardingInvitations
            .FirstOrDefaultAsync(i => i.CodeHash == hash, cancellationToken)
            ?? throw new NotFoundException("OnboardingInvitation", request.Code);

        if (invitation.ProcessId is not Guid processId)
            throw new ConflictException("This onboarding hasn't been started yet.");

        var process = await _db.OnboardingProcesses
            .Include(p => p.Steps)
            .FirstOrDefaultAsync(p => p.Id == processId, cancellationToken)
            ?? throw new NotFoundException("OnboardingProcess", processId);

        if (request.Completed)
            process.CompleteStep(request.StepId, _clock.UtcNow, request.ResponseJson);
        else
            process.ReopenStep(request.StepId);

        if (process.Status == OnboardingStatus.Completed)
        {
            invitation.MarkCompleted();

            if (invitation.EmployeeId is Guid employeeId)
            {
                var employee = await _db.Employees
                    .FirstOrDefaultAsync(e => e.Id == employeeId, cancellationToken);
                employee?.Activate();
            }
        }

        await _db.SaveChangesAsync(cancellationToken);

        var company = await _db.Companies
            .FirstOrDefaultAsync(c => c.Id == invitation.CompanyId, cancellationToken);

        return new InviteDetailDto
        {
            Status = invitation.Status.ToString(),
            CompanyName = company?.Name ?? string.Empty,
            TemplateName = process.TemplateName,
            FirstName = invitation.FirstName,
            LastName = invitation.LastName,
            Email = invitation.Email,
            Title = invitation.Title,
            Process = OnboardingProcessDto.FromEntity(process),
        };
    }
}
