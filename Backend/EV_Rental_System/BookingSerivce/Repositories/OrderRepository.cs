using BookingSerivce;
using BookingSerivce.Models;
using BookingService.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BookingSerivce.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly MyDbContext _context;

        public OrderRepository(MyDbContext context)
        {
            _context = context;
        }

        // ===== BASIC CRUD =====
        public async Task<Order?> GetByIdAsync(int orderId)
        {
            return await _context.Orders.FindAsync(orderId);
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _context.Orders
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<Order> AddAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<Order> UpdateAsync(Order order)
        {
            order.UpdatedAt = DateTime.UtcNow;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task DeleteAsync(int orderId)
        {
            var order = await GetByIdAsync(orderId);
            if (order != null)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int orderId)
        {
            return await _context.Orders.AnyAsync(o => o.OrderId == orderId);
        }

        // ===== QUERY METHODS =====
        public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetByVehicleIdAsync(int vehicleId)
        {
            return await _context.Orders
                .Where(o => o.VehicleId == vehicleId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetByStatusAsync(string status)
        {
            return await _context.Orders
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetPendingOrdersAsync()
        {
            return await _context.Orders
                .Where(o => o.Status == "Pending")
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetConfirmedOrdersAsync()
        {
            return await _context.Orders
                .Where(o => o.Status == "Confirmed")
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetCompletedOrdersAsync()
        {
            return await _context.Orders
                .Where(o => o.Status == "Completed")
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        // ===== ADVANCED QUERIES =====
        public async Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersWithPaymentAsync()
        {
            return await _context.Orders
                .Include(o => o.Payment)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderWithPaymentByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task<IEnumerable<Order>> GetActiveOrdersAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.Orders
                .Where(o => o.FromDate <= now && o.ToDate >= now && o.Status == "Confirmed")
                .OrderBy(o => o.ToDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetUpcomingOrdersAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.Orders
                .Where(o => o.FromDate > now && o.Status == "Confirmed")
                .OrderBy(o => o.FromDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> FindAsync(Expression<Func<Order, bool>> predicate)
        {
            return await _context.Orders
                .Where(predicate)
                .ToListAsync();
        }

        // ===== USER SPECIFIC QUERIES =====
        public async Task<IEnumerable<Order>> GetUserOrderHistoryAsync(int userId)
        {
            return await _context.Orders
                .Include(o => o.Payment)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetUserActiveOrdersAsync(int userId)
        {
            var now = DateTime.UtcNow;
            return await _context.Orders
                .Include(o => o.Payment)
                .Where(o => o.UserId == userId &&
                           o.FromDate <= now &&
                           o.ToDate >= now &&
                           o.Status == "Confirmed")
                .ToListAsync();
        }

        public async Task<Order?> GetUserLatestOrderAsync(int userId)
        {
            return await _context.Orders
                .Include(o => o.Payment)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();
        }

        // ===== VEHICLE SPECIFIC QUERIES =====
        public async Task<IEnumerable<Order>> GetVehicleBookingHistoryAsync(int vehicleId)
        {
            return await _context.Orders
                .Include(o => o.Payment)
                .Where(o => o.VehicleId == vehicleId)
                .OrderByDescending(o => o.FromDate)
                .ToListAsync();
        }

        public async Task<bool> IsVehicleAvailableAsync(int vehicleId, DateTime fromDate, DateTime toDate)
        {
            // Xe available nếu KHÔNG có booking nào overlap với khoảng thời gian này
            var hasConflict = await _context.Orders
                .AnyAsync(o => o.VehicleId == vehicleId &&
                              o.Status != "Cancelled" &&
                              o.Status != "Completed" &&
                              ((o.FromDate <= toDate && o.ToDate >= fromDate)));

            return !hasConflict;
        }

        public async Task<IEnumerable<Order>> GetVehicleBookingsInRangeAsync(int vehicleId, DateTime fromDate, DateTime toDate)
        {
            return await _context.Orders
                .Where(o => o.VehicleId == vehicleId &&
                           o.Status != "Cancelled" &&
                           ((o.FromDate <= toDate && o.ToDate >= fromDate)))
                .OrderBy(o => o.FromDate)
                .ToListAsync();
        }

        // ===== STATISTICS =====
        public async Task<int> GetTotalOrdersCountAsync()
        {
            return await _context.Orders.CountAsync();
        }

        public async Task<int> GetOrderCountByStatusAsync(string status)
        {
            return await _context.Orders
                .CountAsync(o => o.Status == status);
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _context.Orders
                .Where(o => o.Status == "Completed")
                .SumAsync(o => o.TotalCost);
        }

        public async Task<decimal> GetRevenueByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .Where(o => o.Status == "Completed" &&
                           o.CreatedAt >= startDate &&
                           o.CreatedAt <= endDate)
                .SumAsync(o => o.TotalCost);
        }

        public async Task<Dictionary<string, int>> GetOrderCountByStatusGroupAsync()
        {
            return await _context.Orders
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);
        }

        public async Task<int> GetUserOrderCountAsync(int userId)
        {
            return await _context.Orders
                .CountAsync(o => o.UserId == userId);
        }

        // ===== ADDITIONAL METHODS FROM LAM BRANCH =====
        public async Task<IEnumerable<Order>> GetOrdersByVehicleAsync(int vehicleId)
        {
            return await GetByVehicleIdAsync(vehicleId);
        }

        public async Task<Order?> GetOrderWithPaymentAsync(int orderId)
        {
            return await GetOrderWithPaymentByIdAsync(orderId);
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
