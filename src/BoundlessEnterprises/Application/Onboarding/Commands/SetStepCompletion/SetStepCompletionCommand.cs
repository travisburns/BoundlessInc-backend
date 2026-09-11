using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Onboarding.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Onboarding.Commands.SetStepCompletion;

/// <summary>Marks an onboarding step complete or reopens it, recomputing status.</summary>
public record SetStepCompletionCommand(Guid CompanyId, Guid ProcessId, Guid StepId, bool Completed)
    : IRequest<OnboardingProcessDto>;

public class SetStepCompletionHandler : IRequestHandler<SetStepCompletionCommand, OnboardingProcessDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _clock;

    public SetStepCompletionHandler(IApplicationDbContext db, ICurrentUser currentUser, IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<OnboardingProcessDto> Handle(SetStepCompletionCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.CanAccessCompany(request.CompanyId))
            throw new ForbiddenAccessException();

        var process = await _db.OnboardingProcesses
            .Include(p => p.Steps)
            .FirstOrDefaultAsync(p => p.Id == request.ProcessId && p.CompanyId == request.CompanyId, cancellationToken)
            ?? throw new NotFoundException("OnboardingProcess", request.ProcessId);

        if (request.Completed)
            process.CompleteStep(request.StepId, _clock.UtcNow);
        else
            process.ReopenStep(request.StepId);

        await _db.SaveChangesAsync(cancellationToken);
        return OnboardingProcessDto.FromEntity(process);
    }
}
