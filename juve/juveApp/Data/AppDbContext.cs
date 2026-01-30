using Microsoft.EntityFrameworkCore;
using juveApp.Models;

namespace juveApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<CommunityRequest> CommunityRequests => Set<CommunityRequest>();
        public DbSet<Headline> Headlines => Set<Headline>();
        public DbSet<Vacation> Vacations => Set<Vacation>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Role table
            modelBuilder.Entity<Role>()
                .ToTable("roles")
                .HasKey(r => r.RoleId);

            modelBuilder.Entity<Role>()
                .Property(r => r.RoleId)
                .HasColumnName("role_id");

            modelBuilder.Entity<Role>()
                .Property(r => r.RoleName)
                .HasColumnName("role_name");

            // Configure User to Role relationship
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId);

            // Configure CommunityRequest
            modelBuilder.Entity<CommunityRequest>()
                .ToTable("community_requests")
                .HasKey(cr => cr.RequestId);

            modelBuilder.Entity<CommunityRequest>()
                .HasOne(cr => cr.Vacation)
                .WithMany(v => v.CommunityRequests)
                .HasForeignKey(cr => cr.VacationId);

            modelBuilder.Entity<CommunityRequest>()
                .HasOne(cr => cr.User)
                .WithMany()
                .HasForeignKey(cr => cr.UserId);

            // Configure Headline
            modelBuilder.Entity<Headline>()
                .ToTable("horizon_headlines")
                .HasKey(h => h.HeadlineId);

            modelBuilder.Entity<Headline>()
                .HasOne(h => h.Creator)
                .WithMany()
                .HasForeignKey(h => h.CreatedBy);

            // Configure Vacation
            modelBuilder.Entity<Vacation>()
                .ToTable("vacations")
                .HasKey(v => v.VacationId);

            modelBuilder.Entity<Vacation>()
                .HasOne(v => v.User)
                .WithMany()
                .HasForeignKey(v => v.UserId);
        }
    }
}

