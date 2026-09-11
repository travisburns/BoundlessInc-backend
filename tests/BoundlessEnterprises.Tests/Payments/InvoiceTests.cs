using BoundlessEnterprises.Domain.Payments;
using Xunit;

namespace BoundlessEnterprises.UnitTests.Payments;

public class InvoiceTests
{
    private static Invoice NewInvoice() =>
        Invoice.Create(Guid.NewGuid(), Guid.NewGuid(), "INV-1");

    [Fact]
    public void Total_SumsLineItems()
    {
        var invoice = NewInvoice();
        invoice.AddItem("Item A", 3, 10m);   // 30
        invoice.AddItem("Item B", 1, 5.50m); // 5.50

        Assert.Equal(35.50m, invoice.Total);
    }

    [Fact]
    public void Issue_RequiresItems()
    {
        var invoice = NewInvoice();
        Assert.Throws<InvalidOperationException>(() => invoice.Issue());

        invoice.AddItem("Item", 1, 10m);
        invoice.Issue();
        Assert.Equal(InvoiceStatus.Open, invoice.Status);
    }

    [Fact]
    public void AddItem_AfterIssue_Throws()
    {
        var invoice = NewInvoice();
        invoice.AddItem("Item", 1, 10m);
        invoice.Issue();

        Assert.Throws<InvalidOperationException>(() => invoice.AddItem("Late", 1, 1m));
    }

    [Fact]
    public void MarkPaid_SetsStatusAndTimestamp()
    {
        var invoice = NewInvoice();
        invoice.AddItem("Item", 1, 10m);
        invoice.Issue();

        var now = DateTime.UtcNow;
        invoice.MarkPaid(now);

        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
        Assert.Equal(now, invoice.PaidAtUtc);
    }
}

public class SubscriptionTests
{
    [Fact]
    public void YearlySubscription_NormalizesMrr()
    {
        var sub = Subscription.Create(Guid.NewGuid(), Guid.NewGuid(), "Annual", "Pro",
            1200m, BillingInterval.Yearly);

        Assert.Equal(100m, sub.MonthlyRecurringRevenue);
    }

    [Fact]
    public void Cancel_SetsStatusAndTimestamp()
    {
        var sub = Subscription.Create(Guid.NewGuid(), Guid.NewGuid(), "Monthly", null,
            200m, BillingInterval.Monthly);
        var now = DateTime.UtcNow;

        sub.Cancel(now);

        Assert.Equal(SubscriptionStatus.Canceled, sub.Status);
        Assert.Equal(now, sub.CanceledAtUtc);
    }
}
