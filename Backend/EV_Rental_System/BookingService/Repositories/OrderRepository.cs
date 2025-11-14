using BookingService.Models;
using BookingService.ExternalDbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookingService.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly MyDbContext _context;
        private readonly TwoWheelVehicleServiceDbContext? _vehicleDb;

        public OrderRepository(MyDbContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            // Try to get external DbContext if available
            _vehicleDb = serviceProvider.GetService<TwoWheelVehicleServiceDbContext>();
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
                .Include(o => o.OnlineContract)
                .Include(o => o.Payments)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _context.Orders
                .Include(o => o.Payments)
                .Include(o => o.OnlineContract)
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

        public async Task<Dictionary<int, int>> GetOrderCountByHourAsync()
        {
            var orders = await _context.Orders
                .Select(o => o.CreatedAt.Hour)
                .ToListAsync();

            return orders
                .GroupBy(hour => hour)
                .ToDictionary(g => g.Key, g => g.Count())
                .OrderBy(kvp => kvp.Key)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public async Task<List<int>> GetTopPeakHoursAsync(int topCount = 3)
        {
            var ordersByHour = await GetOrderCountByHourAsync();

            return ordersByHour
                .OrderByDescending(kvp => kvp.Value)
                .Take(topCount)
                .Select(kvp => kvp.Key)
                .OrderBy(hour => hour)
                .ToList();
        }

        // === STAFF QUERIES ===
        public async Task<IEnumerable<Order>> GetByStationIdAsync(int stationId, OrderStatus? status = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            if (_vehicleDb == null)
            {
                throw new InvalidOperationException("TwoWheelVehicleServiceDbContext is not configured. Please check connection string.");
            }

            // Get vehicle IDs for the station
            var vehicleIds = await _vehicleDb.Vehicles
                .Where(v => v.StationId == stationId)
                .Select(v => v.VehicleId)
                .ToListAsync();

            if (!vehicleIds.Any())
            {
                return Enumerable.Empty<Order>();
            }

            // Query orders for those vehicles
            var query = _context.Orders
                .Include(o => o.Payments)
                .Include(o => o.OnlineContract)
                .AsNoTracking()
                .Where(o => vehicleIds.Contains(o.VehicleId));

            // Apply status filter
            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            // Apply date filters
            if (fromDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt <= toDate.Value);
            }

            return await query
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }
    }
}
