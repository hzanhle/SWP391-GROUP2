using BookingService.Models;
using BookingSerivce.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingSerivce
{
    public class MyDbContext : DbContext
    {
        // DbSets - đại diện cho các bảng trong database
        public DbSet<Order> Orders { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<OnlineContract> OnlineContracts { get; set; }
        public DbSet<VehicleInspectionReport> VehicleInspectionReports { get; set; }
        public DbSet<InspectionDetail> InspectionDetails { get; set; }
        public DbSet<InspectionImage> InspectionImages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<SoftLock> SoftLocks { get; set; } // Stage 1 Enhancement

        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================================
            // ORDER CONFIGURATION
            // ============================================
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.OrderId);

                // Sử dụng decimal(18,2) cho các trường tiền tệ
                entity.Property(o => o.TotalCost)
                    .HasColumnType("decimal(18,2)");

                entity.Property(o => o.DepositAmount)
                    .HasColumnType("decimal(18,2)");

                // Giới hạn độ dài cho Status
                entity.Property(o => o.Status)
                    .HasMaxLength(50);

                // Index cho performance
                entity.HasIndex(o => o.UserId);
                entity.HasIndex(o => o.VehicleId);
                entity.HasIndex(o => o.Status);
                entity.HasIndex(o => o.CreatedAt);
            });

            // ============================================
            // PAYMENT CONFIGURATION
            // ============================================
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(p => p.PaymentId);

                // One-to-One relationship: Order - Payment
                entity.HasOne(p => p.Order)
                    .WithOne(o => o.Payment)
                    .HasForeignKey<Payment>(p => p.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(p => p.DepositedAmount)
                    .HasColumnType("decimal(18,2)");

                entity.Property(p => p.PaidAmount)
                    .HasColumnType("decimal(18,2)");

                entity.Property(p => p.PaymentMethod)
                    .HasMaxLength(50);

                entity.Property(p => p.Status)
                    .HasMaxLength(50);

                entity.Property(p => p.TransactionCode)
                    .HasMaxLength(100);

                entity.Property(p => p.DepositTransactionCode)
                    .HasMaxLength(100);

                // Index
                entity.HasIndex(p => p.OrderId).IsUnique();
                entity.HasIndex(p => p.TransactionCode);
                entity.HasIndex(p => p.Status);
            });

            // ============================================
            // ONLINE CONTRACT CONFIGURATION
            // ============================================
            modelBuilder.Entity<OnlineContract>(entity =>
            {
                entity.HasKey(c => c.ContractId);

                // One-to-One relationship: Order - OnlineContract
                entity.HasOne(c => c.Order)
                    .WithOne(o => o.OnlineContract)
                    .HasForeignKey<OnlineContract>(c => c.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(c => c.ContractNumber)
                    .HasMaxLength(50);

                entity.Property(c => c.Status)
                    .HasMaxLength(50);

                entity.Property(c => c.Terms)
                    .HasColumnType("nvarchar(max)");

                entity.Property(c => c.SignedFromIpAddress)
                    .HasMaxLength(45);

                entity.Property(c => c.PdfFilePath)
                    .HasMaxLength(500);

                // Unique constraint cho ContractNumber
                entity.HasIndex(c => c.ContractNumber).IsUnique();

                // Index
                entity.HasIndex(c => c.OrderId).IsUnique();
                entity.HasIndex(c => c.Status);
            });

            // ============================================
            // VEHICLE INSPECTION REPORT CONFIGURATION
            // ============================================
            modelBuilder.Entity<VehicleInspectionReport>(entity =>
            {
                entity.HasKey(v => v.InspectionId);

                // One-to-Many relationship: Order - VehicleInspectionReports
                entity.HasOne(v => v.Order)
                    .WithMany()
                    .HasForeignKey(v => v.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(v => v.InspectionType)
                    .HasMaxLength(50);

                entity.Property(v => v.OverallCondition)
                    .HasMaxLength(50);

                entity.Property(v => v.CompensationAmount)
                    .HasColumnType("decimal(18,2)");

                entity.Property(v => v.CompensationStatus)
                    .HasMaxLength(50);

                entity.Property(v => v.Status)
                    .HasMaxLength(50);

                // Index
                entity.HasIndex(v => v.OrderId);
                entity.HasIndex(v => v.InspectionType);
                entity.HasIndex(v => v.Status);
            });

            // ============================================
            // INSPECTION DETAIL CONFIGURATION
            // ============================================
            modelBuilder.Entity<InspectionDetail>(entity =>
            {
                entity.HasKey(d => d.DetailId);

                // One-to-Many relationship: VehicleInspectionReport - InspectionDetails
                entity.HasOne(d => d.Inspection)
                    .WithMany(v => v.InspectionDetails)
                    .HasForeignKey(d => d.InspectionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(d => d.Category)
                    .HasMaxLength(50);

                entity.Property(d => d.ItemName)
                    .HasMaxLength(100);

                entity.Property(d => d.Severity)
                    .HasMaxLength(50);

                entity.Property(d => d.Status)
                    .HasMaxLength(50);

                entity.Property(d => d.CompensationAmount)
                    .HasColumnType("decimal(18,2)");

                entity.Property(d => d.Location)
                    .HasMaxLength(100);

                // Index
                entity.HasIndex(d => d.InspectionId);
                entity.HasIndex(d => d.Category);
                entity.HasIndex(d => d.HasIssue);
            });

            // ============================================
            // INSPECTION IMAGE CONFIGURATION
            // ============================================
            modelBuilder.Entity<InspectionImage>(entity =>
            {
                entity.HasKey(i => i.ImageId);

                // One-to-Many relationship: VehicleInspectionReport - InspectionImages
                // SỬA: Đổi từ Cascade sang NoAction để tránh multiple cascade paths
                entity.HasOne(i => i.Inspection)
                    .WithMany(v => v.InspectionImages)
                    .HasForeignKey(i => i.InspectionId)
                    .OnDelete(DeleteBehavior.NoAction); // ĐÃ SỬA: Cascade → NoAction

                // One-to-Many relationship: InspectionDetail - InspectionImages (Optional)
                entity.HasOne(i => i.Detail)
                    .WithMany()
                    .HasForeignKey(i => i.DetailId)
                    .OnDelete(DeleteBehavior.NoAction) // ĐÃ SỬA: SetNull → NoAction
                    .IsRequired(false);

                entity.Property(i => i.ImagePath)
                    .HasMaxLength(500);

                entity.Property(i => i.ImageType)
                    .HasMaxLength(50);

                // Index
                entity.HasIndex(i => i.InspectionId);
                entity.HasIndex(i => i.DetailId);
                entity.HasIndex(i => i.ImageType);
            });

            // ============================================
            // NOTIFICATION CONFIGURATION
            // ============================================
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(n => n.Id);

                // Index thường dùng cho notification
                entity.HasIndex(n => n.Created);
            });

            // ============================================
            // SOFTLOCK CONFIGURATION (Stage 1 Enhancement)
            // ============================================
            modelBuilder.Entity<SoftLock>(entity =>
            {
                entity.HasKey(sl => sl.LockToken);

                entity.Property(sl => sl.Status)
                    .HasMaxLength(20)
                    .IsRequired();

                // Indexes for performance
                entity.HasIndex(sl => sl.VehicleId);
                entity.HasIndex(sl => sl.UserId);
                entity.HasIndex(sl => sl.Status);
                entity.HasIndex(sl => sl.ExpiresAt);

                // Compound index for availability checks
                entity.HasIndex(sl => new { sl.VehicleId, sl.Status, sl.ExpiresAt });
            });
        }
    }
}