using Microsoft.EntityFrameworkCore;
using PortfolioPlatform.Api.Models.Content;

namespace PortfolioPlatform.Api.Data.Seeders;

/// <summary>
/// Seeds common tags that users can attach to projects and blog posts.
/// </summary>
/// <remarks>
/// Tags are different from topics. A topic is the main writing category for a blog post,
/// while tags describe tools, skills, technologies, industries, or themes connected to the work.
/// Keeping this list here gives new installations a useful starting vocabulary without forcing
/// users to create common tags such as Vue, ASP.NET Core, PostgreSQL, or Teaching by hand.
/// </remarks>
public static class CommonTagSeeder
{
    /// <summary>
    /// Seeds common tags through EF Core's synchronous seeding pipeline.
    /// </summary>
    /// <param name="context">The EF Core context supplied by <c>UseSeeding</c>.</param>
    public static void Seed(DbContext context)
    {
        // EF tooling currently relies on the synchronous seeding path, so this method is kept
        // fully functional instead of delegating only to the async version.
        DbSet<Tag> tags = context.Set<Tag>();

        foreach (Tag tag in CreateCommonTags())
        {
            // Slug is the best stable lookup value. Name is checked too so manually created
            // tags do not get duplicated just because they were saved before a slug existed.
            bool alreadyExists = tags.Any(existingTag =>
                existingTag.Slug == tag.Slug || existingTag.Name == tag.Name);

            if (alreadyExists)
            {
                continue;
            }

            tags.Add(tag);
        }

        context.SaveChanges();
    }

