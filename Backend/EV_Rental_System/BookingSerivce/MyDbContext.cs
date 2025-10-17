using BookingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingService
{
    public class MyDbContext : DbContext
    {
        // DbSets chính
        public DbSet<Order> Orders { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<OnlineContract> OnlineContracts { get; set; }
        public DbSet<TrustScore> TrustScores { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        // Constructor dùng DI
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =========================
            // Order
            // =========================
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.OrderId);

                // Decimal columns
                entity.Property(o => o.HourlyRate).HasColumnType("decimal(18,2)");
                entity.Property(o => o.TotalCost).HasColumnType("decimal(18,2)");
                entity.Property(o => o.DepositAmount).HasColumnType("decimal(18,2)");

                // Enum as string
                entity.Property(o => o.Status)
                      .HasConversion<string>()
                      .HasMaxLength(50);

                // Indexes
                entity.HasIndex(o => o.UserId);
                entity.HasIndex(o => o.VehicleId);
                entity.HasIndex(o => o.Status);
                entity.HasIndex(o => o.CreatedAt);
                entity.HasIndex(o => new { o.FromDate, o.ToDate }); // Cho check availability
            });

            // =========================
            // Payment
            // =========================
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(p => p.PaymentId);

                // Relationship 1-1 với Order
                entity.HasOne(p => p.Order)
                      .WithOne(o => o.Payment)
                      .HasForeignKey<Payment>(p => p.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Decimal columns
                entity.Property(p => p.Amount).HasColumnType("decimal(18,2)");

                // String columns
                entity.Property(p => p.PaymentMethod).HasMaxLength(50);
                entity.Property(p => p.TransactionId).HasMaxLength(100);
                entity.Property(p => p.PaymentGatewayResponse).HasColumnType("nvarchar(max)");

                // Enum as string
                entity.Property(p => p.Status)
                      .HasConversion<string>()
                      .HasMaxLength(50);

                // Indexes
                entity.HasIndex(p => p.OrderId).IsUnique();
                entity.HasIndex(p => p.Status);
                entity.HasIndex(p => p.TransactionId);
            });

            // =========================
            // OnlineContract
            // =========================
            modelBuilder.Entity<OnlineContract>(entity =>
            {
                entity.HasKey(c => c.OnlineContractId);

                // Relationship 1-1 với Order
                entity.HasOne(c => c.Order)
                      .WithOne(o => o.OnlineContract)
                      .HasForeignKey<OnlineContract>(c => c.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                // String columns
                entity.Property(c => c.ContractNumber).HasMaxLength(50).IsRequired();
                entity.Property(c => c.ContractFilePath).HasMaxLength(500);
                entity.Property(c => c.Status).HasMaxLength(50);
                entity.Property(c => c.SignatureData).HasMaxLength(100);

                // Indexes
                entity.HasIndex(c => c.OrderId).IsUnique();
                entity.HasIndex(c => c.ContractNumber).IsUnique(); // Contract number phải unique
                entity.HasIndex(c => c.Status);
            });

            // =========================
            // TrustScore
            // =========================
            modelBuilder.Entity<TrustScore>(entity =>
            {
                entity.HasKey(t => t.TrustScoreId);

                entity.Property(t => t.Score).HasColumnType("int");

                entity.HasIndex(t => t.UserId);
                entity.HasIndex(t => t.CreatedAt);
            });

            // =========================
            // Notification
            // =========================
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(n => n.Id);

                entity.Property(n => n.Title).HasMaxLength(200);
                entity.Property(n => n.Description).HasMaxLength(1000);
                entity.Property(n => n.DataType).HasMaxLength(50);

                entity.HasIndex(n => n.UserId);
                entity.HasIndex(n => n.Created);
                entity.HasIndex(n => new { n.DataType, n.DataId }); // Cho query theo type
            });
        }
    }
}