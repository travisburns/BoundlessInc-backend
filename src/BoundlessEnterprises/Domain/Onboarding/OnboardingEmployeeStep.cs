using BoundlessEnterprises.Domain.Common;

namespace BoundlessEnterprises.Domain.Onboarding;

/// <summary>
/// A step within a running onboarding process for a specific employee. Snapshots
/// the template step's name/order/kind/required flag so template edits do not
/// rewrite history, tracks completion, and stores the data the hire submitted.
/// </summary>
public class OnboardingEmployeeStep : Entity
{
    private OnboardingEmployeeStep() { }

    internal OnboardingEmployeeStep(Guid processId, int order, string name, bool isRequired, OnboardingStepKind kind)
    {
        ProcessId = processId;
        Order = order;
        Name = name;
        IsRequired = isRequired;
        Kind = kind;
    }

    public Guid ProcessId { get; private set; }
    public int Order { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public bool IsRequired { get; private set; }

    /// <summary>What form the wizard rendered for this step.</summary>
    public OnboardingStepKind Kind { get; private set; }

    public bool IsCompleted { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }

    /// <summary>The data the hire submitted for this step, stored as JSON.</summary>
    public string? ResponseJson { get; private set; }

    internal void Complete(DateTime whenUtc, string? responseJson = null)
    {
        IsCompleted = true;
        CompletedAtUtc = whenUtc;
        if (responseJson is not null)
            ResponseJson = responseJson;
    }

    internal void Reopen()
    {
        IsCompleted = false;
        CompletedAtUtc = null;
    }
}
