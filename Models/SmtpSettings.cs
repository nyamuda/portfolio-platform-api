namespace PortfolioPlatform.Api.Models;

/// <summary>
/// SMTP settings used by the email delivery service.
/// </summary>
/// <remarks>
/// These values should come from appsettings for local development and from environment variables or
/// user-secrets in real deployments. Do not hard-code production credentials in source control.
/// </remarks>
public class SmtpSettings
{
    /// <summary>
    /// Display name used in outgoing sender details.
    /// </summary>
    public required string SenderName { get; set; }

    /// <summary>
    /// Email address used as the sender account.
    /// </summary>
    public required string SenderEmail { get; set; }

    /// <summary>
    /// SMTP password or app password for the sender account.
    /// </summary>
    public required string Password { get; set; }

    /// <summary>
    /// SMTP host name used to connect to the mail server.
    /// </summary>
    public required string Host { get; set; }
}
