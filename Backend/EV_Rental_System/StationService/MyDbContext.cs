using Microsoft.EntityFrameworkCore;
using StationService.Models;

namespace StationService
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }

        public DbSet<Station> Stations { get; set; }
        public DbSet<StaffShift> StaffShifts { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; } 

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Station>(e =>
    {
                e.Property(x => x.Name).HasMaxLength(200).IsRequired();
                e.Property(x => x.Location).HasMaxLength(500).IsRequired();
                e.Property(x => x.Lat).IsRequired();
                e.Property(x => x.Lng).IsRequired();
                    });
            ConfigureStation(modelBuilder);
            ConfigureStaffShift(modelBuilder);
            ConfigureFeedback(modelBuilder);
            SeedData(modelBuilder);
        }

        private void ConfigureStation(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Station>(entity =>
            {
                // Primary Key
                entity.HasKey(e => e.Id);

                // Properties
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Location)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.ManagerId)
                    .IsRequired(false);

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValue(true);

                // Relationships
                entity.HasMany(s => s.StaffShifts)
                    .WithOne(ss => ss.Station)
                    .HasForeignKey(ss => ss.StationId)
                    .OnDelete(DeleteBehavior.Cascade);

                //
                entity.HasMany(s => s.Feedbacks)
                    .WithOne(f => f.Station)
                    .HasForeignKey(f => f.StationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ConfigureStaffShift(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StaffShift>(entity =>
            {
                // Primary Key
                entity.HasKey(e => e.Id);

                // Properties
                entity.Property(e => e.UserId)
                    .IsRequired();

                entity.Property(e => e.StationId)
                    .IsRequired();

                entity.Property(e => e.ShiftDate)
                    .IsRequired()
                    .HasColumnType("date");

                entity.Property(e => e.StartTime)
                    .IsRequired()
                    .HasColumnType("time");

                entity.Property(e => e.EndTime)
                    .IsRequired()
                    .HasColumnType("time");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValue("Scheduled");

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()"); // SQL Server
                                                      // .HasDefaultValueSql("NOW()"); // PostgreSQL/MySQL

                // Indexes for performance
                entity.HasIndex(e => new { e.UserId, e.ShiftDate })
                    .HasDatabaseName("IX_StaffShift_User_Date");

                entity.HasIndex(e => new { e.StationId, e.ShiftDate })
                    .HasDatabaseName("IX_StaffShift_Station_Date");

                // Unique constraint - prevent duplicate shifts
                entity.HasIndex(e => new { e.UserId, e.StationId, e.ShiftDate, e.StartTime })
                    .IsUnique()
                    .HasDatabaseName("UQ_StaffShift_UniqueShift");
            });
        }

        private void ConfigureFeedback(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Feedback>(entity =>
            {
                // Primary Key
                entity.HasKey(e => e.FeedbackId);

                // Properties
                entity.Property(e => e.Description)
                      .HasMaxLength(1000); // Giới hạn độ dài cho mô tả

                entity.Property(e => e.CreatedDate)
                      .IsRequired()
                      .HasDefaultValueSql("GETDATE()"); // Tự động lấy ngày giờ hiện tại
            });
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Station>().HasData(
                new Station
                {
                    Id = 1,
                    Name = "Trạm Đăng Kiểm Quận 1",
                    Location = "123 Nguyễn Huệ, Quận 1, TP.HCM",
                    ManagerId = null,
                    IsActive = true
                },
                new Station
                {
                    Id = 2,
                    Name = "Trạm Đăng Kiểm Thủ Đức",
                    Location = "456 Võ Văn Ngân, Thủ Đức, TP.HCM",
                    ManagerId = null,
                    IsActive = true
                },
                new Station
                {
                    Id = 3,
                    Name = "Trạm Đăng Kiểm Bình Thạnh",
                    Location = "789 Xô Viết Nghệ Tĩnh, Bình Thạnh, TP.HCM",
                    ManagerId = null,
                    IsActive = true
                }
            );
        }
    }
}