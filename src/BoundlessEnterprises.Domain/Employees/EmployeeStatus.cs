namespace BoundlessEnterprises.Domain.Employees;

/// <summary>Employment lifecycle status of an employee record.</summary>
public enum EmployeeStatus
{
    /// <summary>Record created; onboarding not yet complete.</summary>
    Pending = 0,

    /// <summary>Actively employed.</summary>
    Active = 1,

    /// <summary>Temporarily away (leave of absence).</summary>
    OnLeave = 2,

    /// <summary>No longer employed.</summary>
    Terminated = 3,
}
