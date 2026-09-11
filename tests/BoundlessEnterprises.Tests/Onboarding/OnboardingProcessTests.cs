using BoundlessEnterprises.Domain.Onboarding;
using Xunit;

namespace BoundlessEnterprises.UnitTests.Onboarding;

public class OnboardingProcessTests
{
    private static OnboardingTemplate BuildTemplate()
    {
        var template = OnboardingTemplate.Create(Guid.NewGuid(), "Test Template");
        template.AddStep("Step 1");
        template.AddStep("Step 2");
        template.AddStep("Optional", isRequired: false);
        return template;
    }

    [Fact]
    public void Start_SnapshotsSteps_InOrder_AndStartsNotStarted()
    {
        var template = BuildTemplate();
        var process = OnboardingProcess.Start(template, Guid.NewGuid());

        Assert.Equal(3, process.TotalSteps);
        Assert.Equal(OnboardingStatus.NotStarted, process.Status);
        Assert.Equal(new[] { 1, 2, 3 }, process.Steps.Select(s => s.Order).ToArray());
    }

    [Fact]
    public void CompletingOneStep_MovesToInProgress()
    {
        var template = BuildTemplate();
        var process = OnboardingProcess.Start(template, Guid.NewGuid());
        var first = process.Steps[0];

        process.CompleteStep(first.Id, DateTime.UtcNow);

        Assert.Equal(OnboardingStatus.InProgress, process.Status);
        Assert.Equal(1, process.CompletedSteps);
    }

    [Fact]
    public void CompletingAllRequiredSteps_CompletesProcess_EvenIfOptionalRemains()
    {
        var template = BuildTemplate();
        var process = OnboardingProcess.Start(template, Guid.NewGuid());

        foreach (var step in process.Steps.Where(s => s.IsRequired))
            process.CompleteStep(step.Id, DateTime.UtcNow);

        Assert.Equal(OnboardingStatus.Completed, process.Status);
        Assert.NotNull(process.CompletedAtUtc);
    }

    [Fact]
    public void ReopeningARequiredStep_MovesBackToInProgress()
    {
        var template = BuildTemplate();
        var process = OnboardingProcess.Start(template, Guid.NewGuid());
        foreach (var step in process.Steps.Where(s => s.IsRequired))
            process.CompleteStep(step.Id, DateTime.UtcNow);
        Assert.Equal(OnboardingStatus.Completed, process.Status);

        process.ReopenStep(process.Steps.First(s => s.IsRequired).Id);

        Assert.Equal(OnboardingStatus.InProgress, process.Status);
        Assert.Null(process.CompletedAtUtc);
    }
}
