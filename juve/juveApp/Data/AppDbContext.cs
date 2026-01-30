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
        public DbSet<VacationComment> VacationComments => Set<VacationComment>();
        public DbSet<VacationWidget> VacationWidgets => Set<VacationWidget>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure UTC DateTime conversion for all DateTime properties
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(
                            new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                                v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc),
                                v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
                            )
                        );
                    }
                }
            }

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

            // Configure VacationComment
            modelBuilder.Entity<VacationComment>()
                .ToTable("vacation_comments")
                .HasKey(vc => vc.CommentId);

            modelBuilder.Entity<VacationComment>()
                .HasOne(vc => vc.Vacation)
                .WithMany(v => v.Comments)
                .HasForeignKey(vc => vc.VacationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VacationComment>()
                .HasOne(vc => vc.User)
                .WithMany()
                .HasForeignKey(vc => vc.UserId);

            // Configure VacationWidget
            modelBuilder.Entity<VacationWidget>()
                .ToTable("vacation_widgets")
                .HasKey(vw => vw.VacationWidgetId);

            modelBuilder.Entity<VacationWidget>()
                .HasOne(vw => vw.Vacation)
                .WithMany(v => v.Widgets)
                .HasForeignKey(vw => vw.VacationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Create indexes
            modelBuilder.Entity<VacationComment>()
                .HasIndex(vc => vc.VacationId);

            modelBuilder.Entity<VacationComment>()
                .HasIndex(vc => vc.UserId);

            modelBuilder.Entity<VacationWidget>()
                .HasIndex(vw => vw.VacationId);
        }
    }
}