    /// <summary>
    /// Seeds common tags through EF Core's asynchronous seeding pipeline.
    /// </summary>
    /// <param name="context">The EF Core context supplied by <c>UseAsyncSeeding</c>.</param>
    /// <param name="cancellationToken">Cancellation token passed through by EF Core tooling/runtime.</param>
    public static async Task SeedAsync(DbContext context, CancellationToken cancellationToken)
    {
        // Keep this logic matched with the sync version so EnsureCreated, migrations, and
        // database update tooling all produce the same starter tags.
        DbSet<Tag> tags = context.Set<Tag>();

        foreach (Tag tag in CreateCommonTags())
        {
            bool alreadyExists = await tags.AnyAsync(
                existingTag => existingTag.Slug == tag.Slug || existingTag.Name == tag.Name,
                cancellationToken
            );

            if (alreadyExists)
            {
                continue;
            }

            tags.Add(tag);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Builds the starter tag list for portfolio projects and writing.
    /// </summary>
    /// <returns>A fresh list of tags ready to insert if missing.</returns>
    private static List<Tag> CreateCommonTags()
    {
        // One timestamp keeps the seed batch consistent and makes the inserted rows easier
        // to reason about when checking the database later.
        DateTime now = DateTime.UtcNow;

        return
        [
            // Programming languages
            CreateTag("JavaScript", "javascript", "Programming language commonly used for web applications.", "#F59E0B", "devicon:javascript", true, now),
            CreateTag("TypeScript", "typescript", "Typed JavaScript used for larger frontend and full-stack applications.", "#2563EB", "devicon:typescript", true, now),
            CreateTag("C#", "c-sharp", "Programming language used with .NET and ASP.NET Core.", "#7C3AED", "devicon:csharp", true, now),
            CreateTag("Python", "python", "Programming language used for scripting, automation, data, and backend work.", "#0F766E", "devicon:python", true, now),
            CreateTag("Java", "java", "Programming language used for backend, Android, and enterprise systems.", "#B45309", "devicon:java", false, now),
            CreateTag("PHP", "php", "Programming language commonly used for server-rendered websites and CMS work.", "#6366F1", "devicon:php", false, now),
            CreateTag("Go", "go", "Programming language often used for services, CLIs, and infrastructure tools.", "#0891B2", "devicon:go", false, now),
            CreateTag("Rust", "rust", "Systems programming language focused on performance and memory safety.", "#9A3412", "devicon:rust", false, now),
            CreateTag("SQL", "sql", "Language used to query and manage relational databases.", "#334155", "ph:database", true, now),

            // Frontend frameworks and UI
            CreateTag("Vue", "vue", "Frontend framework for building interactive user interfaces.", "#16A34A", "devicon:vuejs", true, now),
            CreateTag("React", "react", "Frontend library for building component-based user interfaces.", "#0284C7", "devicon:react", true, now),
            CreateTag("Angular", "angular", "Frontend framework for large web applications.", "#DC2626", "devicon:angularjs", false, now),
            CreateTag("Nuxt", "nuxt", "Vue framework for full-stack and server-rendered applications.", "#059669", "devicon:nuxtjs", false, now),
            CreateTag("Next.js", "next-js", "React framework for full-stack and server-rendered applications.", "#111827", "devicon:nextjs", false, now),
            CreateTag("Vite", "vite", "Fast frontend build tool used by modern Vue and React projects.", "#7C3AED", "devicon:vitejs", true, now),
            CreateTag("PrimeVue", "primevue", "Vue UI component library used to build polished interfaces quickly.", "#2563EB", "simple-icons:primevue", false, now),
            CreateTag("Bootstrap", "bootstrap", "CSS framework for responsive layouts and common interface patterns.", "#6D28D9", "devicon:bootstrap", false, now),
            CreateTag("Tailwind CSS", "tailwind-css", "Utility-first CSS framework for custom interface design.", "#0891B2", "devicon:tailwindcss", true, now),
            CreateTag("SCSS", "scss", "CSS preprocessor used for variables, nesting, and maintainable styling.", "#DB2777", "devicon:sass", false, now),
            CreateTag("Responsive Design", "responsive-design", "Design approach that keeps pages usable across desktop and mobile screens.", "#2563EB", "ph:device-mobile", true, now),
            CreateTag("Accessibility", "accessibility", "Practices that make digital products easier for more people to use.", "#0F766E", "ph:person-arms-spread", false, now),

            // Backend and APIs
            CreateTag("ASP.NET Core", "asp-net-core", "Microsoft web framework for building APIs and backend applications.", "#512BD4", "devicon:dotnetcore", true, now),
            CreateTag(".NET", "dotnet", "Microsoft development platform for building web, desktop, and cloud applications.", "#512BD4", "devicon:dot-net", true, now),
            CreateTag("Entity Framework Core", "entity-framework-core", "ORM used to work with databases from .NET applications.", "#4338CA", "ph:database", true, now),
            CreateTag("REST API", "rest-api", "API style commonly used by web and mobile applications.", "#2563EB", "ph:cloud", true, now),
            CreateTag("JWT", "jwt", "Token format often used for stateless authentication.", "#111827", "ph:key", false, now),
            CreateTag("OAuth", "oauth", "Authorization flow used for sign-in providers such as Google.", "#EA580C", "ph:lock-key", false, now),
            CreateTag("Authentication", "authentication", "Account sign-in, identity, password reset, and access control work.", "#BE123C", "ph:shield-check", true, now),
            CreateTag("Authorization", "authorization", "Permission and role checks that control what users can access.", "#B91C1C", "ph:shield", false, now),
            CreateTag("Background Jobs", "background-jobs", "Work handled outside the main request flow, such as emails or scheduled tasks.", "#475569", "ph:clock-countdown", false, now),

            // Databases and storage
            CreateTag("PostgreSQL", "postgresql", "Relational database used for reliable application data storage.", "#2563EB", "devicon:postgresql", true, now),
            CreateTag("SQL Server", "sql-server", "Microsoft relational database used in many business applications.", "#B91C1C", "devicon:microsoftsqlserver", false, now),
            CreateTag("MySQL", "mysql", "Popular relational database used for web applications.", "#0369A1", "devicon:mysql", false, now),
            CreateTag("SQLite", "sqlite", "Lightweight file-based relational database.", "#475569", "devicon:sqlite", false, now),
            CreateTag("MongoDB", "mongodb", "Document database used for flexible JSON-like data models.", "#16A34A", "devicon:mongodb", false, now),
            CreateTag("Supabase", "supabase", "Backend platform commonly used for Postgres, auth, storage, and realtime features.", "#16A34A", "simple-icons:supabase", true, now),
            CreateTag("Cloud Storage", "cloud-storage", "Remote file storage for images, documents, and other user uploads.", "#0284C7", "ph:cloud-arrow-up", false, now),

            // Cloud, DevOps, and tooling
            CreateTag("Docker", "docker", "Container platform used to package and run applications consistently.", "#0284C7", "devicon:docker", true, now),
            CreateTag("Git", "git", "Version control system used to track and collaborate on code.", "#EA580C", "devicon:git", true, now),
            CreateTag("GitHub", "github", "Platform for hosting repositories, issues, pull requests, and developer portfolios.", "#111827", "devicon:github", true, now),
            CreateTag("CI/CD", "ci-cd", "Automated build, test, and deployment workflows.", "#2563EB", "ph:arrows-clockwise", false, now),
            CreateTag("Azure", "azure", "Microsoft cloud platform for hosting apps, databases, and services.", "#2563EB", "devicon:azure", false, now),
            CreateTag("AWS", "aws", "Amazon cloud platform for hosting apps, storage, databases, and infrastructure.", "#B45309", "devicon:amazonwebservices", false, now),
            CreateTag("Vercel", "vercel", "Hosting platform often used for frontend and full-stack JavaScript apps.", "#111827", "devicon:vercel", false, now),
            CreateTag("Netlify", "netlify", "Hosting platform for frontend sites, functions, and deploy previews.", "#0F766E", "devicon:netlify", false, now),
            CreateTag("Testing", "testing", "Work related to automated tests, manual checks, and software quality.", "#7C3AED", "ph:check-circle", true, now),
            CreateTag("Playwright", "playwright", "End-to-end testing tool for web applications.", "#16A34A", "devicon:playwright", false, now),

            // Product, design, and writing
            CreateTag("UX", "ux", "User experience work focused on clarity, usability, and flow.", "#2563EB", "ph:cursor-click", true, now),
            CreateTag("UI Design", "ui-design", "Visual and interaction design for screens, components, and layouts.", "#DB2777", "ph:layout", true, now),
            CreateTag("Product Design", "product-design", "Design work that connects user needs, business goals, and product decisions.", "#7C3AED", "ph:rocket-launch", true, now),
            CreateTag("Product Management", "product-management", "Planning and prioritising product work around user value.", "#9333EA", "ph:kanban", false, now),
            CreateTag("Content Writing", "content-writing", "Writing clear articles, posts, documentation, and public-facing content.", "#B45309", "ph:pencil-line", true, now),
            CreateTag("Technical Writing", "technical-writing", "Writing docs, guides, explanations, and technical learning material.", "#475569", "ph:article", true, now),
            CreateTag("Documentation", "documentation", "Developer or user-facing docs that explain how something works.", "#334155", "ph:book-open-text", true, now),
            CreateTag("Case Study", "case-study", "Structured write-up showing the problem, approach, work, and outcome.", "#4338CA", "ph:article-medium", true, now),

            // Education and professional work
            CreateTag("Teaching", "teaching", "Work related to explaining ideas, tutoring, lessons, and student support.", "#0F766E", "ph:student", true, now),
            CreateTag("Tutoring", "tutoring", "One-to-one or small-group academic support and coaching.", "#047857", "ph:chalkboard-teacher", true, now),
            CreateTag("Education", "education", "Projects or writing connected to learning, schools, courses, and study support.", "#2563EB", "ph:graduation-cap", true, now),
            CreateTag("Curriculum", "curriculum", "Work aligned to syllabuses, learning goals, or structured study paths.", "#4338CA", "ph:books", false, now),
            CreateTag("Mentorship", "mentorship", "Guidance, coaching, support, and helping others grow.", "#16A34A", "ph:hand-heart", false, now),
            CreateTag("Freelancing", "freelancing", "Client work, independent projects, pricing, delivery, and proposals.", "#9333EA", "ph:handshake", true, now),
            CreateTag("Business", "business", "Work connected to operations, clients, markets, and practical strategy.", "#0F172A", "ph:building-office", false, now),
            CreateTag("Entrepreneurship", "entrepreneurship", "Starting, shaping, and growing new products, services, or ventures.", "#EA580C", "ph:lightbulb", false, now),
            CreateTag("Leadership", "leadership", "Leading teams, mentoring people, making decisions, and taking responsibility.", "#BE123C", "ph:flag-banner", false, now),

            // Broader personal and community themes
            CreateTag("Personal Growth", "personal-growth", "Habits, mindset, reflection, goals, and lessons learned over time.", "#0891B2", "ph:plant", false, now),
            CreateTag("Health and Wellness", "health-and-wellness", "Wellbeing, balance, routines, health, and sustainable work habits.", "#15803D", "ph:heartbeat", false, now),
            CreateTag("Community", "community", "Groups, service, collaboration, events, and work that supports others.", "#16A34A", "ph:users-three", false, now),
            CreateTag("Faith and Religion", "faith-and-religion", "Faith, values, religion, service, reflection, and spiritual growth.", "#92400E", "ph:hands-praying", false, now),
            CreateTag("Volunteering", "volunteering", "Service, community work, unpaid support, and giving time to useful causes.", "#0F766E", "ph:heart", false, now),
            CreateTag("Finance", "finance", "Money, budgeting, financial planning, and practical finance topics.", "#047857", "ph:money", false, now),
            CreateTag("Research", "research", "Investigation, reading, experiments, evidence, and useful findings.", "#6D28D9", "ph:magnifying-glass", false, now),
            CreateTag("AI", "ai", "Artificial intelligence, automation, prompts, tools, and practical use cases.", "#2563EB", "ph:sparkle", true, now)
        ];
    }

    /// <summary>
    /// Creates one tag with the shared defaults used by the seed list.
    /// </summary>
    /// <param name="name">Human-readable tag name.</param>
    /// <param name="slug">URL-friendly tag identifier.</param>
    /// <param name="description">Short explanation shown in management screens or future public tag pages.</param>
    /// <param name="colorHex">Display color used by frontend tag chips and cards.</param>
    /// <param name="iconName">Icon name understood by the frontend icon system.</param>
    /// <param name="isFeatured">Whether the tag should appear in featured/suggested tag areas.</param>
    /// <param name="createdAt">Shared creation timestamp for this seed run.</param>
    /// <returns>A new tag entity ready to be inserted if missing.</returns>
    private static Tag CreateTag(
        string name,
        string slug,
        string description,
        string colorHex,
        string iconName,
        bool isFeatured,
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
            CreatedAt = createdAt
        };
}