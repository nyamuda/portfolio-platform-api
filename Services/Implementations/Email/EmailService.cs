using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using PortfolioPlatform.Api.Models;
using PortfolioPlatform.Api.Services.Abstractions.Email;

namespace PortfolioPlatform.Api.Services.Implementations.Email;

public class EmailService(
    IOptions<SmtpSettings> options,
    IWebHostEnvironment environment,
    ILogger<EmailService> logger
) : IEmailService
{
    private readonly SmtpSettings _smtpSettings = options.Value;
    private readonly IWebHostEnvironment _environment = environment;
    private readonly ILogger<EmailService> _logger = logger;

    /// <inheritdoc/>
    public async Task SendAsync(EmailMessage emailMessage)
    {
        if (!HasUsableSmtpSettings())
        {
            // Local development often starts before a real SMTP account is wired in.
            // In that case we skip delivery instead of breaking registration or
            // password reset. Production must fail loudly so missing email setup is
            // caught before users depend on it.
            if (_environment.IsDevelopment())
            {
                _logger.LogWarning(
                    "Email delivery skipped because SMTP settings are not configured. Subject: {Subject}. Recipient: {RecipientEmail}.",
                    emailMessage.Subject,
                    emailMessage.RecipientEmail
                );
                return;
            }

            throw new InvalidOperationException(
                "SMTP settings are not configured. Add Authentication:SmtpSettings before sending email."
            );
        }

        var messageToSend = new MimeMessage();
        messageToSend
            .From
            .Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
        messageToSend
            .To
            .Add(new MailboxAddress(emailMessage.RecipientName, emailMessage.RecipientEmail));
        messageToSend.Subject = emailMessage.Subject;

        // Account emails use prepared HTML templates, so send the body as HTML.
        messageToSend.Body = new TextPart("html") { Text = emailMessage.HtmlBody };

        // Keep the SMTP interaction small and explicit: connect, authenticate,
        // send, and disconnect. This mirrors the proven API flow and makes any
        // provider error easier to identify from logs.
        using var client = new SmtpClient();
        await client.ConnectAsync(_smtpSettings.Host, 587, false);
        await client.AuthenticateAsync(_smtpSettings.SenderEmail, _smtpSettings.Password);
        await client.SendAsync(messageToSend);
        await client.DisconnectAsync(true);
    }

    /// <summary>
    /// Checks whether SMTP settings look real enough to attempt delivery.
    /// </summary>
    /// <remarks>
    /// This deliberately catches placeholder values from appsettings. Without this
    /// guard, MailKit tries to connect to smtp.example.com or authenticate with a
    /// fake password and the user sees a generic server error.
    /// </remarks>
    private bool HasUsableSmtpSettings()
    {
        return HasRealValue(_smtpSettings.SenderName)
            && HasRealValue(_smtpSettings.SenderEmail)
            && HasRealValue(_smtpSettings.Password)
            && HasRealValue(_smtpSettings.Host)
            && !_smtpSettings.SenderEmail.EndsWith("@example.com", StringComparison.OrdinalIgnoreCase)
            && !_smtpSettings.Host.Equals("smtp.example.com", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Returns false for empty strings and obvious placeholder values.
    /// </summary>
    private static bool HasRealValue(string? value)
    {
        return !string.IsNullOrWhiteSpace(value)
            && !value.Contains("replace", StringComparison.OrdinalIgnoreCase)
            && !value.Contains("example", StringComparison.OrdinalIgnoreCase);
    }
}
