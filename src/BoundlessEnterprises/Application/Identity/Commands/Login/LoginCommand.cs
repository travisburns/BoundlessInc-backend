using BoundlessEnterprises.Application.Identity.DTOs;
using MediatR;

namespace BoundlessEnterprises.Application.Identity.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<AuthResultDto>;
