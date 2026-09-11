using BoundlessEnterprises.Application.Identity.DTOs;
using MediatR;

namespace BoundlessEnterprises.Application.Identity.Queries.GetCurrentUser;

/// <summary>Resolves the profile of the currently authenticated user.</summary>
public record GetCurrentUserQuery(Guid UserId) : IRequest<UserProfileDto>;
