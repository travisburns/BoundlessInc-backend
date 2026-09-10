using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Integrations.Services;
using BoundlessEnterprises.Domain.Integrations;
using BoundlessEnterprises.Domain.Intelligence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Intelligence.Commands.IngestEvent;

/// <summary>
/// Ingests a normalized business event from an operating application. Authorized
/// by the integration API key (not a user token) — this is the upward
/// apps → center flow, so the company is resolved from the key.
/// </summary>
public record IngestEventCommand(
    string ApiKey,
    string EventType,
    decimal Revenue,
    string Currency,
    DateTime? OccurredAtUtc,
    string? ExternalRef) : IRequest<Guid>;

public class IngestEventHandler : IRequestHandler<IngestEventCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;

    public IngestEventHandler(IApplicationDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<Guid> Handle(IngestEventCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ApiKey))
            throw new UnauthorizedAccessException("Missing API key.");

        var hash = ApiKeys.Hash(request.ApiKey.Trim());
        var integration = await _db.Integrations
            .FirstOrDefaultAsync(i => i.ApiKeyHash == hash, cancellationToken);

        if (integration is null || integration.Status != IntegrationStatus.Active)
            throw new UnauthorizedAccessException("Invalid or disabled API key.");

        var occurred = request.OccurredAtUtc ?? _clock.UtcNow;
        var businessEvent = BusinessEvent.Create(
            integration.CompanyId, integration.Id, request.EventType,
            request.Revenue, request.Currency, occurred, request.ExternalRef);

        _db.BusinessEvents.Add(businessEvent);
        integration.RecordEvent(_clock.UtcNow);

        await _db.SaveChangesAsync(cancellationToken);
        return businessEvent.Id;
    }
}
