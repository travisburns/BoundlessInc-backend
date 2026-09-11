using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Companies.Commands.CreateCompany;
using BoundlessEnterprises.Application.Companies.DTOs;
using BoundlessEnterprises.Domain.Companies;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Companies.Commands.UpdateCompany;

/// <summary>Updates a company's profile, capabilities, and status. Platform-admin only.</summary>
public record UpdateCompanyCommand(
    Guid Id,
    string Name,
    string Status,
    string? Tagline,
    string? Description,
    string? Sector,
    string? AccentColor,
    string? WebsiteUrl,
    string? Domain,
    string? ContactEmail,
    string? ContactPhone,
    bool SupportsEmployeeLogin,
    bool SupportsPayments,
    int SortOrder) : IRequest<CompanyDetailDto>;

public sealed class UpdateCompanyValidator : AbstractValidator<UpdateCompanyCommand>
{
    public UpdateCompanyValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Status).NotEmpty();
        RuleFor(x => x.ContactEmail).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));
    }
}

public class UpdateCompanyHandler : IRequestHandler<UpdateCompanyCommand, CompanyDetailDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public UpdateCompanyHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<CompanyDetailDto> Handle(UpdateCompanyCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsPlatformAdmin)
            throw new ForbiddenAccessException("Only platform administrators can edit companies.");

        var company = await _db.Companies.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Company", request.Id);

        var status = Enum.TryParse<CompanyStatus>(request.Status, out var s) ? s : company.Status;

        company.UpdateProfile(request.Name, request.Tagline, request.Description, request.Sector,
            logoUrl: company.LogoUrl, accentColor: request.AccentColor, websiteUrl: request.WebsiteUrl,
            domain: request.Domain, contactEmail: request.ContactEmail, contactPhone: request.ContactPhone);
        company.ConfigureCapabilities(request.SupportsEmployeeLogin, request.SupportsPayments, request.SortOrder);
        CreateCompanyHandler.ApplyStatus(company, status);

        await _db.SaveChangesAsync(cancellationToken);
        return CompanyDetailDto.FromEntity(company);
    }
}
