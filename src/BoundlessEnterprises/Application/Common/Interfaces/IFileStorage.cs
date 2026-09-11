namespace BoundlessEnterprises.Application.Common.Interfaces;

/// <summary>Stores and retrieves uploaded file bytes, keyed by an opaque storage key.</summary>
public interface IFileStorage
{
    /// <summary>Persists the content and returns a storage key.</summary>
    Task<string> SaveAsync(Stream content, CancellationToken cancellationToken = default);

    /// <summary>Opens the stored content for reading, or null if it doesn't exist.</summary>
    Task<Stream?> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default);

    Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);
}
