using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Work.DTOs;
using BoundlessEnterprises.Domain.Work;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Work.Commands.UpdateAssignment;

/// <summary>Edits an assignment's core fields. Platform-admin or the ring holder.</summary>
public record UpdateAssignmentCommand(
    Guid Id,
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

public sealed class UpdateAssignmentValidator : AbstractValidator<UpdateAssignmentCommand>
{
    public UpdateAssignmentValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Type).NotEmpty();
        RuleFor(x => x.Priority).NotEmpty();
    }
}

public class UpdateAssignmentHandler : IRequestHandler<UpdateAssignmentCommand, AssignmentDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public UpdateAssignmentHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<AssignmentDto> Handle(UpdateAssignmentCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _db.Assignments.Include(a => a.Updates)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Assignment", request.Id);

        var ring = await _db.Rings.FirstOrDefaultAsync(r => r.Id == assignment.RingId, cancellationToken)
            ?? throw new NotFoundException("Ring", assignment.RingId);

        if (!WorkAccess.CanManage(_currentUser, ring))
            throw new ForbiddenAccessException();

        var type = Enum.TryParse<AssignmentType>(request.Type, ignoreCase: true, out var t) ? t : assignment.Type;
        var priority = Enum.TryParse<AssignmentPriority>(request.Priority, ignoreCase: true, out var p) ? p : assignment.Priority;

        assignment.UpdateCore(
            request.Title, request.Summary, request.Objective, type, priority,
            request.AssigneeName, request.IssuedBy, request.CompanyId, request.StartDate, request.DueDate,
            request.Deliverable, request.AcceptanceCriteria ?? Array.Empty<string>(), request.Dependencies,
            request.Blockers, request.NextStep, request.ReviewRequired, request.References ?? Array.Empty<string>(),
            request.Tags ?? Array.Empty<string>(), request.DomainDataJson);

        await _db.SaveChangesAsync(cancellationToken);
        return AssignmentDto.FromEntity(assignment, ring.Name);
    }
}
