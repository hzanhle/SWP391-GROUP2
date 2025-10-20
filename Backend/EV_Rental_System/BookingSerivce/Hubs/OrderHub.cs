using Microsoft.AspNetCore.SignalR;

namespace BookingSerivce.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time order notifications.
    /// Clients subscribe by joining a group with their userId.
    /// </summary>
    public class OrderHub : Hub
    {
        /// <summary>
        /// Called when client connects. Automatically joins a group for their user ID.
        /// </summary>
        public async Task JoinUserGroup(int userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        /// <summary>
        /// Called when client disconnects or manually leaves.
        /// </summary>
        public async Task LeaveUserGroup(int userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        /// <summary>
        /// Notify a specific user that their order has expired.
        /// </summary>
        public async Task NotifyOrderExpired(int userId, int orderId, string reason)
        {
            await Clients.Group($"user_{userId}").SendAsync("OrderExpired", new
            {
                OrderId = orderId,
                Reason = reason,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Notify a specific user that their payment was successful.
        /// </summary>
        public async Task NotifyPaymentSuccess(int userId, int orderId, int paymentId)
        {
            await Clients.Group($"user_{userId}").SendAsync("PaymentSuccess", new
            {
                OrderId = orderId,
                PaymentId = paymentId,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Notify a specific user that their payment failed.
        /// </summary>
        public async Task NotifyPaymentFailed(int userId, int orderId, string reason)
        {
            await Clients.Group($"user_{userId}").SendAsync("PaymentFailed", new
            {
                OrderId = orderId,
                Reason = reason,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Notify a specific user about order status changes.
        /// </summary>
        public async Task NotifyOrderStatusChanged(int userId, int orderId, string oldStatus, string newStatus)
        {
            await Clients.Group($"user_{userId}").SendAsync("OrderStatusChanged", new
            {
                OrderId = orderId,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Stage 3 Enhancement: Notify customer that vehicle pickup has been confirmed by staff.
        /// </summary>
        public async Task NotifyPickupConfirmed(int userId, int orderId, string staffName, DateTime pickupTime)
        {
            await Clients.Group($"user_{userId}").SendAsync("PickupConfirmed", new
            {
                OrderId = orderId,
                Message = $"Your vehicle is ready! Staff {staffName} has prepared it for you.",
                PickupTime = pickupTime,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Stage 3 Enhancement: Notify customer that vehicle return has been confirmed by staff.
        /// </summary>
        public async Task NotifyReturnConfirmed(int userId, int orderId, string staffName, DateTime returnTime)
        {
            await Clients.Group($"user_{userId}").SendAsync("ReturnConfirmed", new
            {
                OrderId = orderId,
                Message = $"Vehicle return confirmed by staff {staffName}. Inspection will begin shortly.",
                ReturnTime = returnTime,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Stage 3 Enhancement: Notify customer about inspection completion.
        /// </summary>
        public async Task NotifyInspectionComplete(int userId, int orderId, bool hasDamage, decimal? damageCharge)
        {
            var message = hasDamage
                ? $"Inspection complete. Damage found - additional charge: {damageCharge:N0} VND"
                : "Inspection complete. No damage found. Your deposit will be refunded.";

            await Clients.Group($"user_{userId}").SendAsync("InspectionComplete", new
            {
                OrderId = orderId,
                HasDamage = hasDamage,
                DamageCharge = damageCharge,
                Message = message,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
