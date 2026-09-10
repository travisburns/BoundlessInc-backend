using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Identity.DTOs;
using BoundlessEnterprises.Application.Identity.Services;
using MediatR;

namespace BoundlessEnterprises.Application.Identity.Queries.GetCurrentUser;

public class GetCurrentUserHandler : IRequestHandler<GetCurrentUserQuery, UserProfileDto>
{
    private readonly IApplicationDbContext _db;

    public GetCurrentUserHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<UserProfileDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        return await UserProfileReader.BuildAsync(_db, request.UserId, cancellationToken)
            ?? throw new NotFoundException("User", request.UserId);
    }
}
