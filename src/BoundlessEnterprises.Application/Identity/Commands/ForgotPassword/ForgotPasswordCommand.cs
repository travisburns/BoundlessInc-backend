using MediatR;

namespace BoundlessEnterprises.Application.Identity.Commands.ForgotPassword;

/// <summary>
/// Starts the password-reset flow. Always succeeds from the caller's point of
/// view, whether or not the email maps to an account, to avoid user enumeration.
/// </summary>
public record ForgotPasswordCommand(string Email) : IRequest;
