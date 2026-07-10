using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace PortfolioPlatform.Api.Models.Content;

/// <summary>
/// Reusable label that can be attached to public portfolio content.
/// </summary>
/// <remarks>
/// Tags help visitors understand the skills, tools, subjects, themes, or work areas connected
/// to projects and writing. Projects and blog posts store real tag relationships; incoming DTOs
/// still accept simple tag-name lists so forms stay easy to use.
/// </remarks>
[Index(nameof(Name), IsUnique = true)]
[Index(nameof(Slug), IsUnique = true)]
public class Tag
{
    /// <summary>
    /// Internal primary key for the tag.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Display name of the tag, such as Vue, Teaching, UX, Chemistry, or ASP.NET Core.
    /// </summary>
    [StringLength(120)]
    public required string Name { get; set; }

    /// <summary>
    /// Optional short explanation of what this tag represents.
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Optional URL-friendly version of the tag name for public tag pages.
    /// </summary>
    [StringLength(140)]
    public string? Slug { get; set; }

    /// <summary>
    /// Optional display color used for tag chips/badges in the frontend.
    /// </summary>
    [StringLength(20)]
    public string? ColorHex { get; set; }

    /// <summary>
    /// Optional icon name used by the frontend when showing richer tag cards.
    /// </summary>
    [StringLength(80)]
    public string? IconName { get; set; }

    /// <summary>
    /// Indicates whether this tag should be shown in public tag suggestions or featured tag areas.
    /// </summary>
    public bool IsFeatured { get; set; }

    /// <summary>
    /// Projects that currently use this tag.
    /// </summary>
    public List<Project> Projects { get; set; } = [];

    /// <summary>
    /// Offerings that currently use this tag.
    /// </summary>
    public List<Offering> Offerings { get; set; } = [];

    /// <summary>
    /// Blog posts that currently use this tag.
    /// </summary>
    public List<BlogPost> BlogPosts { get; set; } = [];

    /// <summary>
    /// Date and time when the tag was first created in UTC.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the tag was last updated in UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

