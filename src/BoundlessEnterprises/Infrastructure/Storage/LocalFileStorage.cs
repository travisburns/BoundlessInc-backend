using BoundlessEnterprises.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace BoundlessEnterprises.Infrastructure.Storage;

/// <summary>
/// Stores uploaded files on the local filesystem under a configured root
/// (Storage:Root, default "storage"). Suitable for development and single-node
/// deployments; swap for a blob-storage implementation in production.
/// </summary>
public sealed class LocalFileStorage : IFileStorage
{
    private readonly string _root;

    public LocalFileStorage(IConfiguration configuration)
    {
        var configured = configuration["Storage:Root"];
        _root = string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(AppContext.BaseDirectory, "storage")
            : configured;
        Directory.CreateDirectory(_root);
    }

    public async Task<string> SaveAsync(Stream content, CancellationToken cancellationToken = default)
    {
        var key = Guid.NewGuid().ToString("N");
        var path = PathFor(key);
        await using var file = File.Create(path);
        await content.CopyToAsync(file, cancellationToken);
        return key;
    }

    public Task<Stream?> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var path = PathFor(storageKey);
        Stream? stream = File.Exists(path) ? File.OpenRead(path) : null;
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var path = PathFor(storageKey);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    private string PathFor(string key)
    {
        // Keys are GUIDs (no separators); guard against traversal regardless.
        var safe = Path.GetFileName(key);
        return Path.Combine(_root, safe);
    }
}
