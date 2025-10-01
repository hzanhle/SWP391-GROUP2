using Microsoft.EntityFrameworkCore;
using StationService.Models;

namespace StationService
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }

        public DbSet<Station> Stations { get; set; }
        public DbSet<StaffShift> StaffShifts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình Station
            modelBuilder.Entity<Station>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Location)
                    .IsRequired()
                    .HasMaxLength(500);

                // Relationship: 1 Station có nhiều StaffShifts
                entity.HasMany(s => s.StaffShifts)
                    .WithOne(ss => ss.Station)
                    .HasForeignKey(ss => ss.StationId)
                    .OnDelete(DeleteBehavior.Cascade); // Xóa Station → xóa tất cả shifts
            });

            // Cấu hình StaffShift
            modelBuilder.Entity<StaffShift>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.UserId)
                    .IsRequired();

                entity.Property(e => e.StationId)
                    .IsRequired();

                entity.Property(e => e.ShiftDate)
                    .IsRequired();

                entity.Property(e => e.StartTime)
                    .IsRequired();

                entity.Property(e => e.EndTime)
                    .IsRequired();

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .HasDefaultValue("Scheduled");

                // Index để tìm kiếm nhanh
                entity.HasIndex(e => new { e.UserId, e.ShiftDate })
                    .HasDatabaseName("IX_StaffShift_User_Date");

                entity.HasIndex(e => new { e.StationId, e.ShiftDate })
                    .HasDatabaseName("IX_StaffShift_Station_Date");

                // Constraint: Không cho trùng ca (cùng user, station, date, time)
                entity.HasIndex(e => new { e.UserId, e.StationId, e.ShiftDate, e.StartTime })
                    .IsUnique()
                    .HasDatabaseName("UQ_StaffShift_UniqueShift");
            });

            // Seed data mẫu (optional)
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Stations
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
                }
            );

            // Seed StaffShifts
            modelBuilder.Entity<StaffShift>().HasData(
                new StaffShift
                {
                    Id = 1,
                    UserId = 1,
                    StationId = 1,
                    ShiftDate = DateOnly.FromDateTime(DateTime.Now),
                    StartTime = new TimeOnly(8, 0),
                    EndTime = new TimeOnly(12, 0),
                    Status = "Scheduled",
                    CreatedAt = DateTime.Now
                },
                new StaffShift
                {
                    Id = 2,
                    UserId = 2,
                    StationId = 1,
                    ShiftDate = DateOnly.FromDateTime(DateTime.Now),
                    StartTime = new TimeOnly(13, 0),
                    EndTime = new TimeOnly(17, 0),
                    Status = "Scheduled",
                    CreatedAt = DateTime.Now
                }
            );
        }
    }
}