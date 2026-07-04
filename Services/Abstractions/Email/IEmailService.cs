using PortfolioPlatform.Api.Models;

namespace PortfolioPlatform.Api.Services.Abstractions.Email;

/// <summary>
/// Sends prepared email messages through the configured SMTP provider.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an HTML email message.
    /// </summary>
    /// <param name="emailMessage">Message recipient, subject, and body.</param>
    Task SendAsync(EmailMessage emailMessage);
}
