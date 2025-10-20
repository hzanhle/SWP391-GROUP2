namespace BookingSerivce.Services
{
    /// <summary>
    /// Service interface for calculating and retrieving user trust scores.
    /// Trust score affects deposit percentage (30%, 40%, or 50%).
    /// </summary>
    public interface ITrustScoreService
    {
        /// <summary>
        /// Gets the current trust score for a user.
        /// Score range: 0-100 where higher is better.
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>Trust score (0-100)</returns>
        Task<int> GetUserTrustScoreAsync(int userId);

        /// <summary>
        /// Calculates the deposit percentage based on trust score.
        /// >= 70: 30% deposit
        /// 40-69: 40% deposit
        /// < 40: 50% deposit
        /// </summary>
        /// <param name="trustScore">The user's trust score</param>
        /// <returns>Deposit percentage as decimal (0.30, 0.40, or 0.50)</returns>
        decimal CalculateDepositPercentage(int trustScore);
    }
}
