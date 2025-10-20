using BookingSerivce.Repositories;

namespace BookingSerivce.Services
{
    /// <summary>
    /// Mock implementation of TrustScoreService.
    /// In production, this would calculate scores based on:
    /// - Rental history
    /// - Payment reliability
    /// - Damage/incident history
    /// - Account age
    /// - Verification level
    /// </summary>
    public class TrustScoreService : ITrustScoreService
    {
        private readonly IOrderRepository _orderRepository;

        public TrustScoreService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        /// <summary>
        /// Mock implementation - returns a calculated trust score based on order history.
        /// In production, this would use a more sophisticated algorithm.
        /// </summary>
        public async Task<int> GetUserTrustScoreAsync(int userId)
        {
            // TODO: Replace this with actual trust score calculation from UserService/TrustService
            // For now, calculate a simple score based on completed orders

            var userOrders = await _orderRepository.GetUserOrderHistoryAsync(userId);
            var completedOrders = userOrders.Count(o => o.Status == "Completed");
            var cancelledOrders = userOrders.Count(o => o.Status == "Cancelled");
            var totalOrders = userOrders.Count();

            if (totalOrders == 0)
            {
                // New users start with medium trust score
                return 60;
            }

            // Calculate base score
            int baseScore = 50;

            // Add points for completed orders (up to +40)
            int completedBonus = Math.Min(completedOrders * 5, 40);

            // Subtract points for cancellations (up to -30)
            int cancelPenalty = Math.Min(cancelledOrders * 10, 30);

            // Calculate final score (clamped to 0-100)
            int finalScore = baseScore + completedBonus - cancelPenalty;
            finalScore = Math.Max(0, Math.Min(100, finalScore));

            return finalScore;
        }

        /// <summary>
        /// Calculates deposit percentage based on trust score tiers.
        /// </summary>
        public decimal CalculateDepositPercentage(int trustScore)
        {
            return trustScore switch
            {
                >= 70 => 0.30m,  // 30% deposit for trusted users
                >= 40 => 0.40m,  // 40% deposit for average users
                _ => 0.50m       // 50% deposit for low-trust users
            };
        }
    }
}
