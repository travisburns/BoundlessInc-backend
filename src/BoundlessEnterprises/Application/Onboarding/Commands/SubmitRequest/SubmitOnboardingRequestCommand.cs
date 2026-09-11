using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Onboarding.DTOs;
using BoundlessEnterprises.Domain.Companies;
using BoundlessEnterprises.Domain.Onboarding;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Onboarding.Commands.SubmitRequest;

/// <summary>
/// Public: a prospective hire asks to onboard into a company. Creates a pending
/// request for an admin to review — no code is issued here.
/// </summary>
public record SubmitOnboardingRequestCommand(
    Guid CompanyId,
    string FirstName,
    string LastName,
    string Email,
    string? DesiredRole) : IRequest<RequestSubmittedDto>;

public sealed class SubmitOnboardingRequestValidator : AbstractValidator<SubmitOnboardingRequestCommand>
{
    public SubmitOnboardingRequestValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.DesiredRole).MaximumLength(150);
    }
}

public class SubmitOnboardingRequestHandler : IRequestHandler<SubmitOnboardingRequestCommand, RequestSubmittedDto>
{
    private readonly IApplicationDbContext _db;

    public SubmitOnboardingRequestHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<RequestSubmittedDto> Handle(SubmitOnboardingRequestCommand request, CancellationToken cancellationToken)
    {
        var company = await _db.Companies
            .FirstOrDefaultAsync(c => c.Id == request.CompanyId, cancellationToken)
            ?? throw new NotFoundException("Company", request.CompanyId);

        if (company.Status is not (CompanyStatus.Active or CompanyStatus.ComingSoon))
            throw new ConflictException("That company isn't accepting onboarding requests right now.");

        var onboardingRequest = OnboardingRequest.Create(
            request.CompanyId, request.FirstName, request.LastName, request.Email, request.DesiredRole);

        _db.OnboardingRequests.Add(onboardingRequest);
        await _db.SaveChangesAsync(cancellationToken);

        return new RequestSubmittedDto { Id = onboardingRequest.Id, CompanyName = company.Name };
    }
}
