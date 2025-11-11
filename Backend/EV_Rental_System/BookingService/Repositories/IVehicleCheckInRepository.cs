using BookingService.Models;

namespace BookingService.Repositories
{
    public interface IVehicleCheckInRepository
    {
        Task<VehicleCheckIn?> GetByIdAsync(int checkInId);
        Task<VehicleCheckIn?> GetByOrderIdAsync(int orderId);
        Task<VehicleCheckIn> CreateAsync(VehicleCheckIn checkIn);
        Task<VehicleCheckIn> UpdateAsync(VehicleCheckIn checkIn);
        Task<bool> DeleteAsync(int checkInId);
        Task<List<VehicleCheckIn>> GetAllAsync();
    }
}
