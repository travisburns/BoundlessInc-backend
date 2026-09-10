using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Payments.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Payments.Queries.GetPayments;

public record GetPaymentsQuery(Guid CompanyId) : IRequest<IReadOnlyList<PaymentDto>>;

public class GetPaymentsHandler : IRequestHandler<GetPaymentsQuery, IReadOnlyList<PaymentDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetPaymentsHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<PaymentDto>> Handle(GetPaymentsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.CanAccessCompany(request.CompanyId))
            throw new ForbiddenAccessException();

        var payments = await _db.Payments
            .AsNoTracking()
            .Where(p => p.CompanyId == request.CompanyId)
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return payments.Select(PaymentDto.FromEntity).ToList();
    }
}
