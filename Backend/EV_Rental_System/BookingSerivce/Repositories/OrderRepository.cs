using BookingSerivce;
using BookingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingSerivce.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly MyDbContext _context;

        public OrderRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<Order> AddAsync(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<Order?> GetByIdAsync(int orderId)
        {
            return await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task<IEnumerable<Order>> GetUserOrderHistoryAsync(int userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<Order> UpdateAsync(Order order)
        {
            order.UpdatedAt = DateTime.UtcNow;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<bool> IsVehicleAvailableAsync(int vehicleId, DateTime fromDate, DateTime toDate)
        {
            // Check if there are any overlapping orders for this vehicle
            var overlappingOrders = await GetOverlappingOrdersAsync(vehicleId, fromDate, toDate);
            return !overlappingOrders.Any();
        }

        public async Task<IEnumerable<Order>> GetOrdersByVehicleAsync(int vehicleId)
        {
            return await _context.Orders
                .Where(o => o.VehicleId == vehicleId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderWithPaymentAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task<Order?> GetOrderWithContractAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.OnlineContract)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task<IEnumerable<Order>> GetOverlappingOrdersAsync(int vehicleId, DateTime fromDate, DateTime toDate)
        {
            // Check for orders that overlap with the requested date range
            // Exclude cancelled or completed orders
            return await _context.Orders
                .Where(o => o.VehicleId == vehicleId
                    && o.Status != "Cancelled"
                    && o.Status != "Completed"
                    && (
                        // New booking starts during existing booking
                        (fromDate >= o.FromDate && fromDate < o.ToDate)
                        // New booking ends during existing booking
                        || (toDate > o.FromDate && toDate <= o.ToDate)
                        // New booking completely contains existing booking
                        || (fromDate <= o.FromDate && toDate >= o.ToDate)
                    ))
                .ToListAsync();
        }
    }
}
