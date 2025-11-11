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
        public DbSet<Settlement> Settlements { get; set; }
        public DbSet<TrustScore> TrustScores { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("dbo");

            // Configure Payment
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.Property(p => p.Amount)
                    .HasColumnType("decimal(18,2)");
            });

            // Configure Order
            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(o => o.TotalCost)
                    .HasColumnType("decimal(18,2)");

                entity.Property(o => o.DepositAmount)
                    .HasColumnType("decimal(18,2)");
            });

            // Configure InspectionDetail
            modelBuilder.Entity<InspectionDetail>(entity =>
            {
                entity.Property(d => d.CompensationAmount)
                    .HasColumnType("decimal(18,2)");
            });

            // Configure VehicleInspectionReport
            modelBuilder.Entity<VehicleInspectionReport>(entity =>
            {
                entity.Property(r => r.CompensationAmount)
                    .HasColumnType("decimal(18,2)");
            });

            // Configure relationships
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