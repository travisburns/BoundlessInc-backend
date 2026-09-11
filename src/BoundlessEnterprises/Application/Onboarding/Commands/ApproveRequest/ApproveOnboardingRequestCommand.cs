using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Onboarding.DTOs;
using BoundlessEnterprises.Application.Onboarding.Services;
using BoundlessEnterprises.Domain.Onboarding;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Onboarding.Commands.ApproveRequest;

/// <summary>
/// Admin: approves a pending onboarding request, which issues an invitation
/// (generating the code once) and links it back to the request.
/// </summary>
public record ApproveOnboardingRequestCommand(
    Guid CompanyId,
    Guid RequestId,
    Guid TemplateId,
    string? Title,
    int ExpiresInDays = 14) : IRequest<InvitationCreatedDto>;

public sealed class ApproveOnboardingRequestValidator : AbstractValidator<ApproveOnboardingRequestCommand>
{
    public ApproveOnboardingRequestValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.RequestId).NotEmpty();
        RuleFor(x => x.TemplateId).NotEmpty();
        RuleFor(x => x.ExpiresInDays).InclusiveBetween(1, 90);
    }
}

public class ApproveOnboardingRequestHandler : IRequestHandler<ApproveOnboardingRequestCommand, InvitationCreatedDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _clock;

    public ApproveOnboardingRequestHandler(IApplicationDbContext db, ICurrentUser currentUser, IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<InvitationCreatedDto> Handle(ApproveOnboardingRequestCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.CanAccessCompany(request.CompanyId))
            throw new ForbiddenAccessException();

        var onboardingRequest = await _db.OnboardingRequests
            .FirstOrDefaultAsync(r => r.Id == request.RequestId && r.CompanyId == request.CompanyId, cancellationToken)
            ?? throw new NotFoundException("OnboardingRequest", request.RequestId);

        if (onboardingRequest.Status != OnboardingRequestStatus.Pending)
            throw new ConflictException("This request has already been reviewed.");

        var template = await _db.OnboardingTemplates
            .FirstOrDefaultAsync(t => t.Id == request.TemplateId && t.CompanyId == request.CompanyId, cancellationToken)
            ?? throw new NotFoundException("OnboardingTemplate", request.TemplateId);

        var (raw, hash) = InviteCodes.Generate();
        var invitation = OnboardingInvitation.Create(
            request.CompanyId, request.TemplateId, onboardingRequest.Email,
            onboardingRequest.FirstName, onboardingRequest.LastName,
            request.Title ?? onboardingRequest.DesiredRole ?? "New Employee",
            hash, _clock.UtcNow.AddDays(request.ExpiresInDays));

        _db.OnboardingInvitations.Add(invitation);
        onboardingRequest.Approve(invitation.Id, _clock.UtcNow);

        await _db.SaveChangesAsync(cancellationToken);

        return new InvitationCreatedDto
        {
            Id = invitation.Id,
            Code = raw,
            Email = invitation.Email,
            TemplateName = template.Name,
            ExpiresAtUtc = invitation.ExpiresAtUtc,
        };
    }
}
