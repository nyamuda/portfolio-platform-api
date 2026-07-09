using Microsoft.EntityFrameworkCore;
using PortfolioPlatform.Api.Models.Content;

namespace PortfolioPlatform.Api.Data.Seeders;

/// <summary>
/// Seeds the starter writing topics used to organise blog posts across the portfolio app.
/// </summary>
/// <remarks>
/// This class deliberately owns the seed data instead of placing long seed lists inside
/// <see cref="ApplicationDbContext"/>. The DbContext wires the seeder into EF Core's seeding
/// pipeline, while this class keeps the actual data easy to read, update, and test.
/// </remarks>
public static class CommonTopicSeeder
{
    /// <summary>
    /// Seeds common topics through EF Core's synchronous seeding pipeline.
    /// </summary>
    /// <param name="context">The EF Core context supplied by <c>UseSeeding</c>.</param>
    public static void Seed(DbContext context)
    {
        // EF Core tooling currently depends on the synchronous seeding path, so this method
        // must remain fully implemented even though the application usually uses async code.
        DbSet<Topic> topics = context.Set<Topic>();

        foreach (Topic topic in CreateCommonTopics())
        {
            // Slug is the stable lookup value. Name is also checked so older rows created
            // before slug support do not cause duplicate visible topics.
            bool alreadyExists = topics.Any(existingTopic =>
                existingTopic.Slug == topic.Slug || existingTopic.Name == topic.Name);

            if (alreadyExists)
            {
                continue;
            }

            topics.Add(topic);
        }

        context.SaveChanges();
    }

