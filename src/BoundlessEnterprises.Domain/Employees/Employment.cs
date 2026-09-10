using BoundlessEnterprises.Domain.Common;

namespace BoundlessEnterprises.Domain.Employees;

/// <summary>
/// A position an employee holds at a company: title, department, type, and dates.
/// An employee can accumulate multiple employments (history); one is primary.
/// </summary>
public class Employment : AuditableEntity
{
    private Employment() { }

    private Employment(
        Guid employeeId,
        Guid companyId,
        string title,
        string? department,
        EmploymentType type,
        DateOnly startDate,
        bool isPrimary)
    {
        EmployeeId = employeeId;
        CompanyId = companyId;
        Title = title;
        Department = department;
        Type = type;
        StartDate = startDate;
        IsPrimary = isPrimary;
    }

    public Guid EmployeeId { get; private set; }

    /// <summary>The company this position belongs to (BusinessId).</summary>
    public Guid CompanyId { get; private set; }

    public string Title { get; private set; } = string.Empty;
    public string? Department { get; private set; }
    public EmploymentType Type { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public bool IsPrimary { get; private set; }

    public bool IsActive => EndDate is null;

    public static Employment Create(
        Guid employeeId,
        Guid companyId,
        string title,
        string? department,
        EmploymentType type,
        DateOnly startDate,
        bool isPrimary = false)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Employment title is required.", nameof(title));

        return new Employment(employeeId, companyId, title.Trim(), department, type, startDate, isPrimary);
    }

    public void End(DateOnly endDate) => EndDate = endDate;
    public void MakePrimary() => IsPrimary = true;
    public void ClearPrimary() => IsPrimary = false;
}
