using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Identity.DTOs;
using BoundlessEnterprises.Application.Identity.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Identity.Commands.Login;

public class LoginHandler : IRequestHandler<LoginCommand, AuthResultDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwt;
    private readonly IDateTimeProvider _clock;

    public LoginHandler(
        IApplicationDbContext db,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwt,
        IDateTimeProvider clock)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwt = jwt;
        _clock = clock;
    }

    public async Task<AuthResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _db.Users
            .Include(u => u.Memberships)
            .ThenInclude(m => m.Roles)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        // Uniform failure to avoid leaking which accounts exist.
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("This account is not active.");

        var profile = await UserProfileReader.BuildAsync(_db, user.Id, cancellationToken)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        var roleNames = profile.Companies.SelectMany(c => c.Roles).Distinct();
        var token = _jwt.CreateToken(user, user.Memberships, roleNames);

        user.RecordLogin(_clock.UtcNow);
        await _db.SaveChangesAsync(cancellationToken);

        return new AuthResultDto
        {
            Token = token.Token,
            ExpiresAtUtc = token.ExpiresAtUtc,
            User = profile,
        };
    }
}
