using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace BookingService.Services.SignalR
{
    public class OrderTimerHub : Hub
    {
        private readonly ILogger<OrderTimerHub> _logger;
        public OrderTimerHub(ILogger<OrderTimerHub> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Client sẽ gọi hàm này sau khi tạo Order thành công
        /// để lắng nghe các sự kiện của Order đó.
        /// </summary>
        public async Task JoinOrderGroup(string orderId)
        {
            var groupName = $"order_{orderId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation($"Client {Context.ConnectionId} joined group {groupName}");
        }

        /// <summary>
        /// Client gọi khi rời khỏi trang thanh toán.
        /// </summary>
        public async Task LeaveOrderGroup(string orderId)
        {
            var groupName = $"order_{orderId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation($"Client {Context.ConnectionId} left group {groupName}");
        }
    }
}
