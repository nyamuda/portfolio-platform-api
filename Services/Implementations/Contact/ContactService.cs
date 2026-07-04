using Microsoft.Extensions.Options;
using PortfolioPlatform.Api.Dtos.Contact;
using PortfolioPlatform.Api.Models;
using PortfolioPlatform.Api.Models.Auth;
using PortfolioPlatform.Api.Services.Abstractions.Contact;
using PortfolioPlatform.Api.Services.Abstractions.Email;

namespace PortfolioPlatform.Api.Services.Implementations.Contact;

public class ContactService(
    IEmailService emailService,
    IEmailTemplateBuilder templateBuilder,
    IOptions<Company> options
) : IContactService
{
    private readonly IEmailService _emailService = emailService;
    private readonly IEmailTemplateBuilder _emailTemplateBuilder = templateBuilder;
    private readonly Company _company = options.Value;

    /// <inheritdoc/>
    public async Task SendAsync(ContactDto dto)
    {
        // Build the email body from the validated public form input.
        string htmlBody = _emailTemplateBuilder.BuildContactFormMessageTemplate(dto);

        // Contact messages are delivered to the configured owner/support email.
        // They are not stored in the database because SMTP delivery is the source pattern here.
        EmailMessage emailMessage = new()
        {
            RecipientName = _company.Name,
            RecipientEmail = _company.Email,
            Subject = "Contact Form Message",
            HtmlBody = htmlBody
        };

        // Send the message to the configured support/site-owner email address.
        await _emailService.SendAsync(emailMessage);
    }
}
