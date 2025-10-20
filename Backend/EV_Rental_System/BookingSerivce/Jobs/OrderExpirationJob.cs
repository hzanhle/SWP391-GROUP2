using BookingSerivce.Hubs;
using BookingSerivce.Repositories;
using Microsoft.AspNetCore.SignalR;

namespace BookingSerivce.Jobs
{
    /// <summary>
    /// Background job that expires pending orders after 5 minutes.
    /// Runs every 30 seconds to check for expired orders.
    /// </summary>
    public class OrderExpirationJob
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IHubContext<OrderHub> _hubContext;
        private readonly ILogger<OrderExpirationJob> _logger;

        public OrderExpirationJob(
            IOrderRepository orderRepository,
            IHubContext<OrderHub> hubContext,
            ILogger<OrderExpirationJob> logger)
        {
            _orderRepository = orderRepository;
            _hubContext = hubContext;
            _logger = logger;
        }

        /// <summary>
        /// Processes expired orders and sends SignalR notifications.
        /// Called by Hangfire every 30 seconds.
        /// </summary>
        public async Task ProcessExpiredOrdersAsync()
        {
            try
            {
                var expiredOrders = await _orderRepository.GetExpiredOrdersAsync();
                var expiredList = expiredOrders.ToList();

                if (!expiredList.Any())
                {
                    _logger.LogDebug("No expired orders found");
                    return;
                }

                _logger.LogInformation($"Processing {expiredList.Count} expired orders");

                foreach (var order in expiredList)
                {
                    try
                    {
                        // Update order status
                        order.Status = "Expired";
                        order.CancellationReason = "Payment not initiated within 5 minutes";
                        order.UpdatedAt = DateTime.UtcNow;

                        await _orderRepository.UpdateAsync(order);

                        // Send SignalR notification to user
                        await _hubContext.Clients.Group($"user_{order.UserId}")
                            .SendAsync("OrderExpired", new
                            {
                                OrderId = order.OrderId,
                                Reason = order.CancellationReason,
                                Timestamp = DateTime.UtcNow
                            });

                        _logger.LogInformation($"Order {order.OrderId} expired successfully");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error expiring order {order.OrderId}");
                    }
                }

                _logger.LogInformation($"Successfully processed {expiredList.Count} expired orders");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OrderExpirationJob");
                throw;
            }
        }
    }
}
