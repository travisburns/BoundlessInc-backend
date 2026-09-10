using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Payments.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Payments.Queries.GetInvoices;

public record GetInvoicesQuery(Guid CompanyId) : IRequest<IReadOnlyList<InvoiceDto>>;

public class GetInvoicesHandler : IRequestHandler<GetInvoicesQuery, IReadOnlyList<InvoiceDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetInvoicesHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<InvoiceDto>> Handle(GetInvoicesQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.CanAccessCompany(request.CompanyId))
            throw new ForbiddenAccessException();

        var invoices = await _db.Invoices
            .AsNoTracking()
            .Include(i => i.Items)
            .Where(i => i.CompanyId == request.CompanyId)
            .OrderByDescending(i => i.IssuedAtUtc)
            .ToListAsync(cancellationToken);

        var customerNames = await _db.Customers
            .AsNoTracking()
            .Where(c => c.CompanyId == request.CompanyId)
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

        return invoices
            .Select(i => InvoiceDto.FromEntity(i, customerNames.GetValueOrDefault(i.CustomerId)))
            .ToList();
    }
}
