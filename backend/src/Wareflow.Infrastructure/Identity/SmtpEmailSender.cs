using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wareflow.Application.Authentication;

namespace Wareflow.Infrastructure.Identity;

public sealed class SmtpEmailSender(
    IOptions<SmtpOptions> smtpOptions,
    IOptions<IdentityEmailOptions> emailOptions,
    ILogger<SmtpEmailSender> logger) : IEmailSender
{
    public async Task SendAsync(
        string recipient,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken)
    {
        var smtp = smtpOptions.Value;
        using var client = new SmtpClient(smtp.Host, smtp.Port)
        {
            EnableSsl = smtp.UseSsl
        };

        if (!string.IsNullOrWhiteSpace(smtp.UserName))
        {
            client.Credentials = new NetworkCredential(smtp.UserName, smtp.Password);
        }

        using var message = new MailMessage
        {
            From = new MailAddress(
                emailOptions.Value.FromAddress,
                emailOptions.Value.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        message.To.Add(recipient);

        try
        {
            await client.SendMailAsync(message, cancellationToken);
            logger.LogInformation("Authentication email delivered to {Recipient}", recipient);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Authentication email delivery failed for {Recipient}",
                recipient);
            throw;
        }
    }
}
