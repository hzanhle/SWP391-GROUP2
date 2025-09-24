using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using UserService.Models;
using UserService.Models.UserService.Models;

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

            // ==================== 1-1 relations ====================
            modelBuilder.Entity<User>()
                .HasOne(u => u.CitizenInfo)
                .WithOne()
                .HasForeignKey<CitizenInfo>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasOne(u => u.DriverLicense)
                .WithOne()
                .HasForeignKey<DriverLicense>(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ==================== Seed Roles ====================
            modelBuilder.Entity<Role>().HasData(
                new Role(1, "Member"),
                new Role(2, "Employee"),
                new Role(3, "Admin")
            );

            // ==================== Seed Admin User ====================
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    UserName = "admin",
                    Email = "admin@example.com",
                    PhoneNumber = "0123456789",
                    Password = "Admin@123",
                    RoleId = 3
                }
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
