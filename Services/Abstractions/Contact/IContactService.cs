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

    /// <summary>
    /// Sends a message submitted through a public profile's contact form to that
    /// profile owner's email address, resolved from the given profile slug.
    /// </summary>

    /// <param name="dto">The contact message submitted by the visitor.</param>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no published profile matches <paramref name="profileSlug"/>.
    /// </exception>
    Task SendToProfileOwnerAsync(ContactProfileOwnerDto dto);
}
