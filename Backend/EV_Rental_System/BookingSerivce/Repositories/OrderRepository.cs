using BookingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly MyDbContext _context;

        public OrderRepository(MyDbContext context)
        {
            _context = context;
        }

        // === CREATE ===
        public async Task<Order> CreateAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            return order;
        }

        // === READ ===
        public async Task<Order?> GetByIdAsync(int orderId)
        {
            return await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task<Order?> GetByIdWithDetailsAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.Payment)
                .Include(o => o.OnlineContract)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task<List<Order>> GetByUserIdAsync(int userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Order>> GetByVehicleIdAsync(int vehicleId)
        {
            return await _context.Orders
                .Where(o => o.VehicleId == vehicleId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Order>> GetByStatusAsync(OrderStatus status)
        {
            return await _context.Orders
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Order>> GetAllAsync()
        {
            return await _context.Orders
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        // === UPDATE ===
        public async Task<Order> UpdateAsync(Order order)
        {
            order.UpdatedAt = DateTime.UtcNow;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            return order;
        }

        // === DELETE ===
        public async Task DeleteAsync(int orderId)
        {
            var order = await GetByIdAsync(orderId);
            if (order == null)
            {
                throw new KeyNotFoundException($"Order with ID {orderId} not found");
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
        }

        // === AVAILABILITY CHECK ===
        public async Task<bool> IsVehicleAvailableAsync(int vehicleId, DateTime fromDate, DateTime toDate, int? excludeOrderId = null)
        {
            var conflictingOrders = await GetConflictingOrdersAsync(vehicleId, fromDate, toDate);

            // Nếu có excludeOrderId (dùng cho update), bỏ qua order đó
            if (excludeOrderId.HasValue)
            {
                conflictingOrders = conflictingOrders
                    .Where(o => o.OrderId != excludeOrderId.Value)
                    .ToList();
            }

            return conflictingOrders.Count == 0;
        }

        public async Task<List<Order>> GetConflictingOrdersAsync(int vehicleId, DateTime fromDate, DateTime toDate)
        {
            return await _context.Orders
                .Where(o => o.VehicleId == vehicleId)
                .Where(o => o.Status != OrderStatus.Cancelled && o.Status != OrderStatus.Completed)
                .Where(o =>
                    // Check overlap:
                    // Case 1: Order bắt đầu trong khoảng cần check
                    (o.FromDate < toDate && o.ToDate > fromDate)
                )
                .ToListAsync();
        }

        // === BUSINESS QUERIES ===
        public async Task<List<Order>> GetPendingOrdersAsync()
        {
            return await _context.Orders
                .Where(o => o.Status == OrderStatus.Pending)
                .Include(o => o.Payment)
                .Include(o => o.OnlineContract)
                .OrderBy(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Order>> GetExpiredPendingOrdersAsync(int minutesThreshold)
        {
            var cutoffTime = DateTime.UtcNow.AddMinutes(-minutesThreshold);

            return await _context.Orders
                .Where(o => o.Status == OrderStatus.Pending)
                .Where(o => o.CreatedAt < cutoffTime)
                .Include(o => o.Payment)
                .Include(o => o.OnlineContract)
                .ToListAsync();
        }

        public async Task<int> GetUserCompletedOrdersCountAsync(int userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .Where(o => o.Status == OrderStatus.Completed)
                .CountAsync();
        }

        public async Task<bool> HasCompletedOrderAsync(int userId)
        {
            return await _context.Orders
                .AnyAsync(o => o.UserId == userId && o.Status == OrderStatus.Completed);
        }

        public async Task<IEnumerable<Order>> GetByUserIdAndStatusAsync(int userId, OrderStatus status)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId && o.Status == status)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOverlappingOrdersAsync(
            int vehicleId,
            DateTime fromDate,
            DateTime toDate,
            OrderStatus[] statuses)
        {
            return await _context.Orders
                .Where(o => o.VehicleId == vehicleId &&
                            statuses.Contains(o.Status) &&
                            o.FromDate < toDate &&
                            o.ToDate > fromDate)
                .ToListAsync();
        }
        
    }
}
