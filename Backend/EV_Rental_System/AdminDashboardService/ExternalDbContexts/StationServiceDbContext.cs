using Microsoft.EntityFrameworkCore;
using AdminDashboardService.ExternalModels.StationServiceModels;

namespace AdminDashboardService.ExternalDbContexts
{
    public class StationServiceDbContext : DbContext
    {
        public StationServiceDbContext(DbContextOptions<StationServiceDbContext> options) : base(options) { }

        public DbSet<Station> Stations { get; set; }
        public DbSet<StaffShift> StaffShifts { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("dbo");
        }
    }
}