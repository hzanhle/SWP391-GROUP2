using BookingService.Models;

namespace BookingService.Repositories
{
    public interface IVehicleReturnRepository
    {
        Task<VehicleReturn?> GetByIdAsync(int returnId);
        Task<VehicleReturn?> GetByOrderIdAsync(int orderId);
        Task<VehicleReturn> CreateAsync(VehicleReturn vehicleReturn);
        Task<VehicleReturn> UpdateAsync(VehicleReturn vehicleReturn);
        Task<bool> DeleteAsync(int returnId);
        Task<List<VehicleReturn>> GetAllAsync();
    }
}
