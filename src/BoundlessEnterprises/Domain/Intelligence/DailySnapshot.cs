using BoundlessEnterprises.Domain.Common;

namespace BoundlessEnterprises.Domain.Intelligence;

/// <summary>
/// A per-company, per-day rollup of ingested events. Materialized from
/// <see cref="BusinessEvent"/> to make trend queries cheap over time.
/// </summary>
public class DailySnapshot : Entity
{
    private DailySnapshot() { }

    public DailySnapshot(Guid companyId, DateOnly date, decimal revenue, int eventCount)
    {
        CompanyId = companyId;
        Date = date;
        Revenue = revenue;
        EventCount = eventCount;
    }

    public Guid CompanyId { get; private set; }
    public DateOnly Date { get; private set; }
    public decimal Revenue { get; private set; }
    public int EventCount { get; private set; }

    public void Set(decimal revenue, int eventCount)
    {
        Revenue = revenue;
        EventCount = eventCount;
    }
}
