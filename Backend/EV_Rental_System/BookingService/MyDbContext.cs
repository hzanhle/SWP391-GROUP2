﻿using BookingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingService
{
    public class MyDbContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<OnlineContract> OnlineContracts { get; set; }
        public DbSet<TrustScore> TrustScores { get; set; }
        public DbSet<TrustScoreHistory> TrustScoreHistories { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Settlement> Settlements { get; set; }

        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =========================
            // ORDER
            // =========================
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.OrderId);

                entity.Property(o => o.HourlyRate).HasColumnType("decimal(18,2)");
                entity.Property(o => o.TotalCost).HasColumnType("decimal(18,2)");
                entity.Property(o => o.DepositAmount).HasColumnType("decimal(18,2)");

                entity.Property(o => o.Status)
                      .HasConversion<string>()
                      .HasMaxLength(50);

                entity.HasIndex(o => o.UserId);
                entity.HasIndex(o => o.VehicleId);
                entity.HasIndex(o => o.Status);
                entity.HasIndex(o => o.CreatedAt);
                entity.HasIndex(o => new { o.FromDate, o.ToDate });
            });

            // =========================
            // PAYMENT
            // =========================
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(p => p.PaymentId);

                entity.Property(p => p.Amount).HasColumnType("decimal(18,2)");
                entity.Property(p => p.PaymentMethod).HasMaxLength(50);
                entity.Property(p => p.TransactionId).HasMaxLength(100);
                entity.Property(p => p.PaymentGatewayResponse).HasColumnType("nvarchar(max)");

                entity.Property(p => p.Status)
                      .HasConversion<string>()
                      .HasMaxLength(50);

                // 1-1 với Order
                entity.HasOne(p => p.Order)
                      .WithOne(o => o.Payment)
                      .HasForeignKey<Payment>(p => p.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(p => p.OrderId).IsUnique();
                entity.HasIndex(p => p.Status);
                entity.HasIndex(p => p.TransactionId);
            });

            // =========================
            // ONLINE CONTRACT
            // =========================
            modelBuilder.Entity<OnlineContract>(entity =>
            {
                entity.HasKey(c => c.OnlineContractId);

                entity.Property(c => c.ContractNumber).HasMaxLength(50).IsRequired();
                entity.Property(c => c.ContractFilePath).HasMaxLength(500);
                entity.Property(c => c.SignatureData).HasMaxLength(100);

                // 1-1 với Order
                entity.HasOne(c => c.Order)
                      .WithOne(o => o.OnlineContract)
                      .HasForeignKey<OnlineContract>(c => c.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(c => c.OrderId).IsUnique();
                entity.HasIndex(c => c.ContractNumber).IsUnique();
            });

            // =========================
            // TRUST SCORE
            // =========================
            modelBuilder.Entity<TrustScore>(entity =>
            {
                entity.HasKey(t => t.TrustScoreId);
                entity.Property(t => t.Score).HasColumnType("int");
                entity.HasIndex(t => t.UserId);
                entity.HasIndex(t => t.CreatedAt);
            });

            // =========================
            // TRUST SCORE HISTORY
            // =========================
            modelBuilder.Entity<TrustScoreHistory>(entity =>
            {
                entity.HasKey(h => h.HistoryId);

                entity.Property(h => h.Reason).HasMaxLength(500).IsRequired();
                entity.Property(h => h.ChangeType).HasMaxLength(50).IsRequired();

                // Indexes for queries
                entity.HasIndex(h => h.UserId);
                entity.HasIndex(h => h.OrderId);
                entity.HasIndex(h => h.CreatedAt);
                entity.HasIndex(h => h.ChangeType);
            });

            // =========================
            // NOTIFICATION
            // =========================
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(n => n.Id);
                entity.Property(n => n.Title).HasMaxLength(200);
                entity.Property(n => n.Description).HasMaxLength(1000);
                entity.Property(n => n.DataType).HasMaxLength(50);

                entity.HasIndex(n => n.UserId);
                entity.HasIndex(n => n.Created);
                entity.HasIndex(n => new { n.DataType, n.DataId });
            });

            // =========================
            // SETTLEMENT
            // =========================
            modelBuilder.Entity<Settlement>(entity =>
            {
                entity.HasKey(s => s.SettlementId);

                // Decimal precision
                entity.Property(s => s.OvertimeHours).HasColumnType("decimal(10,2)");
                entity.Property(s => s.OvertimeFee).HasColumnType("decimal(18,2)");
                entity.Property(s => s.DamageCharge).HasColumnType("decimal(18,2)");
                entity.Property(s => s.InitialDeposit).HasColumnType("decimal(18,2)");
                entity.Property(s => s.TotalAdditionalCharges).HasColumnType("decimal(18,2)");
                entity.Property(s => s.DepositRefundAmount).HasColumnType("decimal(18,2)");
                entity.Property(s => s.AdditionalPaymentRequired).HasColumnType("decimal(18,2)");

                // String lengths
                entity.Property(s => s.DamageDescription).HasMaxLength(1000);
                entity.Property(s => s.InvoiceUrl).HasMaxLength(500);

                // 1-1 relationship with Order (optional - an order may not have settlement yet)
                entity.HasOne(s => s.Order)
                      .WithOne()
                      .HasForeignKey<Settlement>(s => s.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Indexes for queries
                entity.HasIndex(s => s.OrderId).IsUnique();
                entity.HasIndex(s => s.IsFinalized);
                entity.HasIndex(s => s.CreatedAt);
            });
        }
    }
}
