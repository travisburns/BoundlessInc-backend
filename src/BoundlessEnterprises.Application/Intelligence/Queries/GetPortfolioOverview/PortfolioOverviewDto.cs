namespace BoundlessEnterprises.Application.Intelligence.Queries.GetPortfolioOverview;

public record CompanyPerformanceDto
{
    public Guid CompanyId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string? AccentColor { get; init; }
    public decimal Revenue { get; init; }
    public int Events { get; init; }
    public decimal Mrr { get; init; }
}

public record RevenuePointDto
{
    public DateOnly Date { get; init; }
    public decimal Revenue { get; init; }
}

/// <summary>
/// The executive command center's cross-company snapshot: portfolio totals, a
/// per-company breakdown, and a recent revenue trend.
/// </summary>
public record PortfolioOverviewDto
{
    public decimal TotalRevenue { get; init; }
    public int TotalEvents { get; init; }
    public decimal TotalMrr { get; init; }
    public int CompanyCount { get; init; }
    public IReadOnlyList<CompanyPerformanceDto> Companies { get; init; } = Array.Empty<CompanyPerformanceDto>();
    public IReadOnlyList<RevenuePointDto> RevenueByDay { get; init; } = Array.Empty<RevenuePointDto>();
}
