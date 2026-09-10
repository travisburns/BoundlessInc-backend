namespace BoundlessEnterprises.Application.Common.Exceptions;

/// <summary>Thrown when a requested aggregate does not exist. Maps to HTTP 404.</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string name, object key)
        : base($"{name} ({key}) was not found.") { }
}
