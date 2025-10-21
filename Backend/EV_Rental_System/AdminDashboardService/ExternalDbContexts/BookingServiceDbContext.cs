using Microsoft.EntityFrameworkCore;
using AdminDashboardService.ExternalModels.BookingServiceModels;

namespace AdminDashboardService.ExternalDbContexts
{
    public class BookingServiceDbContext : DbContext
    {
        public BookingServiceDbContext(DbContextOptions<BookingServiceDbContext> options) : base(options) { }

        public DbSet<Order> Orders { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<OnlineContract> OnlineContracts { get; set; }
        public DbSet<VehicleInspectionReport> VehicleInspectionReports { get; set; }
        public DbSet<InspectionDetail> InspectionDetails { get; set; }
        public DbSet<InspectionImage> InspectionImages { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("dbo");

            // Cấu hình mối quan hệ
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Payment)
                .WithOne()
                .HasForeignKey<Payment>(p => p.OrderId);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.OnlineContract)
                .WithOne()
                .HasForeignKey<OnlineContract>(c => c.OrderId);
        }
    }
}