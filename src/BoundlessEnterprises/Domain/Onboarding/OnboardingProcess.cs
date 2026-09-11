using BoundlessEnterprises.Domain.Common;

namespace BoundlessEnterprises.Domain.Onboarding;

/// <summary>
/// A running onboarding process: one employee working through one template's
/// steps. The engine (status, progress, approvals, tracking) is shared; only the
/// template's step list differs per company.
/// </summary>
public class OnboardingProcess : AuditableEntity
{
    private readonly List<OnboardingEmployeeStep> _steps = new();

    private OnboardingProcess() { }

    private OnboardingProcess(Guid companyId, Guid employeeId, Guid templateId, string templateName)
    {
        CompanyId = companyId;
        EmployeeId = employeeId;
        TemplateId = templateId;
        TemplateName = templateName;
        Status = OnboardingStatus.NotStarted;
        StartedAtUtc = DateTime.UtcNow;
    }

    public Guid CompanyId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Guid TemplateId { get; private set; }
    public string TemplateName { get; private set; } = string.Empty;
    public OnboardingStatus Status { get; private set; }
    public DateTime StartedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }

    public IReadOnlyList<OnboardingEmployeeStep> Steps =>
        _steps.OrderBy(s => s.Order).ToList();

    public int TotalSteps => _steps.Count;
    public int CompletedSteps => _steps.Count(s => s.IsCompleted);

    /// <summary>
    /// Starts an onboarding process for an employee from a template, snapshotting
    /// the template's steps into trackable employee steps.
    /// </summary>
    public static OnboardingProcess Start(OnboardingTemplate template, Guid employeeId)
    {
        ArgumentNullException.ThrowIfNull(template);
        if (employeeId == Guid.Empty)
            throw new ArgumentException("EmployeeId is required.", nameof(employeeId));

        var process = new OnboardingProcess(template.CompanyId, employeeId, template.Id, template.Name);
        foreach (var step in template.Steps)
            process._steps.Add(new OnboardingEmployeeStep(process.Id, step.Order, step.Name, step.IsRequired));

        process.Recalculate();
        return process;
    }

    public void CompleteStep(Guid employeeStepId, DateTime whenUtc)
    {
        var step = _steps.FirstOrDefault(s => s.Id == employeeStepId)
            ?? throw new InvalidOperationException("Step not found in this process.");
        step.Complete(whenUtc);
        Recalculate();
    }

    public void ReopenStep(Guid employeeStepId)
    {
        var step = _steps.FirstOrDefault(s => s.Id == employeeStepId)
            ?? throw new InvalidOperationException("Step not found in this process.");
        step.Reopen();
        Recalculate();
    }

    private void Recalculate()
    {
        var requiredSteps = _steps.Where(s => s.IsRequired).ToList();
        var allRequiredDone = requiredSteps.Count > 0 && requiredSteps.All(s => s.IsCompleted);

        if (allRequiredDone)
        {
            Status = OnboardingStatus.Completed;
            CompletedAtUtc ??= DateTime.UtcNow;
        }
        else if (_steps.Any(s => s.IsCompleted))
        {
            Status = OnboardingStatus.InProgress;
            CompletedAtUtc = null;
        }
        else
        {
            Status = OnboardingStatus.NotStarted;
            CompletedAtUtc = null;
        }
    }
}
