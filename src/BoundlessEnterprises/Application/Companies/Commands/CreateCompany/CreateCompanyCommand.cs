using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Companies.DTOs;
using BoundlessEnterprises.Application.Companies.Services;
using BoundlessEnterprises.Domain.Companies;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Companies.Commands.CreateCompany;

/// <summary>
/// Creates a company in the portfolio. Slug and code are derived from the name
/// when not supplied, and made unique. Platform-admin only.
/// </summary>
public record CreateCompanyCommand(
    string Name,
    string? Slug,
    string? Code,
    string Type,
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

public sealed class CreateCompanyValidator : AbstractValidator<CreateCompanyCommand>
{
    public CreateCompanyValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Type).NotEmpty();
        RuleFor(x => x.Status).NotEmpty();
        RuleFor(x => x.ContactEmail).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));
        RuleFor(x => x.AccentColor).MaximumLength(20);
    }
}

public class CreateCompanyHandler : IRequestHandler<CreateCompanyCommand, CompanyDetailDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public CreateCompanyHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<CompanyDetailDto> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsPlatformAdmin)
            throw new ForbiddenAccessException("Only platform administrators can create companies.");

        var type = Enum.TryParse<CompanyType>(request.Type, out var t) ? t : CompanyType.Subsidiary;
        var status = Enum.TryParse<CompanyStatus>(request.Status, out var s) ? s : CompanyStatus.ComingSoon;

        var existingSlugs = await _db.Companies.Select(c => c.Slug).ToListAsync(cancellationToken);
        var existingCodes = await _db.Companies.Select(c => c.Code).ToListAsync(cancellationToken);

        var slug = UniqueSlug(
            string.IsNullOrWhiteSpace(request.Slug) ? CompanyIdentity.Slugify(request.Name) : CompanyIdentity.Slugify(request.Slug!),
            existingSlugs);
        var code = UniqueCode(
            string.IsNullOrWhiteSpace(request.Code) ? CompanyIdentity.BaseCode(request.Name) : request.Code!.Trim().ToUpperInvariant(),
            existingCodes);

        var company = Company.Create(request.Name, slug, code, type);
        company.UpdateProfile(request.Name, request.Tagline, request.Description, request.Sector,
            logoUrl: null, accentColor: request.AccentColor, websiteUrl: request.WebsiteUrl,
            domain: request.Domain, contactEmail: request.ContactEmail, contactPhone: request.ContactPhone);
        company.ConfigureCapabilities(request.SupportsEmployeeLogin, request.SupportsPayments, request.SortOrder);
        ApplyStatus(company, status);

        _db.Companies.Add(company);
        await _db.SaveChangesAsync(cancellationToken);

        return CompanyDetailDto.FromEntity(company);
    }

    internal static void ApplyStatus(Company company, CompanyStatus status)
    {
        switch (status)
        {
            case CompanyStatus.Active: company.Activate(); break;
            case CompanyStatus.Inactive: company.Deactivate(); break;
            case CompanyStatus.Archived: company.Archive(); break;
            default: company.MarkComingSoon(); break;
        }
    }

    private static string UniqueSlug(string baseSlug, ICollection<string> taken)
    {
        if (string.IsNullOrWhiteSpace(baseSlug)) baseSlug = "company";
        if (!taken.Contains(baseSlug)) return baseSlug;
        for (var i = 2; ; i++)
        {
            var candidate = $"{baseSlug}-{i}";
            if (!taken.Contains(candidate)) return candidate;
        }
    }

    private static string UniqueCode(string baseCode, ICollection<string> taken)
    {
        for (var i = 1; ; i++)
        {
            var candidate = $"{baseCode}-{i:000}";
            if (!taken.Contains(candidate)) return candidate;
        }
    }
}
