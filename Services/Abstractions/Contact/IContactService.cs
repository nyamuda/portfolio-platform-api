using PortfolioPlatform.Api.Dtos.Contact;

namespace PortfolioPlatform.Api.Services.Abstractions.Contact;

/// <summary>
/// Handles messages submitted through public contact forms.
/// </summary>
public interface IContactService
{
    /// <summary>
    /// Sends a public contact form message to the configured site-owner email address.
    /// </summary>
    /// <param name="dto">The contact message submitted by the visitor.</param>
    Task SendAsync(ContactDto dto);
}
