using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Models;

namespace UserService
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }

        // ==================== DbSet ====================
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<CitizenInfo> CitizenInfos { get; set; }
        public DbSet<DriverLicense> DriverLicenses { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==================== 1-N User-CitizenInfo ====================
            modelBuilder.Entity<User>()
                .HasMany(u => u.CitizenInfos)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ==================== 1-N User-DriverLicense ====================
            modelBuilder.Entity<User>()
                .HasMany(u => u.DriverLicenses)
                .WithOne(d => d.User)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ==================== 1-N User-Notification ====================
            modelBuilder.Entity<User>()
                .HasMany(u => u.Notifications)
                .WithOne(n => n.User)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ==================== Seed Roles ====================
            modelBuilder.Entity<Role>().HasData(
                new Role(1, "Member"),
                new Role(2, "Employee"),
                new Role(3, "Admin")
            );

            // ==================== Configure Image ====================
            modelBuilder.Entity<Image>(entity =>
            {
                entity.HasKey(i => i.ImageId);
                entity.Property(i => i.Url)
                      .IsRequired()
                      .HasMaxLength(500);
                entity.Property(i => i.Type)
                      .IsRequired()
                      .HasMaxLength(50);
                entity.Property(i => i.TypeId)
                      .IsRequired();
            });
        }
    }
}
