using System.Net;
using Microsoft.Extensions.Options;
using PortfolioPlatform.Api.Dtos.Contact;
using PortfolioPlatform.Api.Models.Auth;
using PortfolioPlatform.Api.Services.Abstractions.Email;

namespace PortfolioPlatform.Api.Services.Implementations.Email;

public class EmailTemplateBuilder(IOptions<Company> options) : IEmailTemplateBuilder
{
    private readonly Company _company = options.Value;

    /// <inheritdoc/>
    public string BuildPasswordResetRequestTemplate(string recipientName, string otp)
    {
        // Password reset and email verification share the same layout, but the copy
        // stays specific so the email feels clear to the person receiving it.
        return BuildOtpEmail(
            title: "Password Reset Request",
            heading: "Password reset code",
            recipientName: recipientName,
            intro: "We received a request to reset your password. Use the code below to continue.",
            otp: otp,
            footerNote: "If you did not request a password reset, you can safely ignore this email."
        );
    }

    /// <inheritdoc/>
    public string BuildEmailVerificationRequestTemplate(string recipientName, string otp)
    {
        // Reuse the OTP email shell so all account-code emails stay visually consistent.
        return BuildOtpEmail(
            title: "Email Verification Code",
            heading: "Verify your email",
            recipientName: recipientName,
            intro: "Use the code below to verify your email address.",
            otp: otp,
            footerNote: "If you did not request this verification code, you can safely ignore this email."
        );
    }

    /// <inheritdoc/>
    public string BuildContactFormMessageTemplate(ContactDto contactDto)
    {
        // Contact data comes from public input. Encode it before inserting it into
        // HTML so markup is displayed as text rather than interpreted by the email client.
        string name = WebUtility.HtmlEncode(contactDto.Name);
        string email = WebUtility.HtmlEncode(contactDto.Email);
        string topic = WebUtility.HtmlEncode(contactDto.Topic);

        // Preserve line breaks from the message textarea while still keeping the text HTML-safe.
        string message = WebUtility
            .HtmlEncode(contactDto.Message)
            .Replace("\r\n", "<br />")
            .Replace("\n", "<br />");

        // Keep the contact email intentionally simple. Its job is reliable delivery and easy reading,
        // not a full marketing template.
        return $@"
<!DOCTYPE html>
<html lang=""en"">
  <body style=""margin:0;padding:0;background:#f7fafc;font-family:Helvetica,Arial,sans-serif;"">
    <div style=""max-width:600px;margin:0 auto;padding:40px 16px;"">
      <div style=""background:#fff;border:1px solid #e2e8f0;border-radius:6px;padding:32px;"">
        <h1 style=""margin-top:0;font-size:24px;"">New contact message</h1>
        <p><strong>From:</strong> {name}</p>
        <p><strong>Email:</strong> <a href=""mailto:{email}"">{email}</a></p>
        <p><strong>Topic:</strong> {topic}</p>
        <p><strong>Message:</strong><br />{message}</p>
      </div>
      <p style=""text-align:center;color:#718096;font-size:14px;"">{WebUtility.HtmlEncode(_company.Email)}</p>
    </div>
  </body>
</html>";
    }

    private string BuildOtpEmail(
        string title,
        string heading,
        string recipientName,
        string intro,
        string otp,
        string footerNote
    )
    {
        // The text values are mostly internal, but encoding them keeps the helper safe
        // if a future caller passes user-controlled names or copy.
        string safeCompany = WebUtility.HtmlEncode(_company.Name);
        string safeName = WebUtility.HtmlEncode(recipientName);
        string safeIntro = WebUtility.HtmlEncode(intro);
        string safeFooter = WebUtility.HtmlEncode(footerNote);

        // Email clients are inconsistent with external CSS, so account emails use inline styles.
        return $@"
<!DOCTYPE html>
<html lang=""en"">
  <head>
    <meta charset=""UTF-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>{WebUtility.HtmlEncode(title)}</title>
  </head>
  <body style=""margin:0;padding:0;background:#f7fafc;font-family:Helvetica,Arial,sans-serif;"">
    <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" bgcolor=""#f7fafc"">
      <tr>
        <td align=""center"" style=""padding:40px 16px;"">
          <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""max-width:600px;width:100%;"">
            <tr>
              <td style=""background:#fff;border:1px solid #e2e8f0;border-radius:6px;padding:40px;"">
                <h1 style=""margin-top:0;font-size:24px;font-weight:700;"">{WebUtility.HtmlEncode(heading)}</h1>
                <p>Hi {safeName},</p>
                <p>{safeIntro}</p>
                <div style=""margin:32px 0;text-align:center;"">
                  <span style=""display:inline-block;padding:12px 24px;background:#edf2f7;border-radius:6px;font-size:24px;font-weight:bold;letter-spacing:4px;"">{otp}</span>
                </div>
                <p>Thank you,</p>
                <p>The {safeCompany} Team</p>
                <p style=""font-size:14px;color:#6c757d;margin-top:32px;"">{safeFooter}</p>
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </body>
</html>";
    }
}
