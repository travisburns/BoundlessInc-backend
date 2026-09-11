using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Identity.Commands.ForgotPassword;
using BoundlessEnterprises.Application.Identity.Commands.Login;
using BoundlessEnterprises.Application.Identity.Commands.ResetPassword;
using BoundlessEnterprises.Application.Identity.Queries.GetCurrentUser;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BoundlessEnterprises.Api.Endpoints.Auth;

/// <summary>Authentication: login, password reset, and the current-user profile.</summary>
public sealed class AuthEndpoints : IEndpointModule
{
    public record LoginRequest(string Email, string Password);
    public record ForgotPasswordRequest(string Email);
    public record ResetPasswordRequest(string Email, string Token, string NewPassword);

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/login", async (LoginRequest body, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new LoginCommand(body.Email, body.Password), ct)))
            .WithName("Login")
            .WithSummary("Authenticate with email and password.");

        group.MapPost("/forgot-password", async (ForgotPasswordRequest body, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new ForgotPasswordCommand(body.Email), ct);
            return Results.NoContent();
        })
            .WithName("ForgotPassword")
            .WithSummary("Begin the password-reset flow (always succeeds).");

        group.MapPost("/reset-password", async (ResetPasswordRequest body, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new ResetPasswordCommand(body.Email, body.Token, body.NewPassword), ct);
            return Results.NoContent();
        })
            .WithName("ResetPassword")
            .WithSummary("Complete a password reset with a valid token.");

        group.MapGet("/me", async (ICurrentUser currentUser, ISender sender, CancellationToken ct) =>
        {
            if (currentUser.UserId is not { } userId)
                return Results.Unauthorized();
            return Results.Ok(await sender.Send(new GetCurrentUserQuery(userId), ct));
        })
            .RequireAuthorization()
            .WithName("GetCurrentUser")
            .WithSummary("Return the authenticated user's profile and memberships.");
    }
}
