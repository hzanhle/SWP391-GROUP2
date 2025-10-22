using Microsoft.EntityFrameworkCore;
using AdminDashboardService.ExternalModels.UserServiceModels;

namespace AdminDashboardService.ExternalDbContexts
{
    public class UserServiceDbContext : DbContext
    {
        public UserServiceDbContext(DbContextOptions<UserServiceDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<CitizenInfo> CitizenInfos { get; set; }
        public DbSet<DriverLicense> DriverLicenses { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Image> Images { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("dbo");

            // Cấu hình các mối quan hệ nếu cần truy vấn có tham gia
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany()
                .HasForeignKey(u => u.RoleId);
        }
    }
}