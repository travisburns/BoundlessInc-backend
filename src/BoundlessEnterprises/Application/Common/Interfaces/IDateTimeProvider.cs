namespace BoundlessEnterprises.Application.Common.Interfaces;

/// <summary>Abstracts the system clock so use cases and tests are deterministic.</summary>
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
