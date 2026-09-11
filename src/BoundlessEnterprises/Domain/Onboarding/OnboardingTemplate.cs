using BoundlessEnterprises.Domain.Common;

namespace BoundlessEnterprises.Domain.Onboarding;

/// <summary>
/// A reusable onboarding template owned by a company. The template defines the
/// ordered steps; the process/tracking is shared engine behavior, so companies
/// customize the template without re-implementing onboarding.
/// </summary>
public class OnboardingTemplate : AuditableEntity
{
    private readonly List<OnboardingStep> _steps = new();

    private OnboardingTemplate() { }

    private OnboardingTemplate(Guid companyId, string name, string? description)
    {
        CompanyId = companyId;
        Name = name;
        Description = description;
        IsActive = true;
    }

    public Guid CompanyId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    public IReadOnlyList<OnboardingStep> Steps =>
        _steps.OrderBy(s => s.Order).ToList();

    public static OnboardingTemplate Create(Guid companyId, string name, string? description = null)
    {
        if (companyId == Guid.Empty)
            throw new ArgumentException("CompanyId is required.", nameof(companyId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Template name is required.", nameof(name));

        return new OnboardingTemplate(companyId, name.Trim(), description);
    }

    /// <summary>Appends a step; order is assigned automatically.</summary>
    public OnboardingStep AddStep(
        string name,
        string? description = null,
        bool isRequired = true,
        OnboardingStepKind kind = OnboardingStepKind.Generic)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Step name is required.", nameof(name));

        var order = _steps.Count == 0 ? 1 : _steps.Max(s => s.Order) + 1;
        var step = new OnboardingStep(Id, order, name.Trim(), description, isRequired, kind);
        _steps.Add(step);
        return step;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
