using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Onboarding.DTOs;
using BoundlessEnterprises.Application.Onboarding.Services;
using BoundlessEnterprises.Domain.Onboarding;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Onboarding.Commands.CreateInvitation;

/// <summary>Creates a self-serve onboarding invitation for a new hire and returns the code once.</summary>
public record CreateInvitationCommand(
    Guid CompanyId,
    Guid TemplateId,
    string Email,
    string FirstName,
    string LastName,
    string? Title,
    int ExpiresInDays = 14) : IRequest<InvitationCreatedDto>;

public sealed class CreateInvitationValidator : AbstractValidator<CreateInvitationCommand>
{
    public CreateInvitationValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.TemplateId).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Title).MaximumLength(150);
        RuleFor(x => x.ExpiresInDays).InclusiveBetween(1, 90);
    }
}

public class CreateInvitationHandler : IRequestHandler<CreateInvitationCommand, InvitationCreatedDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _clock;

    public CreateInvitationHandler(IApplicationDbContext db, ICurrentUser currentUser, IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<InvitationCreatedDto> Handle(CreateInvitationCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.CanAccessCompany(request.CompanyId))
            throw new ForbiddenAccessException();

        var template = await _db.OnboardingTemplates
            .FirstOrDefaultAsync(t => t.Id == request.TemplateId && t.CompanyId == request.CompanyId, cancellationToken)
            ?? throw new NotFoundException("OnboardingTemplate", request.TemplateId);

        var (raw, hash) = InviteCodes.Generate();
        var invitation = OnboardingInvitation.Create(
            request.CompanyId, request.TemplateId, request.Email, request.FirstName,
            request.LastName, request.Title ?? "New Employee", hash,
            _clock.UtcNow.AddDays(request.ExpiresInDays));

        _db.OnboardingInvitations.Add(invitation);
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
