using BoundlessEnterprises.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace BoundlessEnterprises.Infrastructure.Email;

/// <summary>
/// Development email sender that logs messages instead of delivering them.
/// Swap for an SMTP / provider implementation in production.
/// </summary>
public sealed class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("EMAIL → {To} | {Subject}\n{Body}", to, subject, htmlBody);
        return Task.CompletedTask;
    }
}
