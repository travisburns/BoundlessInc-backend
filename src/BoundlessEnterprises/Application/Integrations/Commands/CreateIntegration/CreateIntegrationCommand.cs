using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Integrations.DTOs;
using BoundlessEnterprises.Application.Integrations.Services;
using BoundlessEnterprises.Domain.Integrations;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Integrations.Commands.CreateIntegration;

/// <summary>Creates an integration and returns the raw API key once.</summary>
public record CreateIntegrationCommand(Guid CompanyId, string Name) : IRequest<IntegrationCreatedDto>;

public sealed class CreateIntegrationValidator : AbstractValidator<CreateIntegrationCommand>
{
    public CreateIntegrationValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
    }
}

public class CreateIntegrationHandler : IRequestHandler<CreateIntegrationCommand, IntegrationCreatedDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public CreateIntegrationHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IntegrationCreatedDto> Handle(CreateIntegrationCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.CanAccessCompany(request.CompanyId))
            throw new ForbiddenAccessException();

        var companyExists = await _db.Companies.AnyAsync(c => c.Id == request.CompanyId, cancellationToken);
        if (!companyExists)
            throw new NotFoundException("Company", request.CompanyId);

        var (raw, hash, prefix) = ApiKeys.Generate();
        var integration = Integration.Create(request.CompanyId, request.Name, hash, prefix);

        _db.Integrations.Add(integration);
        await _db.SaveChangesAsync(cancellationToken);

        return new IntegrationCreatedDto
        {
            Integration = IntegrationDto.FromEntity(integration),
            ApiKey = raw,
        };
    }
}
