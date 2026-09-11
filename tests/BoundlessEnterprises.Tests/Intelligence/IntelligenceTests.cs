using BoundlessEnterprises.Domain.Integrations;
using BoundlessEnterprises.Domain.Intelligence;
using Xunit;

namespace BoundlessEnterprises.UnitTests.Intelligence;

public class IntegrationTests
{
    [Fact]
    public void RecordEvent_IncrementsCountAndTimestamp()
    {
        var integration = Integration.Create(Guid.NewGuid(), "POS", "hash", "be_live_ab…");
        Assert.Equal(0, integration.EventCount);

        var now = DateTime.UtcNow;
        integration.RecordEvent(now);

        Assert.Equal(1, integration.EventCount);
        Assert.Equal(now, integration.LastEventAtUtc);
        Assert.True(integration.IsActive);
    }

    [Fact]
    public void Disable_MakesInactive()
    {
        var integration = Integration.Create(Guid.NewGuid(), "POS", "hash", "prefix");
        integration.Disable();
        Assert.False(integration.IsActive);
        Assert.Equal(IntegrationStatus.Disabled, integration.Status);
    }
}

public class BusinessEventTests
{
    [Fact]
    public void Create_NormalizesCurrency_AndSetsFields()
    {
        var companyId = Guid.NewGuid();
        var occurred = new DateTime(2026, 9, 1, 10, 0, 0, DateTimeKind.Utc);
        var evt = BusinessEvent.Create(companyId, null, "OrderCompleted", 76.42m, "usd", occurred, "ref-1");

        Assert.Equal(companyId, evt.CompanyId);
        Assert.Equal("OrderCompleted", evt.EventType);
        Assert.Equal(76.42m, evt.Revenue);
        Assert.Equal("USD", evt.Currency);
        Assert.Equal(occurred, evt.OccurredAtUtc);
        Assert.Equal("ref-1", evt.ExternalRef);
    }

    [Fact]
    public void Create_Throws_WhenEventTypeMissing()
    {
        Assert.Throws<ArgumentException>(() =>
            BusinessEvent.Create(Guid.NewGuid(), null, " ", 0m, "USD", DateTime.UtcNow, null));
    }
}
