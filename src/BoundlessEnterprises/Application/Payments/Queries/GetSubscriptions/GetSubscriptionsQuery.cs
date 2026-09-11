using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Payments.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Payments.Queries.GetSubscriptions;

public record GetSubscriptionsQuery(Guid CompanyId) : IRequest<IReadOnlyList<SubscriptionDto>>;

public class GetSubscriptionsHandler : IRequestHandler<GetSubscriptionsQuery, IReadOnlyList<SubscriptionDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetSubscriptionsHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<SubscriptionDto>> Handle(GetSubscriptionsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.CanAccessCompany(request.CompanyId))
            throw new ForbiddenAccessException();

        var subscriptions = await _db.Subscriptions
            .AsNoTracking()
            .Where(s => s.CompanyId == request.CompanyId)
            .OrderByDescending(s => s.StartedAtUtc)
            .ToListAsync(cancellationToken);

        var customerNames = await _db.Customers
            .AsNoTracking()
            .Where(c => c.CompanyId == request.CompanyId)
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

        return subscriptions
            .Select(s => SubscriptionDto.FromEntity(s, customerNames.GetValueOrDefault(s.CustomerId)))
            .ToList();
    }
}
