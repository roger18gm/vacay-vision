using Microsoft.EntityFrameworkCore;
using juveApp.Models;

namespace juveApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();

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
        }
    }
}

