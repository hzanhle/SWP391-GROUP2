// File: AdminDashboardService/Data/MyDbContext.cs

using BookingSerivce.Models;
using BookingService.Models;
using Microsoft.EntityFrameworkCore;
using StationService.Models;
using TwoWheelVehicleService.Models;
// Quan trọng: Bác cần using namespace của các Model từ các project khác
using UserService.Models;

namespace AdminDashboardService.Data
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }

        // Khai báo DbSet cho tất cả các bảng mà Dashboard cần truy vấn
        public DbSet<User> Users { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<OnlineContract> OnlineContracts { get; set; }
        public DbSet<Station> Stations { get; set; }

        // Chúng ta không cần DbSet<Image> ở đây vì EF Core sẽ tự phát hiện
        // thông qua các mối quan hệ trong User và Vehicle.

        // GHI ĐÈ PHƯƠNG THỨC NÀY ĐỂ GIẢI QUYẾT XUNG ĐỘT
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Chỉ thị cho EF Core ánh xạ các class 'Image' vào các bảng riêng biệt
            // để tránh xung đột tên.
            modelBuilder.Entity<UserService.Models.Image>().ToTable("UserImages");
            modelBuilder.Entity<TwoWheelVehicleService.Models.Image>().ToTable("VehicleImages");
        }
    }
}