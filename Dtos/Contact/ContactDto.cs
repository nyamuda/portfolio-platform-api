using System.ComponentModel.DataAnnotations;

namespace PortfolioPlatform.Api.Dtos.Contact;

/// <summary>
/// Represents a bounded message submitted through the public contact form.
/// Data annotations protect the email service even when a caller bypasses the frontend.
/// </summary>
public class ContactDto
{
    /// <summary>
    /// Name of the person sending the message.
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public required string Name { get; set; }

    /// <summary>
    /// Email address where the sender can be reached.
    /// </summary>
    [Required]
    [EmailAddress]
    [StringLength(254)]
    public required string Email { get; set; }

    /// <summary>
    /// Message topic selected or entered by the sender.
    /// </summary>
    [Required]
    [StringLength(80)]
    public required string Topic { get; set; }

    /// <summary>
    /// Main message body.
    /// </summary>
    [Required]
    [StringLength(3000, MinimumLength = 20)]
    public required string Message { get; set; }
}
