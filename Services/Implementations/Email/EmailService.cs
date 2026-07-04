using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using PortfolioPlatform.Api.Models;
using PortfolioPlatform.Api.Services.Abstractions.Email;

namespace PortfolioPlatform.Api.Services.Implementations.Email;

public class EmailService(IOptions<SmtpSettings> options) : IEmailService
{
    private readonly SmtpSettings _smtpSettings = options.Value;

    /// <inheritdoc/>
    public async Task SendAsync(EmailMessage emailMessage)
    {
        var messageToSend = new MimeMessage();
        messageToSend
            .From
            .Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
        messageToSend
            .To
            .Add(new MailboxAddress(emailMessage.RecipientName, emailMessage.RecipientEmail));
        messageToSend.Subject = emailMessage.Subject;

        // Authentication emails are HTML emails because the templates include simple branded layout.
        messageToSend.Body = new TextPart("html") { Text = emailMessage.HtmlBody };

        using var client = new SmtpClient();
        await client.ConnectAsync(_smtpSettings.Host, 587, false);
        await client.AuthenticateAsync(_smtpSettings.SenderEmail, _smtpSettings.Password);
        await client.SendAsync(messageToSend);
        await client.DisconnectAsync(true);
    }
}
