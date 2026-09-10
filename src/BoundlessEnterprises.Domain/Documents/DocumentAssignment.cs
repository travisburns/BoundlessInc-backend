using BoundlessEnterprises.Domain.Common;

namespace BoundlessEnterprises.Domain.Documents;

/// <summary>
/// Assignment of a document to an employee, tracking acknowledgment. Carries the
/// CompanyId so assignments stay within the owning company's boundary.
/// </summary>
public class DocumentAssignment : AuditableEntity
{
    private DocumentAssignment() { }

    private DocumentAssignment(Guid companyId, Guid documentId, Guid employeeId)
    {
        CompanyId = companyId;
        DocumentId = documentId;
        EmployeeId = employeeId;
        Status = AssignmentStatus.Assigned;
        AssignedAtUtc = DateTime.UtcNow;
    }

    public Guid CompanyId { get; private set; }
    public Guid DocumentId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public AssignmentStatus Status { get; private set; }
    public DateTime AssignedAtUtc { get; private set; }
    public DateTime? AcknowledgedAtUtc { get; private set; }

    public static DocumentAssignment Create(Guid companyId, Guid documentId, Guid employeeId)
    {
        if (companyId == Guid.Empty)
            throw new ArgumentException("CompanyId is required.", nameof(companyId));
        if (documentId == Guid.Empty)
            throw new ArgumentException("DocumentId is required.", nameof(documentId));
        if (employeeId == Guid.Empty)
            throw new ArgumentException("EmployeeId is required.", nameof(employeeId));

        return new DocumentAssignment(companyId, documentId, employeeId);
    }

    public void Acknowledge(DateTime whenUtc)
    {
        Status = AssignmentStatus.Acknowledged;
        AcknowledgedAtUtc = whenUtc;
    }
}
