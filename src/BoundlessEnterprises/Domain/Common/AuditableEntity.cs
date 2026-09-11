namespace BoundlessEnterprises.Domain.Common;

/// <summary>
/// An entity that carries creation and modification audit metadata.
/// Populated automatically by the persistence layer's SaveChanges interceptor.
/// </summary>
public abstract class AuditableEntity : Entity
{
    protected AuditableEntity() { }

    protected AuditableEntity(Guid id) : base(id) { }

    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }
}
