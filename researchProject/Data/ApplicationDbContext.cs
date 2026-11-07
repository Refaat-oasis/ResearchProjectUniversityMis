using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using researchProject.Models.Entities;

namespace researchProject.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<College> Colleges => Set<College>();
        public DbSet<CollegeCoAdmin> CollegeCoAdmins => Set<CollegeCoAdmin>();
        public DbSet<Researcher> Researchers => Set<Researcher>();
        public DbSet<ResearchCategory> ResearchCategories => Set<ResearchCategory>();
        public DbSet<Research> Researches => Set<Research>();
        public DbSet<ResearcherResearchesBridge> ResearcherResearchesBridge => Set<ResearcherResearchesBridge>();
        public DbSet<ResearchAttachment> ResearchAttachments => Set<ResearchAttachment>();
        public DbSet<Judge> Judges => Set<Judge>();
        public DbSet<Evaluation> Evaluations => Set<Evaluation>();
        public DbSet<JudgeInvitation> JudgeInvitations => Set<JudgeInvitation>();
        public DbSet<ResearchStatusHistory> ResearchStatusHistories => Set<ResearchStatusHistory>();
        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Index on Notifications for fast lookup by user and date
            builder.Entity<Notification>()
                .HasIndex(n => new { n.UserId, n.CreatedAt });

            // 1. Profile table 1:1 unique indexes on UserId
            builder.Entity<CollegeCoAdmin>()
                .HasIndex(c => c.UserId)
                .IsUnique();

            builder.Entity<Researcher>()
                .HasIndex(r => r.UserId)
                .IsUnique();

            builder.Entity<Judge>()
                .HasIndex(j => j.UserId)
                .IsUnique();

            // 2. Evaluation unique index on (ResearchId, JudgeId, RoundNumber)
            builder.Entity<Evaluation>()
                .HasIndex(e => new { e.ResearchId, e.JudgeId, e.RoundNumber })
                .IsUnique();

            // 3. Unique index on JudgeInvitation TokenHash
            builder.Entity<JudgeInvitation>()
                .HasIndex(i => i.TokenHash)
                .IsUnique();

            // 4. College relationships
            builder.Entity<College>()
                .HasMany(c => c.CollegeCoAdmins)
                .WithOne(a => a.College)
                .HasForeignKey(a => a.CollegeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<College>()
                .HasMany(c => c.Researchers)
                .WithOne(r => r.College)
                .HasForeignKey(r => r.CollegeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<College>()
                .HasMany(c => c.ResearchCategories)
                .WithOne(cat => cat.College)
                .HasForeignKey(cat => cat.CollegeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<College>()
                .HasMany(c => c.Researches)
                .WithOne(res => res.College)
                .HasForeignKey(res => res.CollegeId)
                .OnDelete(DeleteBehavior.Restrict);

            // 5. ResearchCategory relationship
            builder.Entity<ResearchCategory>()
                .HasMany(cat => cat.Researches)
                .WithOne(res => res.Category)
                .HasForeignKey(res => res.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // 6. Research relationships
            builder.Entity<Research>()
                .HasMany(res => res.ResearcherResearchesBridges)
                .WithOne(b => b.Research)
                .HasForeignKey(b => b.ResearchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Research>()
                .HasMany(res => res.ResearchAttachments)
                .WithOne(att => att.Research)
                .HasForeignKey(att => att.ResearchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Research>()
                .HasMany(res => res.Evaluations)
                .WithOne(ev => ev.Research)
                .HasForeignKey(ev => ev.ResearchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Research>()
                .HasMany(res => res.ResearchStatusHistories)
                .WithOne(h => h.Research)
                .HasForeignKey(h => h.ResearchId)
                .OnDelete(DeleteBehavior.Restrict);

            // 7. ResearcherResearchesBridge relationship
            builder.Entity<ResearcherResearchesBridge>()
                .HasOne(b => b.Researcher)
                .WithMany(r => r.ResearcherResearchesBridges)
                .HasForeignKey(b => b.ResearcherId)
                .OnDelete(DeleteBehavior.Restrict);

            // 8. Evaluation relationships
            builder.Entity<Evaluation>()
                .HasOne(ev => ev.Judge)
                .WithMany(j => j.Evaluations)
                .HasForeignKey(ev => ev.JudgeId)
                .OnDelete(DeleteBehavior.Restrict);

            // 9. Foreign keys for ApplicationUser references
            builder.Entity<CollegeCoAdmin>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Researcher>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Judge>()
                .HasOne(j => j.User)
                .WithMany()
                .HasForeignKey(j => j.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<JudgeInvitation>()
                .HasOne(i => i.InvitedByUser)
                .WithMany()
                .HasForeignKey(i => i.InvitedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ResearchStatusHistory>()
                .HasOne(h => h.ChangedByUser)
                .WithMany()
                .HasForeignKey(h => h.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
