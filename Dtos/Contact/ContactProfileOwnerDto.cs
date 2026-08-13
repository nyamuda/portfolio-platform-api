using System.ComponentModel.DataAnnotations;

namespace PortfolioPlatform.Api.Dtos.Contact;

/// <summary>
/// Represents a message submitted through the public
/// contact form to a profile owner.
/// </summary>
public class ContactProfileOwnerDto
{
    /// <summary>
    /// Name of the person sending the message.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Email address where the message will be delivered to the
    /// profile owner.
    /// </summary>
    [Required]
    public required string RecipientEmail { get; set; }

    /// <summary>
    /// Email of the person sending the message. This is where the
    /// profile owner can reply.
    /// </summary>
    [Required]
    public required string SenderEmail { get; set; }

    /// <summary>
    /// Message topic selected or entered by the sender.
    /// </summary>
    public string Topic { get; set; }

    /// <summary>
    /// Main message body.
    /// </summary>
    [Required]
    public required string Message { get; set; }
}
