using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Domain.Payments;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Intelligence.Queries.GetPortfolioOverview;

/// <summary>Cross-company executive overview. Restricted to platform administrators.</summary>
public record GetPortfolioOverviewQuery(int TrendDays = 14) : IRequest<PortfolioOverviewDto>;

public class GetPortfolioOverviewHandler : IRequestHandler<GetPortfolioOverviewQuery, PortfolioOverviewDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _clock;

    public GetPortfolioOverviewHandler(IApplicationDbContext db, ICurrentUser currentUser, IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<PortfolioOverviewDto> Handle(GetPortfolioOverviewQuery request, CancellationToken cancellationToken)
    {
        // Cross-company intelligence is a holding-company (platform) view only.
        if (!_currentUser.IsPlatformAdmin)
            throw new ForbiddenAccessException("Enterprise intelligence is restricted to platform administrators.");

        var companies = await _db.Companies
            .AsNoTracking()
            .Select(c => new { c.Id, c.Name, c.Code, c.AccentColor })
            .ToListAsync(cancellationToken);

        var eventAgg = await _db.BusinessEvents
            .AsNoTracking()
            .GroupBy(e => e.CompanyId)
            .Select(g => new { CompanyId = g.Key, Revenue = g.Sum(e => e.Revenue), Events = g.Count() })
            .ToDictionaryAsync(x => x.CompanyId, cancellationToken);

        var mrrAgg = await _db.Subscriptions
            .AsNoTracking()
            .Where(s => s.Status == SubscriptionStatus.Active)
            .ToListAsync(cancellationToken);
        var mrrByCompany = mrrAgg
            .GroupBy(s => s.CompanyId)
            .ToDictionary(g => g.Key, g => g.Sum(s => s.MonthlyRecurringRevenue));

        var perCompany = companies
            .Select(c =>
            {
                eventAgg.TryGetValue(c.Id, out var agg);
                mrrByCompany.TryGetValue(c.Id, out var mrr);
                return new CompanyPerformanceDto
                {
                    CompanyId = c.Id,
                    Name = c.Name,
                    Code = c.Code,
                    AccentColor = c.AccentColor,
                    Revenue = agg?.Revenue ?? 0,
                    Events = agg?.Events ?? 0,
                    Mrr = mrr,
                };
            })
            .Where(c => c.Revenue > 0 || c.Events > 0 || c.Mrr > 0)
            .OrderByDescending(c => c.Revenue)
            .ToList();

        var since = _clock.UtcNow.Date.AddDays(-request.TrendDays + 1);
        var trendRows = await _db.BusinessEvents
            .AsNoTracking()
            .Where(e => e.OccurredAtUtc >= since)
            .GroupBy(e => e.OccurredAtUtc.Date)
            .Select(g => new { Date = g.Key, Revenue = g.Sum(e => e.Revenue) })
            .ToListAsync(cancellationToken);

        var trendByDate = trendRows.ToDictionary(r => DateOnly.FromDateTime(r.Date), r => r.Revenue);
        var trend = Enumerable.Range(0, request.TrendDays)
            .Select(offset =>
            {
                var date = DateOnly.FromDateTime(since.AddDays(offset));
                trendByDate.TryGetValue(date, out var revenue);
                return new RevenuePointDto { Date = date, Revenue = revenue };
            })
            .ToList();

        return new PortfolioOverviewDto
        {
            TotalRevenue = perCompany.Sum(c => c.Revenue),
            TotalEvents = perCompany.Sum(c => c.Events),
            TotalMrr = perCompany.Sum(c => c.Mrr),
            CompanyCount = perCompany.Count,
            Companies = perCompany,
            RevenueByDay = trend,
        };
    }
}
