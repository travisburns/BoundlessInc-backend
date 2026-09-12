using BoundlessEnterprises.Domain.Onboarding.Core;
using Xunit;

namespace BoundlessEnterprises.UnitTests.Onboarding;

public class CoreOnboardingProgressTests
{
    private static CoreOnboardingProgress NewProgress() =>
        CoreOnboardingProgress.Start(Guid.NewGuid());

    [Fact]
    public void SignStage_RecordsSignature()
    {
        var p = NewProgress();
        p.SignStage("welcome", 1, "Welcome to Boundless", "Aaron Brown", "Aaron Brown", DateTime.UtcNow);

        var sig = Assert.Single(p.Signatures);
        Assert.Equal("welcome", sig.StageKey);
        Assert.Equal("Aaron Brown", sig.TypedName);
    }

    [Fact]
    public void SignStage_Twice_UpdatesInPlace_NoDuplicate()
    {
        var p = NewProgress();
        p.SignStage("welcome", 1, "Welcome to Boundless", "A. Brown", "A. Brown", DateTime.UtcNow);
        p.SignStage("welcome", 1, "Welcome to Boundless", "Aaron Brown", "Aaron Brown", DateTime.UtcNow);

        var sig = Assert.Single(p.Signatures);
        Assert.Equal("Aaron Brown", sig.TypedName);
    }

    [Fact]
    public void SignStage_TrimsWhitespace()
    {
        var p = NewProgress();
        p.SignStage("welcome", 1, "Welcome to Boundless", "  Aaron Brown  ", "  Aaron Brown  ", DateTime.UtcNow);

        Assert.Equal("Aaron Brown", p.Signatures[0].TypedName);
        Assert.Equal("Aaron Brown", p.Signatures[0].Signature);
    }

    [Fact]
    public void CompleteDocument_Throws_WhenStagesMissing()
    {
        var p = NewProgress();
        p.SignStage("welcome", 1, "Welcome to Boundless", "Aaron Brown", "Aaron Brown", DateTime.UtcNow);

        Assert.Throws<InvalidOperationException>(() =>
            p.CompleteDocument("Aaron Brown", "Aaron Brown", new[] { "welcome", "dream" }, DateTime.UtcNow));
        Assert.False(p.IsComplete);
    }

    [Fact]
    public void CompleteDocument_Succeeds_WhenAllStagesSigned()
    {
        var p = NewProgress();
        var required = new[] { "welcome", "dream" };
        p.SignStage("welcome", 1, "Welcome to Boundless", "Aaron Brown", "Aaron Brown", DateTime.UtcNow);
        p.SignStage("dream", 2, "Boundless Dream", "Aaron Brown", "Aaron Brown", DateTime.UtcNow);

        var when = DateTime.UtcNow;
        p.CompleteDocument("Aaron Brown", "Aaron Brown", required, when);

        Assert.True(p.IsComplete);
        Assert.Equal(when, p.CompletedAtUtc);
        Assert.Equal("Aaron Brown", p.CompletedName);
    }

    [Fact]
    public void ReSigningStage_AfterCompletion_ReopensDocument()
    {
        var p = NewProgress();
        var required = new[] { "welcome" };
        p.SignStage("welcome", 1, "Welcome to Boundless", "Aaron Brown", "Aaron Brown", DateTime.UtcNow);
        p.CompleteDocument("Aaron Brown", "Aaron Brown", required, DateTime.UtcNow);
        Assert.True(p.IsComplete);

        p.SignStage("welcome", 1, "Welcome to Boundless", "Aaron J. Brown", "Aaron J. Brown", DateTime.UtcNow);

        Assert.False(p.IsComplete);
        Assert.Null(p.CompletedAtUtc);
    }
}
