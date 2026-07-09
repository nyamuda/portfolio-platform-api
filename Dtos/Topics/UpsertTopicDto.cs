using System.ComponentModel.DataAnnotations;

namespace PortfolioPlatform.Api.Dtos.Topics;

/// <summary>
/// Request body used by administrators when creating or updating a topic.
/// </summary>
public class UpsertTopicDto
{
    /// <summary>
    /// Display name of the topic, such as Development, Teaching, Product, Career, or UX.
    /// </summary>
    [Required]
    [StringLength(120)]
    public required string Name { get; set; }

    /// <summary>
    /// Optional short explanation of what this topic represents.
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Optional URL-friendly version of the topic name.
    /// </summary>
    /// <remarks>
    /// If this is left blank, the service creates a slug from <see cref="Name" />.
    /// </remarks>
    [StringLength(140)]
    public string? Slug { get; set; }

    /// <summary>
    /// Optional display color used by the frontend for topic chips or cards.
    /// </summary>
    [StringLength(20)]
    public string? ColorHex { get; set; }

    /// <summary>
    /// Optional icon name used by the frontend when showing richer topic cards.
    /// </summary>
    [StringLength(80)]
    public string? IconName { get; set; }

    /// <summary>
    /// Indicates whether this topic should be shown in featured topic areas.
    /// </summary>
    public bool IsFeatured { get; set; }
}
