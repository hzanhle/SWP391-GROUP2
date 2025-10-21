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
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task<Order?> GetByIdWithDetailsAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.Payment)
                .Include(o => o.OnlineContract)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _context.Orders
                .AsNoTracking()
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        // === UPDATE ===
        public async Task<bool> UpdateAsync(Order order)
        {
            _context.Orders.Update(order);
            return await _context.SaveChangesAsync() > 0;
        }

        // === DELETE ===
        public async Task<bool> DeleteAsync(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return false;

            _context.Orders.Remove(order);
            return await _context.SaveChangesAsync() > 0;
        }

        // === FILTER QUERIES ===
        public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId)
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetByVehicleIdAsync(int vehicleId)
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(o => o.VehicleId == vehicleId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status)
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetByUserIdAndStatusAsync(int userId, OrderStatus status)
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId && o.Status == status)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        // === AVAILABILITY CHECK ===
        public async Task<bool> IsVehicleAvailableAsync(int vehicleId, DateTime fromDate, DateTime toDate, int? excludeOrderId = null)
        {
            var overlapping = await GetOverlappingOrdersAsync(
                vehicleId, fromDate, toDate,
                new[] { OrderStatus.Pending, OrderStatus.Confirmed, OrderStatus.InProgress }
            );

            if (excludeOrderId.HasValue)
                overlapping = overlapping.Where(o => o.OrderId != excludeOrderId.Value);

            return !overlapping.Any();
        }

        public async Task<IEnumerable<Order>> GetOverlappingOrdersAsync(int vehicleId, DateTime fromDate, DateTime toDate, OrderStatus[] statuses)
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(o => o.VehicleId == vehicleId &&
                            statuses.Contains(o.Status) &&
                            o.FromDate < toDate &&
                            o.ToDate > fromDate)
                .ToListAsync();
        }

        // === BUSINESS QUERIES ===
        public async Task<IEnumerable<Order>> GetPendingOrdersAsync()
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(o => o.Status == OrderStatus.Pending)
                .OrderBy(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetExpiredPendingOrdersAsync()
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(o => o.Status == OrderStatus.Pending && o.ExpiresAt != null && o.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task<int> GetUserCompletedOrdersCountAsync(int userId)
        {
            return await _context.Orders
                .AsNoTracking()
                .CountAsync(o => o.UserId == userId && o.Status == OrderStatus.Completed);
        }

        public async Task<bool> HasCompletedOrderAsync(int userId)
        {
            return await _context.Orders
                .AsNoTracking()
                .AnyAsync(o => o.UserId == userId && o.Status == OrderStatus.Completed);
        }
    }
}
