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

                entity.Property(e => e.Lat)
                    .IsRequired();

                entity.Property(e => e.Lng)
                    .IsRequired();

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
                // Primary key
                entity.HasKey(e => e.Id);

                // Relationships
                entity.HasOne(e => e.Station)
                    .WithMany(s => s.StaffShifts)
                    .HasForeignKey(e => e.StationId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Properties
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.StationId).IsRequired();

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
                    .HasDefaultValueSql("SYSUTCDATETIME()");

                entity.Property(e => e.ActualCheckInTime)
                    .HasColumnType("datetime2")
                    .IsRequired(false);

                entity.Property(e => e.ActualCheckOutTime)
                    .HasColumnType("datetime2")
                    .IsRequired(false);

                entity.Property(e => e.Notes)
                    .HasMaxLength(500)
                    .IsRequired(false);

                entity.Property(e => e.CancellationReason)
                    .HasMaxLength(500)
                    .IsRequired(false);

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime2")
                    .IsRequired(false);

                // Indexes
                entity.HasIndex(e => new { e.UserId, e.ShiftDate })
                    .HasDatabaseName("IX_StaffShift_User_Date");

                entity.HasIndex(e => new { e.StationId, e.ShiftDate })
                    .HasDatabaseName("IX_StaffShift_Station_Date");

                entity.HasIndex(e => e.Status)
                    .HasDatabaseName("IX_StaffShift_Status");

                // Unique constraint
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

                // Required Fields
                entity.Property(e => e.StationId).IsRequired();
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.Rate).IsRequired();

                // Optional Fields
                entity.Property(e => e.Description)
                    .HasMaxLength(1000)
                    .IsRequired(false);

                entity.Property(e => e.CreatedDate)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.UpdatedDate)
                    .HasColumnType("datetime2")
                    .IsRequired(false);

                entity.Property(e => e.IsVerified)
                    .IsRequired()
                    .HasDefaultValue(false);

                entity.Property(e => e.IsPublished)
                    .IsRequired()
                    .HasDefaultValue(true);

                // 1 User chỉ feedback 1 lần cho 1 Station
                entity.HasIndex(e => new { e.UserId, e.StationId })
                    .IsUnique()
                    .HasDatabaseName("UQ_Feedback_UserStation");

                // Indexes for performance
                entity.HasIndex(e => e.StationId)
                    .HasDatabaseName("IX_Feedback_Station");

                entity.HasIndex(e => e.UserId)
                    .HasDatabaseName("IX_Feedback_User");

                entity.HasIndex(e => e.Rate)
                    .HasDatabaseName("IX_Feedback_Rate");

                entity.HasIndex(e => e.IsPublished)
                    .HasDatabaseName("IX_Feedback_IsPublished");

                entity.HasIndex(e => e.CreatedDate)
                    .HasDatabaseName("IX_Feedback_CreatedDate");

                // Relationship to Station
                entity.HasOne(e => e.Station)
                    .WithMany(s => s.Feedbacks)
                    .HasForeignKey(e => e.StationId)
                    .OnDelete(DeleteBehavior.Cascade);
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
                    Lat = 10.7769,
                    Lng = 106.7009,
                    ManagerId = null,
                    IsActive = true
                },
                new Station
                {
                    Id = 2,
                    Name = "Trạm Đăng Kiểm Thủ Đức",
                    Location = "456 Võ Văn Ngân, Thủ Đức, TP.HCM",
                    Lat = 10.8505,
                    Lng = 106.7717,
                    ManagerId = null,
                    IsActive = true
                },
                new Station
                {
                    Id = 3,
                    Name = "Trạm Đăng Kiểm Bình Thạnh",
                    Location = "789 Xô Viết Nghệ Tĩnh, Bình Thạnh, TP.HCM",
                    Lat = 10.8014,
                    Lng = 106.7105,
                    ManagerId = null,
                    IsActive = true
                }
            );
        }
    }
}