using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Work.DTOs;
using BoundlessEnterprises.Domain.Work;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Work.Commands.CreateAssignment;

/// <summary>Creates an assignment for a ring (assigns the next code). Platform-admin or the ring holder.</summary>
public record CreateAssignmentCommand(
    Guid RingId,
    string Title,
    string Type,
    string Priority,
    string? Summary,
    string? Objective,
    string? AssigneeName,
    string? IssuedBy,
    Guid? CompanyId,
    DateOnly? StartDate,
    DateOnly? DueDate,
    string? Deliverable,
    IReadOnlyList<string>? AcceptanceCriteria,
    string? Dependencies,
    string? Blockers,
    string? NextStep,
    bool ReviewRequired,
    IReadOnlyList<string>? References,
    IReadOnlyList<string>? Tags,
    string? DomainDataJson) : IRequest<AssignmentDto>;

public sealed class CreateAssignmentValidator : AbstractValidator<CreateAssignmentCommand>
{
    public CreateAssignmentValidator()
    {
        RuleFor(x => x.RingId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Type).NotEmpty();
        RuleFor(x => x.Priority).NotEmpty();
    }
}

public class CreateAssignmentHandler : IRequestHandler<CreateAssignmentCommand, AssignmentDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _clock;

    public CreateAssignmentHandler(IApplicationDbContext db, ICurrentUser currentUser, IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<AssignmentDto> Handle(CreateAssignmentCommand request, CancellationToken cancellationToken)
    {
        var ring = await _db.Rings.FirstOrDefaultAsync(r => r.Id == request.RingId, cancellationToken)
            ?? throw new NotFoundException("Ring", request.RingId);

        if (!WorkAccess.CanManage(_currentUser, ring))
            throw new ForbiddenAccessException();

        var type = Enum.TryParse<AssignmentType>(request.Type, ignoreCase: true, out var t) ? t : AssignmentType.Create;
        var priority = Enum.TryParse<AssignmentPriority>(request.Priority, ignoreCase: true, out var p) ? p : AssignmentPriority.Normal;

        var assignment = Assignment.Create(ring, request.Title, type, priority, _clock.UtcNow);
        assignment.UpdateCore(
            request.Title, request.Summary, request.Objective, type, priority,
            request.AssigneeName ?? ring.HolderName, request.IssuedBy, request.CompanyId,
            request.StartDate, request.DueDate, request.Deliverable,
            request.AcceptanceCriteria ?? Array.Empty<string>(), request.Dependencies, request.Blockers,
            request.NextStep, request.ReviewRequired, request.References ?? Array.Empty<string>(),
            request.Tags ?? Array.Empty<string>(), request.DomainDataJson);

        _db.Assignments.Add(assignment);
        await _db.SaveChangesAsync(cancellationToken); // also persists ring.NextSequence++

        return AssignmentDto.FromEntity(assignment, ring.Name);
    }
}
