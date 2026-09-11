using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Identity.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Identity.Commands.ResetPassword;

public class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IDateTimeProvider _clock;

    public ResetPasswordHandler(IApplicationDbContext db, IPasswordHasher passwordHasher, IDateTimeProvider clock)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _clock = clock;
    }

    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        var tokenHash = ResetTokens.Hash(request.Token);
        if (user is null || !user.IsResetTokenValid(tokenHash, _clock.UtcNow))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["token"] = new[] { "The reset link is invalid or has expired." },
            });

        user.SetPasswordHash(_passwordHasher.Hash(request.NewPassword));
        await _db.SaveChangesAsync(cancellationToken);
    }
}
