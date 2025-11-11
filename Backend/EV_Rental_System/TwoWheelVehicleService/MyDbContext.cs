using Microsoft.EntityFrameworkCore;
using TwoWheelVehicleService.Models;

namespace TwoWheelVehicleService
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }

        public DbSet<Model> Models { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<VehicleStatusHistory> VehicleStatusHistories { get; set; }
        public DbSet<TransferVehicle> TransferVehicles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Quan hệ Model - Vehicle (1-n)
            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Model)
                .WithMany(m => m.Vehicles)
                .HasForeignKey(v => v.ModelId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ Model - Image (1-n)
            modelBuilder.Entity<Image>()
                .HasOne(i => i.Model)
                .WithMany(m => m.Images)
                .HasForeignKey(i => i.ModelId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

}
