using Microsoft.EntityFrameworkCore;
using PortfolioPlatform.Api.Data.Seeders;
using PortfolioPlatform.Api.Models.Content;
using PortfolioPlatform.Api.Models.Profiles;
using PortfolioPlatform.Api.Models.Users;

namespace PortfolioPlatform.Api.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<UserOtp> UserOtps { get; set; }
    public DbSet<Profile> Profiles { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<BlogPost> BlogPosts { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Topic> Topics { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder
            .UseSeeding(
                (context, _) =>
                {
                    // Keep seed registration in the DbContext so EF tooling, migrations, and
                    // EnsureCreated all run the same initialization logic.
                    CommonTopicSeeder.Seed(context);
                }
            )
            .UseAsyncSeeding(
                async (context, _, cancellationToken) =>
                {
                    // The async path mirrors the sync path. EF tooling relies on the sync version,
                    // but runtime code may call the async version during database initialization.
                    await CommonTopicSeeder.SeedAsync(context, cancellationToken);
                }
            );
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // A User can request many one-time passwords over time. OTP records remain
        // account-owned security data, so deleting the account should also delete its codes.
        modelBuilder
            .Entity<UserOtp>()
            .HasOne(userOtp => userOtp.User)
            .WithMany(user => user.UserOtps)
            .HasForeignKey(userOtp => userOtp.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // A User can own one public Profile while a Profile can only belong to one User.
        // Hence, there is a one-to-one relationship between User and Profile.
        //
        // Cascade is intentional here: a profile is account-owned data. If the account is
        // deleted, the public profile should also be deleted because it cannot stand on its own.
        //
        // Simple rules such as unique indexes and string lengths are declared on the model
        // classes themselves. The DbContext is reserved for relationship shape and delete rules.
        modelBuilder
            .Entity<Profile>()
            .HasOne(profile => profile.User)
            .WithOne(user => user.Profile)
            .HasForeignKey<Profile>(profile => profile.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // A Profile can have multiple Projects while a Project can only belong to one Profile.
        // Hence, there is a one-to-many relationship between Profile and Project.
        //
        // Cascade is intentional here: projects are part of the public portfolio profile.
        // Deleting the profile should remove its project records so orphaned projects do not
        // remain visible or queryable without an owner.
        modelBuilder
            .Entity<Project>()
            .HasOne(project => project.Profile)
            .WithMany(profile => profile.Projects)
            .HasForeignKey(project => project.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // A Profile can have many BlogPosts while a BlogPost can only belong to one Profile.
        // Blog posts follow the same ownership rule as projects: they are public-profile content,
        // not standalone records. If the profile is deleted, its drafts and published posts should
        // be removed too so the database does not keep orphaned content.
        modelBuilder
            .Entity<BlogPost>()
            .HasOne(post => post.Profile)
            .WithMany(profile => profile.BlogPosts)
            .HasForeignKey(post => post.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);
        // A Project can use multiple Tags, and a Tag can be reused by multiple Projects.
        // Hence, there is a many-to-many relationship between Project and Tag.
        //
        // The join table is named explicitly so migrations produce a predictable schema.
        // Deleting a project only removes its join rows; the shared Tag row remains available
        // because another project or blog post may still use the same tag.
        modelBuilder
            .Entity<Project>()
            .HasMany(project => project.Tags)
            .WithMany(tag => tag.Projects)
            .UsingEntity(join => join.ToTable("ProjectTags"));

        // A BlogPost can optionally belong to one managed Topic while a Topic can group many BlogPosts.
        // Topic is deliberately optional because and writers should be able to draft a post before choosing a final topic.
        modelBuilder
            .Entity<BlogPost>()
            .HasOne(post => post.Topic)
            .WithMany(topic => topic.BlogPosts)
            .HasForeignKey(post => post.TopicId)
            .OnDelete(DeleteBehavior.SetNull);

        // A BlogPost can use multiple Tags, and a Tag can be reused by multiple BlogPosts.
        // Hence, there is a many-to-many relationship between BlogPost and Tag.
        //
        // This mirrors the project tag mapping above. Tags are shared vocabulary, not owned
        // children of a single post, so removing a post should not delete the tag itself.
        modelBuilder
            .Entity<BlogPost>()
            .HasMany(post => post.Tags)
            .WithMany(tag => tag.BlogPosts)
            .UsingEntity(join => join.ToTable("BlogPostTags"));
    }
}
