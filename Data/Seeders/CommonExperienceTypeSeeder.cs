using Microsoft.EntityFrameworkCore;
using PortfolioPlatform.Api.Models.Content;

namespace PortfolioPlatform.Api.Data.Seeders;

/// <summary>
/// Seeds starter experience types used to classify public timeline entries.
/// </summary>
/// <remarks>
/// Experience types are the primary categories for timeline entries. Keeping the seed list here
/// gives new installations sensible defaults such as Work, Education, Volunteering, Teaching,
/// Certification, Award, Speaking, and Milestone without making the DbContext hard to read.
/// </remarks>
public static class CommonExperienceTypeSeeder
{
    /// <summary>
    /// Seeds common experience types through EF Core's synchronous seeding pipeline.
    /// </summary>
    /// <param name="context">The EF Core context supplied by <c>UseSeeding</c>.</param>
    public static void Seed(DbContext context)
    {
        // EF tooling relies on the synchronous seeding path, so this method must do real work.
        DbSet<ExperienceType> types = context.Set<ExperienceType>();

        foreach (ExperienceType type in CreateCommonTypes())
        {
            // Slug is stable for seeded rows, while Name protects older manually created rows.
            bool alreadyExists = types.Any(
                existingType => existingType.Slug == type.Slug || existingType.Name == type.Name
            );

            if (alreadyExists)
            {
                continue;
            }

            types.Add(type);
        }

        context.SaveChanges();
    }

    /// <summary>
    /// Seeds common experience types through EF Core's asynchronous seeding pipeline.
    /// </summary>
    /// <param name="context">The EF Core context supplied by <c>UseAsyncSeeding</c>.</param>
    /// <param name="cancellationToken">Cancellation token passed through by EF Core tooling/runtime.</param>
    public static async Task SeedAsync(DbContext context, CancellationToken cancellationToken)
    {
        // Keep this path matched with Seed so migrations, database update, and runtime setup behave the same way.
        DbSet<ExperienceType> types = context.Set<ExperienceType>();

        foreach (ExperienceType type in CreateCommonTypes())
        {
            bool alreadyExists = await types.AnyAsync(
                existingType => existingType.Slug == type.Slug || existingType.Name == type.Name,
                cancellationToken
            );

            if (alreadyExists)
            {
                continue;
            }

            types.Add(type);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Builds the default type list inserted into new databases.
    /// </summary>
    /// <returns>A fresh list of starter experience types.</returns>
    private static List<ExperienceType> CreateCommonTypes()
    {
        // One timestamp keeps seeded rows from the same run easy to identify later.
        DateTime now = DateTime.UtcNow;

        return
        [
            CreateType(
                "Work",
                "work",
                "Jobs, contracts, freelance roles, internships, and professional work.",
                "#1D4ED8",
                "ph:briefcase",
                true,
                1,
                now
            ),
            CreateType(
                "Education",
                "education",
                "Schools, degrees, courses, qualifications, and formal study.",
                "#4338CA",
                "ph:graduation-cap",
                true,
                2,
                now
            ),
            CreateType(
                "Volunteering",
                "volunteering",
                "Community service, unpaid support, non-profit work, and giving time to useful causes.",
                "#0F766E",
                "ph:heart",
                true,
                3,
                now
            ),
            CreateType(
                "Teaching",
                "teaching",
                "Tutoring, mentoring, lessons, workshops, and learning support.",
                "#047857",
                "ph:chalkboard-teacher",
                true,
                4,
                now
            ),
            CreateType(
                "Certification",
                "certification",
                "Certificates, licenses, short credentials, and verified training.",
                "#7C3AED",
                "ph:certificate",
                true,
                5,
                now
            ),
            CreateType(
                "Award",
                "award",
                "Awards, honours, recognitions, scholarships, and notable achievements.",
                "#B45309",
                "ph:trophy",
                false,
                6,
                now
            ),
            CreateType(
                "Speaking",
                "speaking",
                "Talks, presentations, panels, webinars, and public teaching moments.",
                "#BE123C",
                "ph:microphone-stage",
                false,
                7,
                now
            ),
            CreateType(
                "Milestone",
                "milestone",
                "Important career, learning, product, or personal development milestones.",
                "#334155",
                "ph:flag-banner",
                false,
                8,
                now
            )
        ];
    }

    /// <summary>
    /// Creates one experience type with the shared defaults used by the seed list.
    /// </summary>
    /// <param name="name">Human-readable type name.</param>
    /// <param name="slug">URL-friendly type identifier.</param>
    /// <param name="description">Short explanation shown in management screens or future public filters.</param>
    /// <param name="colorHex">Display color used by frontend type chips and cards.</param>
    /// <param name="iconName">Icon name understood by the frontend icon system.</param>
    /// <param name="isFeatured">Whether the type should appear in featured/suggested areas.</param>
    /// <param name="sortOrder">Default display order for forms and filters.</param>
    /// <param name="createdAt">Shared creation timestamp for this seed run.</param>
    /// <returns>A new experience type entity ready to be inserted if missing.</returns>
    private static ExperienceType CreateType(
        string name,
        string slug,
        string description,
        string colorHex,
        string iconName,
        bool isFeatured,
        int sortOrder,
        DateTime createdAt
    ) =>
        new()
        {
            Name = name,
            Slug = slug,
            Description = description,
            ColorHex = colorHex,
            IconName = iconName,
            IsFeatured = isFeatured,
            SortOrder = sortOrder,
            CreatedAt = createdAt
        };
}
