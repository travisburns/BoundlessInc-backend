namespace BoundlessEnterprises.Domain.Documents;

/// <summary>Classification of a document held by the platform.</summary>
public enum DocumentType
{
    Policy = 0,
    Agreement = 1,
    Onboarding = 2,
    Resource = 3,
}

/// <summary>State of a document assigned to an employee.</summary>
public enum AssignmentStatus
{
    Assigned = 0,
    Acknowledged = 1,
}
