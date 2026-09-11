using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Identity.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Identity.Commands.ForgotPassword;

public class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand>
{
    private const int TokenLifetimeMinutes = 60;

    private readonly IApplicationDbContext _db;
    private readonly IEmailSender _email;
    private readonly IDateTimeProvider _clock;

    public ForgotPasswordHandler(IApplicationDbContext db, IEmailSender email, IDateTimeProvider clock)
    {
        _db = db;
        _email = email;
        _clock = clock;
    }

    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        if (user is null || !user.IsActive)
            return; // Silent success — no enumeration.

        var token = ResetTokens.Generate();
        user.SetPasswordResetToken(ResetTokens.Hash(token), _clock.UtcNow.AddMinutes(TokenLifetimeMinutes));
        await _db.SaveChangesAsync(cancellationToken);

        var body =
            $"<p>A password reset was requested for your Boundless Enterprises account.</p>" +
            $"<p>Use this token to reset your password (valid for {TokenLifetimeMinutes} minutes):</p>" +
            $"<p><strong>{token}</strong></p>" +
            "<p>If you did not request this, you can ignore this message.</p>";

        await _email.SendAsync(user.Email, "Reset your Boundless Enterprises password", body, cancellationToken);
    }
}
