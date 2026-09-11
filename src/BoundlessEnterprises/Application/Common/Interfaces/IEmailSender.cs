namespace BoundlessEnterprises.Application.Common.Interfaces;

/// <summary>Sends transactional email (password resets, invitations, ...).</summary>
public interface IEmailSender
{
    Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
}
