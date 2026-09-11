using BoundlessEnterprises.Domain.Integrations;

namespace BoundlessEnterprises.Application.Integrations.DTOs;

public record IntegrationDto
{
    public Guid Id { get; init; }
    public Guid CompanyId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ApiKeyPrefix { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int EventCount { get; init; }
    public DateTime? LastEventAtUtc { get; init; }
    public DateTime CreatedAtUtc { get; init; }

    public static IntegrationDto FromEntity(Integration i) => new()
    {
        Id = i.Id,
        CompanyId = i.CompanyId,
        Name = i.Name,
        ApiKeyPrefix = i.ApiKeyPrefix,
        Status = i.Status.ToString(),
        EventCount = i.EventCount,
        LastEventAtUtc = i.LastEventAtUtc,
        CreatedAtUtc = i.CreatedAtUtc,
    };
}

/// <summary>Returned only when an integration is created — includes the raw key once.</summary>
public record IntegrationCreatedDto
{
    public IntegrationDto Integration { get; init; } = new();
    public string ApiKey { get; init; } = string.Empty;
}