    /// <summary>
    /// Seeds common topics through EF Core's asynchronous seeding pipeline.
    /// </summary>
    /// <param name="context">The EF Core context supplied by <c>UseAsyncSeeding</c>.</param>
    /// <param name="cancellationToken">Cancellation token passed through by EF Core tooling/runtime.</param>
    public static async Task SeedAsync(DbContext context, CancellationToken cancellationToken)
    {
        // Keep the async version in step with the synchronous one. EF may call either path
        // depending on whether the database is being created, migrated, or updated by tooling.
        DbSet<Topic> topics = context.Set<Topic>();

        foreach (Topic topic in CreateCommonTopics())
        {
            bool alreadyExists = await topics.AnyAsync(
                existingTopic => existingTopic.Slug == topic.Slug || existingTopic.Name == topic.Name,
                cancellationToken
            );

            if (alreadyExists)
            {
                continue;
            }

            topics.Add(topic);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Builds the default topic list inserted into new databases.
    /// </summary>
    /// <returns>A fresh list of starter topics.</returns>
    private static List<Topic> CreateCommonTopics()
    {
        // Use one timestamp for the whole seed run so rows inserted together tell the same story.
        DateTime now = DateTime.UtcNow;

        return
        [
            new Topic
            {
                Name = "Development",
                Slug = "development",
                Description = "Posts about building software, tools, apps, and technical systems.",
                ColorHex = "#1D4ED8",
                IconName = "ph:code",
                IsFeatured = true,
                CreatedAt = now
            },
            new Topic
            {
                Name = "Product",
                Slug = "product",
                Description = "Posts about product thinking, features, planning, and user value.",
                ColorHex = "#7C3AED",
                IconName = "ph:rocket-launch",
                IsFeatured = true,
                CreatedAt = now
            },
            new Topic
            {
                Name = "Teaching",
                Slug = "teaching",
                Description = "Posts about tutoring, explaining ideas, learning design, and education.",
                ColorHex = "#0F766E",
                IconName = "ph:student",
                IsFeatured = true,
                CreatedAt = now
            },
            new Topic
            {
                Name = "Career",
                Slug = "career",
                Description = "Posts about professional growth, job hunting, interviews, and work habits.",
                ColorHex = "#B45309",
                IconName = "ph:briefcase",
                IsFeatured = true,
                CreatedAt = now
            },
            new Topic
            {
                Name = "UX",
                Slug = "ux",
                Description = "Posts about user experience, interface decisions, clarity, and design quality.",
                ColorHex = "#2563EB",
                IconName = "ph:cursor-click",
                IsFeatured = true,
                CreatedAt = now
            },
            new Topic
            {
                Name = "Projects",
                Slug = "projects",
                Description = "Project notes, build logs, case studies, and lessons from finished work.",
                ColorHex = "#4338CA",
                IconName = "ph:folder-open",
                IsFeatured = false,
                CreatedAt = now
            },
            new Topic
            {
                Name = "Learning",
                Slug = "learning",
                Description = "Posts about what the writer is studying, practising, or improving.",
                ColorHex = "#059669",
                IconName = "ph:book-open",
                IsFeatured = false,
                CreatedAt = now
            },
            new Topic
            {
                Name = "Process",
                Slug = "process",
                Description = "Posts about workflows, decisions, productivity, and how work gets done.",
                ColorHex = "#4B5563",
                IconName = "ph:flow-arrow",
                IsFeatured = false,
                CreatedAt = now
            },
            new Topic
            {
                Name = "Design",
                Slug = "design",
                Description = "Posts about visual design, interface polish, branding, and creative direction.",
                ColorHex = "#DB2777",
                IconName = "ph:paint-brush",
                IsFeatured = true,
                CreatedAt = now
            },
            new Topic
            {
                Name = "Writing",
                Slug = "writing",
                Description = "Essays, notes, reflections, and articles written for a wider audience.",
                ColorHex = "#7C2D12",
                IconName = "ph:pencil-line",
                IsFeatured = true,
                CreatedAt = now
            },
            new Topic
            {
                Name = "Business",
                Slug = "business",
                Description = "Posts about business ideas, operations, clients, markets, and practical strategy.",
                ColorHex = "#0F172A",
                IconName = "ph:building-office",
                IsFeatured = true,
                CreatedAt = now
            },
            new Topic
            {
                Name = "Research",
                Slug = "research",
                Description = "Posts about investigation, reading, experiments, evidence, and useful findings.",
                ColorHex = "#6D28D9",
                IconName = "ph:magnifying-glass",
                IsFeatured = false,
                CreatedAt = now
            },
            new Topic
            {
                Name = "AI",
                Slug = "ai",
                Description = "Posts about artificial intelligence, automation, prompts, tools, and practical use cases.",
                ColorHex = "#2563EB",
                IconName = "ph:sparkle",
                IsFeatured = true,
                CreatedAt = now
            },
            new Topic
            {
                Name = "Community",
                Slug = "community",
                Description = "Posts about groups, service, collaboration, events, and work that supports others.",
                ColorHex = "#16A34A",
                IconName = "ph:users-three",
                IsFeatured = false,
                CreatedAt = now
            },
            new Topic
            {
                Name = "Faith and Religion",
                Slug = "faith-and-religion",
                Description = "Posts about faith, values, religion, service, reflection, and spiritual growth.",
                ColorHex = "#92400E",
                IconName = "ph:hands-praying",
                IsFeatured = false,
                CreatedAt = now
            },
            new Topic
            {
                Name = "Personal Growth",
                Slug = "personal-growth",
                Description = "Posts about habits, mindset, lessons learned, goals, and becoming better over time.",
                ColorHex = "#0891B2",
                IconName = "ph:plant",
                IsFeatured = false,
                CreatedAt = now
            },
            new Topic
            {
                Name = "Freelancing",
                Slug = "freelancing",
                Description = "Posts about client work, proposals, pricing, delivery, and working independently.",
                ColorHex = "#9333EA",
                IconName = "ph:handshake",
                IsFeatured = false,
                CreatedAt = now
            },
            new Topic
            {
                Name = "Health and Wellness",
                Slug = "health-and-wellness",
                Description = "Posts about wellbeing, balance, routines, health, and sustainable work habits.",
                ColorHex = "#15803D",
                IconName = "ph:heartbeat",
                IsFeatured = false,
                CreatedAt = now
            },
            new Topic
            {
                Name = "Leadership",
                Slug = "leadership",
                Description = "Posts about leading teams, mentoring people, making decisions, and taking responsibility.",
                ColorHex = "#BE123C",
                IconName = "ph:flag-banner",
                IsFeatured = false,
                CreatedAt = now
            },
            new Topic
            {
                Name = "Tutorials",
                Slug = "tutorials",
                Description = "Step-by-step guides, walkthroughs, explanations, and practical how-to posts.",
                ColorHex = "#047857",
                IconName = "ph:list-checks",
                IsFeatured = true,
                CreatedAt = now
            },
            new Topic
            {
                Name = "Case Studies",
                Slug = "case-studies",
                Description = "Deeper write-ups that explain a problem, the approach taken, and what changed.",
                ColorHex = "#4338CA",
                IconName = "ph:article-medium",
                IsFeatured = true,
                CreatedAt = now
            }
        ];
    }
}