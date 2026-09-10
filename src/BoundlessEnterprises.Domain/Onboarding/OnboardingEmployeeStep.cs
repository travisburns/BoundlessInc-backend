using BoundlessEnterprises.Domain.Common;

namespace BoundlessEnterprises.Domain.Onboarding;

/// <summary>
/// A step within a running onboarding process for a specific employee. Snapshots
/// the template step's name/order/required flag so template edits do not rewrite
/// history, and tracks completion.
/// </summary>
public class OnboardingEmployeeStep : Entity
{
    private OnboardingEmployeeStep() { }

    internal OnboardingEmployeeStep(Guid processId, int order, string name, bool isRequired)
    {
        ProcessId = processId;
        Order = order;
        Name = name;
        IsRequired = isRequired;
    }

    public Guid ProcessId { get; private set; }
    public int Order { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public bool IsRequired { get; private set; }
    public bool IsCompleted { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }

    internal void Complete(DateTime whenUtc)
    {
        IsCompleted = true;
        CompletedAtUtc = whenUtc;
    }

    internal void Reopen()
    {
        IsCompleted = false;
        CompletedAtUtc = null;
    }
}
