using PortfolioPlatform.Api.Dtos.Contact;

namespace PortfolioPlatform.Api.Services.Abstractions.Email;

/// <summary>
/// Builds HTML email templates used by account and contact workflows.
/// </summary>
public interface IEmailTemplateBuilder
{
    /// <summary>
    /// Builds the password reset email containing an OTP.
    /// </summary>
    string BuildPasswordResetRequestTemplate(string recipientName, string otp);

    /// <summary>
    /// Builds the email verification email containing an OTP.
    /// </summary>
    string BuildEmailVerificationRequestTemplate(string recipientName, string otp);

    /// <summary>
    /// Builds the contact form notification email sent to the site owner.
    /// </summary>
    string BuildContactFormMessageTemplate(ContactDto contactDto);
}
