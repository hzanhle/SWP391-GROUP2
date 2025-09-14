using Microsoft.EntityFrameworkCore;
using UserService.Models;

public class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<CitizenInfo> CitizenInfos { get; set; }
    public DbSet<DriverLicense> DriverLicenses { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Bật log SQL để debug (chỉ dev environment)
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Quan hệ 1-1: User ↔ CitizenInfo
        modelBuilder.Entity<User>()
            .HasOne(u => u.CitizenInfo)
            .WithOne()
            .HasForeignKey<CitizenInfo>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Quan hệ 1-1: User ↔ DriverLicense
        modelBuilder.Entity<User>()
            .HasOne(u => u.DriverLicense)
            .WithOne()
            .HasForeignKey<DriverLicense>(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seed Roles
        modelBuilder.Entity<Role>().HasData(
            new Role(1, "Member"),
            new Role(2, "Employee"),
            new Role(3, "Admin")
        );
    }
}
