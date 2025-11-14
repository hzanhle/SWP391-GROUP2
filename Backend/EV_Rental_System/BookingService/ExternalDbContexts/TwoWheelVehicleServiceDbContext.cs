using Microsoft.EntityFrameworkCore;
using BookingService.ExternalModels.TwoWheelVehicleServiceModels;

namespace BookingService.ExternalDbContexts
{
    public class TwoWheelVehicleServiceDbContext : DbContext
    {
        public TwoWheelVehicleServiceDbContext(DbContextOptions<TwoWheelVehicleServiceDbContext> options) : base(options) { }

        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Model> Models { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("dbo");

            // Cấu hình mối quan hệ
            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Model)
                .WithMany()
                .HasForeignKey(v => v.ModelId);
        }
    }
}

