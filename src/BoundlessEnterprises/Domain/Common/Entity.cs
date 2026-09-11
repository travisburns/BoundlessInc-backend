namespace BoundlessEnterprises.Domain.Common;

/// <summary>
/// Base class for all domain entities. Identity is a <see cref="Guid"/> generated
/// in the domain so entities are valid before they touch the database.
/// </summary>
public abstract class Entity
{
    private readonly List<DomainEvent> _domainEvents = new();

    protected Entity()
    {
        Id = Guid.NewGuid();
    }

    protected Entity(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; protected set; }

    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void Raise(DomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
