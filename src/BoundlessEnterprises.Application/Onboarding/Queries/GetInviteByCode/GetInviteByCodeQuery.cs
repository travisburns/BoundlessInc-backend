using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Onboarding.DTOs;
using BoundlessEnterprises.Application.Onboarding.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Onboarding.Queries.GetInviteByCode;

/// <summary>
/// Public: resolves an onboarding invitation by its raw code (hashed for lookup)
/// so the self-serve wizard can show the new hire what they're onboarding into.
/// Authenticated by possession of the code, not by a JWT.
/// </summary>
public record GetInviteByCodeQuery(string Code) : IRequest<InviteDetailDto>;

public class GetInviteByCodeHandler : IRequestHandler<GetInviteByCodeQuery, InviteDetailDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;

    public GetInviteByCodeHandler(IApplicationDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<InviteDetailDto> Handle(GetInviteByCodeQuery request, CancellationToken cancellationToken)
    {
        var hash = InviteCodes.Hash(request.Code);

        var invitation = await _db.OnboardingInvitations
            .FirstOrDefaultAsync(i => i.CodeHash == hash, cancellationToken)
            ?? throw new NotFoundException("OnboardingInvitation", request.Code);

        var company = await _db.Companies
            .FirstOrDefaultAsync(c => c.Id == invitation.CompanyId, cancellationToken);
        var template = await _db.OnboardingTemplates
            .FirstOrDefaultAsync(t => t.Id == invitation.TemplateId, cancellationToken);

        OnboardingProcessDto? process = null;
        if (invitation.ProcessId is Guid processId)
        {
            var entity = await _db.OnboardingProcesses
                .Include(p => p.Steps)
                .FirstOrDefaultAsync(p => p.Id == processId, cancellationToken);
            if (entity is not null)
                process = OnboardingProcessDto.FromEntity(entity);
        }

        // Reflect an expiry that has lapsed since creation.
        var status = invitation.Status;
        if (status is Domain.Onboarding.OnboardingInvitationStatus.Pending
            && invitation.ExpiresAtUtc <= _clock.UtcNow)
        {
            status = Domain.Onboarding.OnboardingInvitationStatus.Expired;
        }

        return new InviteDetailDto
        {
            Status = status.ToString(),
            CompanyName = company?.Name ?? string.Empty,
            TemplateName = template?.Name ?? invitation.Title,
            FirstName = invitation.FirstName,
            LastName = invitation.LastName,
            Email = invitation.Email,
            Title = invitation.Title,
            Process = process,
        };
    }
}
