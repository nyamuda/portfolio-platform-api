using Microsoft.EntityFrameworkCore;
using PortfolioPlatform.Api.Data;
using PortfolioPlatform.Api.Dtos.Dashboard;
using PortfolioPlatform.Api.Services.Abstractions.Dashboard;

namespace PortfolioPlatform.Api.Services.Implementations.Dashboard;

/// <summary>
/// Builds the small set of numbers and guidance cards shown on a user's dashboard.
/// </summary>
public class DashboardService(ApplicationDbContext context) : IDashboardService
{
    private readonly ApplicationDbContext _context = context;

    /// <inheritdoc/>
    public async Task<DashboardSummaryDto> GetSummaryAsync(int userId)
    {
        // The dashboard does not need full Profile, User, Project, or BlogPost entities.
        // A projection keeps the query narrow and avoids loading navigation graphs just to count records.
        DashboardProfileSnapshot? profile = await _context
            .Profiles
            .AsNoTracking()
            .Where(profile => profile.UserId == userId)
            .Select(profile => new DashboardProfileSnapshot
            {
                // These profile fields are used to decide whether the profile is complete enough to publish.
                Slug = profile.Slug,
                IsPublished = profile.IsPublished,
                DisplayName = profile.DisplayName,
                Headline = profile.Headline,
                Bio = profile.Bio,
                ProfileImageUrl = profile.ProfileImageUrl,
                CoverImageUrl = profile.CoverImageUrl,
                WebsiteUrl = profile.WebsiteUrl,
                LinkedInUrl = profile.LinkedInUrl,
                GitHubUrl = profile.GitHubUrl,

                // Let the database calculate the counts. That is cheaper than loading every project.
                TotalProjects = profile.Projects.Count,
                PublishedProjects = profile.Projects.Count(project => project.IsPublished),
                DraftProjects = profile.Projects.Count(project => !project.IsPublished),
                FeaturedProjects = profile.Projects.Count(project => project.IsFeatured),

                // Blog counts are useful dashboard signals, even when the first UI version only shows some of them.
                TotalBlogPosts = profile.BlogPosts.Count,
                PublishedBlogPosts = profile.BlogPosts.Count(post => post.IsPublished),
                DraftBlogPosts = profile.BlogPosts.Count(post => !post.IsPublished)
            })
            .FirstOrDefaultAsync();

        if (profile is null)
        {
            // A user can have an account before they have a public profile.
            // In that case the dashboard should give one clear next step instead of returning empty numbers.
            return new DashboardSummaryDto
            {
                HasProfile = false,
                NextStep = "Create your public profile."
            };
        }

        // Completion is deliberately calculated in one helper so future developers can tune the score easily.
        int completion = CalculateProfileCompletion(profile);

        // The DTO is intentionally compact. The frontend should not need to understand the scoring rules.
        return new DashboardSummaryDto
        {
            HasProfile = true,
            IsProfilePublished = profile.IsPublished,
            ProfileSlug = profile.Slug,
            ProfileCompletionPercent = completion,
            TotalProjects = profile.TotalProjects,
            PublishedProjects = profile.PublishedProjects,
            DraftProjects = profile.DraftProjects,
            FeaturedProjects = profile.FeaturedProjects,
            TotalBlogPosts = profile.TotalBlogPosts,
            PublishedBlogPosts = profile.PublishedBlogPosts,
            DraftBlogPosts = profile.DraftBlogPosts,
            NextStep = DetermineNextStep(profile.IsPublished, profile.TotalProjects, profile.PublishedProjects, completion)
        };
    }

    /// <summary>
    /// Calculates a simple profile-readiness percentage for dashboard guidance.
    /// </summary>
    /// <param name="profile">The projected profile data used to score completeness.</param>
    /// <returns>A whole-number completion percentage between 0 and 100.</returns>
    private static int CalculateProfileCompletion(DashboardProfileSnapshot profile)
    {
        // This is a product-readiness score, not a strict validation rule.
        // A profile can still save with missing optional fields; this score simply guides the owner.
        int completed = 0;

        // Keep this number close to the checks below. If a new check is added, increase the total too.
        int total = 9;

        // These fields help visitors immediately understand who the profile belongs to.
        if (!string.IsNullOrWhiteSpace(profile.DisplayName)) completed++;
        if (!string.IsNullOrWhiteSpace(profile.Headline)) completed++;
        if (!string.IsNullOrWhiteSpace(profile.Bio)) completed++;

        // Visual identity makes the public profile feel finished and trustworthy.
        if (!string.IsNullOrWhiteSpace(profile.ProfileImageUrl)) completed++;
        if (!string.IsNullOrWhiteSpace(profile.CoverImageUrl)) completed++;

        // External links help employers, clients, parents, or collaborators verify the person behind the profile.
        if (!string.IsNullOrWhiteSpace(profile.WebsiteUrl)) completed++;
        if (!string.IsNullOrWhiteSpace(profile.LinkedInUrl)) completed++;
        if (!string.IsNullOrWhiteSpace(profile.GitHubUrl)) completed++;

        // A profile without at least one published project still feels empty, even if the profile text is complete.
        if (profile.PublishedProjects > 0) completed++;

        // Round to the nearest whole percent so the UI gets a clean value like 78 instead of 77.7777.
        return (int)Math.Round((double)completed / total * 100);
    }

    /// <summary>
    /// Chooses the most useful next action to show on the user's dashboard.
    /// </summary>
    /// <param name="isProfilePublished">Whether the profile is currently visible to public visitors.</param>
    /// <param name="totalProjects">How many projects the profile owner has created.</param>
    /// <param name="publishedProjects">How many projects are currently public.</param>
    /// <param name="completion">The calculated profile-readiness percentage.</param>
    /// <returns>A short, user-facing next-step message.</returns>
    private static string DetermineNextStep(
        bool isProfilePublished,
        int totalProjects,
        int publishedProjects,
        int completion
    )
    {
        // Give the most basic setup problem first. There is no point asking the user to publish
        // if the profile still needs the details that make it useful to visitors.
        if (completion < 70)
            return "Add the key profile details first.";

        // Once the profile details are mostly ready, the next valuable step is adding work.
        if (totalProjects == 0)
            return "Add your first project.";

        // Draft projects are useful to the owner, but public visitors cannot see them.
        if (publishedProjects == 0)
            return "Publish at least one project.";

        // A complete profile with published work still needs to be made public.
        if (!isProfilePublished)
            return "Publish your profile when you are ready.";

        // At this point the profile is usable, so the guidance becomes maintenance-focused.
        return "Keep your best work up to date.";
    }

    /// <summary>
    /// Private read model used only for dashboard calculations.
    /// </summary>
    private sealed class DashboardProfileSnapshot
    {
        // Nullable strings are fine here because missing values are exactly what the completion score checks.
        public string? Slug { get; set; }
        public bool IsPublished { get; set; }
        public string? DisplayName { get; set; }
        public string? Headline { get; set; }
        public string? Bio { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? LinkedInUrl { get; set; }
        public string? GitHubUrl { get; set; }
        public int TotalProjects { get; set; }
        public int PublishedProjects { get; set; }
        public int DraftProjects { get; set; }
        public int FeaturedProjects { get; set; }
        public int TotalBlogPosts { get; set; }
        public int PublishedBlogPosts { get; set; }
        public int DraftBlogPosts { get; set; }
    }
}




