using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Documents.DTOs;
using BoundlessEnterprises.Domain.Documents;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Documents.Commands.CreateDocument;

public record CreateDocumentCommand(
    Guid CompanyId,
    string Title,
    DocumentType Type,
    string? Description,
    string? Url) : IRequest<DocumentDto>;

public sealed class CreateDocumentValidator : AbstractValidator<CreateDocumentCommand>
{
    public CreateDocumentValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Url).MaximumLength(1000);
    }
}

public class CreateDocumentHandler : IRequestHandler<CreateDocumentCommand, DocumentDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public CreateDocumentHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<DocumentDto> Handle(CreateDocumentCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.CanAccessCompany(request.CompanyId))
            throw new ForbiddenAccessException();

        var companyExists = await _db.Companies.AnyAsync(c => c.Id == request.CompanyId, cancellationToken);
        if (!companyExists)
            throw new NotFoundException("Company", request.CompanyId);

        var document = Document.Create(request.CompanyId, request.Title, request.Type, request.Description, request.Url);
        _db.Documents.Add(document);
        await _db.SaveChangesAsync(cancellationToken);

        return DocumentDto.FromEntity(document);
    }
}
