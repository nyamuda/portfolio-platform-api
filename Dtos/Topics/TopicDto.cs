namespace PortfolioPlatform.Api.Dtos.Topics;

/// <summary>
/// Topic information returned to the frontend.
/// </summary>
public class TopicDto
{
    /// <summary>
    /// Internal primary key for the topic.
    /// </summary>
    public required int Id { get; set; }

    /// <summary>
    /// Display name of the topic.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Optional short explanation of what this topic represents.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// URL-friendly version of the topic name.
    /// </summary>
    public string? Slug { get; set; }

    /// <summary>
    /// Optional display color used by the frontend.
    /// </summary>
    public string? ColorHex { get; set; }

    /// <summary>
    /// Optional icon name used by the frontend.
    /// </summary>
    public string? IconName { get; set; }

    /// <summary>
    /// Indicates whether this topic should be shown in featured topic areas.
    /// </summary>
    public bool IsFeatured { get; set; }

    /// <summary>
    /// Date and time when the topic was first created in UTC.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date and time when the topic was last updated in UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Number of blog posts currently assigned to this topic.
    /// </summary>
    public int TotalBlogPosts { get; set; }
}
