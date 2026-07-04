namespace PortfolioPlatform.Api.Models;

/// <summary>
/// Email message prepared by application services before SMTP delivery.
/// </summary>
/// <remarks>
/// Services build this object when they know what needs to be sent, then the email delivery service
/// handles the actual SMTP work. Keeping this model small makes email sending easy to test and reuse.
/// </remarks>
public class EmailMessage
{
    /// <summary>
    /// Human-readable recipient name used in templates and friendly email greetings.
    /// </summary>
    public required string RecipientName { get; set; }

    /// <summary>
    /// Recipient email address that receives the message.
    /// </summary>
    public required string RecipientEmail { get; set; }

    /// <summary>
    /// Subject line shown in the recipient's inbox.
    /// </summary>
    public required string Subject { get; set; }

    /// <summary>
    /// HTML body sent to the recipient.
    /// </summary>
    public required string HtmlBody { get; set; }
}
