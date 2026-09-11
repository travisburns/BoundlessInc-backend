using MediatR;

namespace BoundlessEnterprises.Application.Identity.Commands.ResetPassword;

public record ResetPasswordCommand(string Email, string Token, string NewPassword) : IRequest;
