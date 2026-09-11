using BoundlessEnterprises.Domain.Common;

namespace BoundlessEnterprises.Domain.Onboarding;

/// <summary>A single step definition within an onboarding template.</summary>
public class OnboardingStep : Entity
{
    private OnboardingStep() { }

    internal OnboardingStep(Guid templateId, int order, string name, string? description, bool isRequired, OnboardingStepKind kind)
    {
        TemplateId = templateId;
        Order = order;
        Name = name;
        Description = description;
        IsRequired = isRequired;
        Kind = kind;
    }

    public Guid TemplateId { get; private set; }
    public int Order { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsRequired { get; private set; }

    /// <summary>What the wizard renders for this step (a form, an acknowledgement, or a checklist item).</summary>
    public OnboardingStepKind Kind { get; private set; }
}
