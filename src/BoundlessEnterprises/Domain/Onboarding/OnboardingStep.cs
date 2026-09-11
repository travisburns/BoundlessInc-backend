using BoundlessEnterprises.Domain.Common;

namespace BoundlessEnterprises.Domain.Onboarding;

/// <summary>A single step definition within an onboarding template.</summary>
public class OnboardingStep : Entity
{
    private OnboardingStep() { }

    internal OnboardingStep(Guid templateId, int order, string name, string? description, bool isRequired)
    {
        TemplateId = templateId;
        Order = order;
        Name = name;
        Description = description;
        IsRequired = isRequired;
    }

    public Guid TemplateId { get; private set; }
    public int Order { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsRequired { get; private set; }
}
