namespace PortfolioPlatform.Api.Dtos.ExperienceTypes;

/// <summary>
/// Experience type details returned by lookup and management endpoints.
/// </summary>
public class ExperienceTypeDto
{
    /// <summary>
    /// The unique database identifier for the experience type.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Display name shown in filters, forms, and timeline labels.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Optional short explanation of what this type represents.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Optional URL-friendly type slug.
    /// </summary>
    public string? Slug { get; set; }

    /// <summary>
    /// Optional display color used for badges, chips, or timeline accents.
    /// </summary>
    public string? ColorHex { get; set; }

    /// <summary>
    /// Optional icon name used by the frontend.
    /// </summary>
    public string? IconName { get; set; }

    /// <summary>
    /// Whether this type should be highlighted in suggestions or public filters.
    /// </summary>
    public bool IsFeatured { get; set; }

    /// <summary>
    /// Manual ordering value for forms and public filters.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Number of experiences currently linked to this type.
    /// </summary>
    public int TotalExperiences { get; set; }

    /// <summary>
    /// When the experience type was first created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the experience type was last updated, if it has been edited after creation.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
