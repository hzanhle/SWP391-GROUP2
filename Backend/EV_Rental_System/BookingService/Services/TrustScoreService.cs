using BookingService.Models;
using BookingService.Models.ModelSettings;
using BookingService.Repositories;
using Microsoft.Extensions.Options;
namespace BookingService.Services
{
    public class TrustScoreService : ITrustScoreService
    {
        // === ĐÂY LÀ NƠI QUẢN LÝ LOGIC NGHIỆP VỤ ===
        // Bạn có thể thay đổi các hằng số này để điều chỉnh hệ thống
        private const int INITIAL_SCORE = 100;         // Điểm khởi tạo khi user mới đăng ký
        private const int FIRST_ORDER_BONUS = 50;    // Điểm thưởng cho lần thanh toán đầu tiên
        private const int COMPLETION_BONUS = 10;     // Điểm thưởng khi hoàn thành chuyến đi
        private const int NOSHOW_PENALTY = -100;     // Điểm phạt khi không đến (NoShow)
                                                     // ===========================================

        private readonly ITrustScoreRepository _trustScoreRepo;
        private readonly ITrustScoreHistoryRepository _historyRepo;
        private readonly ILogger<TrustScoreService> _logger;
        private readonly BillingSettings _billingSettings;

        public TrustScoreService(
            ITrustScoreRepository trustScoreRepo,
            ITrustScoreHistoryRepository historyRepo,
            ILogger<TrustScoreService> logger,
            IOptions<BillingSettings> billingSettings)
        {
            _trustScoreRepo = trustScoreRepo;
            _historyRepo = historyRepo;
            _logger = logger;
            _billingSettings = billingSettings.Value;
        }

        /**
         * Hàm private tiện ích để lấy score hoặc tạo mới nếu chưa tồn tại.
         * Giúp các hàm public khác sạch sẽ hơn.
         */
        private async Task<TrustScore> GetOrCreateTrustScoreAsync(int userId, int orderId)
        {
            var trustScore = await _trustScoreRepo.GetByUserIdAsync(userId);

            if (trustScore == null)
            {
                _logger.LogInformation("Creating initial trust score {InitialScore} for new User {UserId}", INITIAL_SCORE, userId);
                var newScore = new TrustScore(INITIAL_SCORE, orderId)
                {
                    UserId = userId
                    // Constructor đã set CreatedAt = DateTime.UtcNow
                };

                // Hàm CreateAsync của repo chỉ "Add" vào DbContext
                // UoW ở OrderService sẽ Commit sau.
                await _trustScoreRepo.CreateAsync(newScore);

                // Track initial score creation in history
                await TrackScoreChangeAsync(userId, orderId, INITIAL_SCORE, 0, INITIAL_SCORE,
                    "Initial trust score", "Bonus", null);

                return newScore;
            }

            return trustScore;
        }

        /**
         * Track score changes in history table
         */
        private async Task TrackScoreChangeAsync(
            int userId,
            int? orderId,
            int changeAmount,
            int previousScore,
            int newScore,
            string reason,
            string changeType,
            int? adjustedByAdminId = null)
        {
            try
            {
                var history = new TrustScoreHistory
                {
                    UserId = userId,
                    OrderId = orderId,
                    ChangeAmount = changeAmount,
                    PreviousScore = previousScore,
                    NewScore = newScore,
                    Reason = reason,
                    ChangeType = changeType,
                    AdjustedByAdminId = adjustedByAdminId,
                    CreatedAt = DateTime.UtcNow
                };

                await _historyRepo.CreateAsync(history);
            }
            catch (Exception ex)
            {
                // Don't fail the main operation if history tracking fails
                _logger.LogError(ex, "Failed to track trust score history for User {UserId}", userId);
            }
        }

        public async Task<int> GetCurrentScoreAsync(int userId)
        {
            var trustScore = await _trustScoreRepo.GetByUserIdAsync(userId);
            // Trả về 0 nếu user chưa có điểm
            return trustScore?.Score ?? 0;
        }

        public async Task UpdateScoreOnFirstPaymentAsync(int userId, int orderId)
        {
            try
            {
                // Hàm này sẽ tự động tạo score (100) nếu chưa có
                var trustScore = await GetOrCreateTrustScoreAsync(userId, orderId);

                // Kiểm tra xem đây có phải là lần đầu (score = 100)
                if (trustScore.Score == INITIAL_SCORE)
                {
                    int previousScore = trustScore.Score;
                    trustScore.Score += FIRST_ORDER_BONUS;
                    trustScore.OrderId = orderId; // Cập nhật orderId liên quan cuối cùng
                    trustScore.CreatedAt = DateTime.UtcNow; // Cập nhật thời gian

                    // Repo chỉ "Update" vào DbContext
                    await _trustScoreRepo.UpdateScoreAsync(trustScore);

                    // Track history
                    await TrackScoreChangeAsync(userId, orderId, FIRST_ORDER_BONUS, previousScore, trustScore.Score,
                        "First payment bonus", "Bonus", null);

                    _logger.LogInformation("Added First Order Bonus (+{Bonus}) for User {UserId}. New score: {Score}", FIRST_ORDER_BONUS, userId, trustScore.Score);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating score on first payment for User {UserId}", userId);
                throw;
            }
        }

        public async Task UpdateScoreOnRentalCompletionAsync(int userId, int orderId)
        {
            try
            {
                var trustScore = await GetOrCreateTrustScoreAsync(userId, orderId);

                int previousScore = trustScore.Score;
                trustScore.Score += COMPLETION_BONUS;
                trustScore.OrderId = orderId;
                trustScore.CreatedAt = DateTime.UtcNow;

                await _trustScoreRepo.UpdateScoreAsync(trustScore);

                // Track history
                await TrackScoreChangeAsync(userId, orderId, COMPLETION_BONUS, previousScore, trustScore.Score,
                    "Rental completion bonus", "Bonus", null);

                _logger.LogInformation("Added Completion Bonus (+{Bonus}) for User {UserId}. New score: {Score}", COMPLETION_BONUS, userId, trustScore.Score);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating score on rental completion for User {UserId}", userId);
                throw;
            }
        }

        public async Task UpdateScoreOnNoShowAsync(int userId, int orderId)
        {
            try
            {
                var trustScore = await GetOrCreateTrustScoreAsync(userId, orderId);

                int previousScore = trustScore.Score;
                trustScore.Score += NOSHOW_PENALTY; // (NOSHOW_PENALTY là số âm)
                trustScore.OrderId = orderId;
                trustScore.CreatedAt = DateTime.UtcNow;

                await _trustScoreRepo.UpdateScoreAsync(trustScore);

                // Track history
                await TrackScoreChangeAsync(userId, orderId, NOSHOW_PENALTY, previousScore, trustScore.Score,
                    "No-show penalty", "Penalty", null);

                _logger.LogInformation("Applied No-Show Penalty ({Penalty}) for User {UserId}. New score: {Score}", NOSHOW_PENALTY, userId, trustScore.Score);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating score on no-show for User {UserId}", userId);
                throw;
            }
        }

        public async Task UpdateScoreOnLateReturnAsync(int userId, int orderId, decimal overtimeHours)
        {
            try
            {
                var trustScore = await GetOrCreateTrustScoreAsync(userId, orderId);

                int previousScore = trustScore.Score;
                // Calculate penalty: -5 points per hour late (rounded up)
                int penalty = -(int)Math.Ceiling(overtimeHours) * _billingSettings.LateReturnPenaltyPerHour;

                trustScore.Score += penalty;
                trustScore.OrderId = orderId;
                trustScore.CreatedAt = DateTime.UtcNow;

                await _trustScoreRepo.UpdateScoreAsync(trustScore);

                // Track history
                await TrackScoreChangeAsync(userId, orderId, penalty, previousScore, trustScore.Score,
                    $"Late return penalty ({overtimeHours:F2} hours late)", "Penalty", null);

                _logger.LogInformation(
                    "Applied Late Return Penalty ({Penalty}) for User {UserId}, Overtime: {OvertimeHours}h. New score: {Score}",
                    penalty, userId, overtimeHours, trustScore.Score);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating score on late return for User {UserId}", userId);
                throw;
            }
        }

        public async Task UpdateScoreOnDamageAsync(int userId, int orderId, decimal damageAmount)
        {
            try
            {
                var trustScore = await GetOrCreateTrustScoreAsync(userId, orderId);

                int previousScore = trustScore.Score;
                // Determine penalty based on damage amount
                int penalty = damageAmount >= _billingSettings.MajorDamageThreshold
                    ? -_billingSettings.MajorDamagePenalty  // Major damage: -30 points
                    : -_billingSettings.MinorDamagePenalty;  // Minor damage: -10 points

                string damageType = damageAmount >= _billingSettings.MajorDamageThreshold ? "Major" : "Minor";

                trustScore.Score += penalty;
                trustScore.OrderId = orderId;
                trustScore.CreatedAt = DateTime.UtcNow;

                await _trustScoreRepo.UpdateScoreAsync(trustScore);

                // Track history
                await TrackScoreChangeAsync(userId, orderId, penalty, previousScore, trustScore.Score,
                    $"{damageType} damage penalty ({damageAmount:N0} VND)", "Penalty", null);

                _logger.LogInformation(
                    "Applied Damage Penalty ({Penalty}) for User {UserId}, Damage: {DamageAmount} VND. New score: {Score}",
                    penalty, userId, damageAmount, trustScore.Score);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating score on damage for User {UserId}", userId);
                throw;
            }
        }

        public async Task<TrustScore?> GetFullTrustScoreAsync(int userId)
        {
            return await _trustScoreRepo.GetByUserIdAsync(userId);
        }

        public async Task<List<TrustScore>> GetTopScoresAsync()
        {
            // Repo của bạn không có tham số 'count',
            // nên service này cũng sẽ trả về tất cả.
            return await _trustScoreRepo.GetTopScoresAsync();
        }

        public async Task<double> GetAverageScoreAsync()
        {
            return await _trustScoreRepo.GetAverageScoreAsync();
        }

        public async Task ManuallyAdjustScoreAsync(int userId, int changeAmount, string reason, int adjustedByAdminId)
        {
            try
            {
                var trustScore = await GetOrCreateTrustScoreAsync(userId, 0); // orderId = 0 for manual adjustments

                int previousScore = trustScore.Score;
                trustScore.Score += changeAmount;
                trustScore.CreatedAt = DateTime.UtcNow;

                await _trustScoreRepo.UpdateScoreAsync(trustScore);

                // Track history with admin ID
                await TrackScoreChangeAsync(userId, null, changeAmount, previousScore, trustScore.Score,
                    $"Manual adjustment: {reason}", "ManualAdjustment", adjustedByAdminId);

                _logger.LogInformation(
                    "Admin {AdminId} manually adjusted score for User {UserId} by {ChangeAmount}. New score: {Score}. Reason: {Reason}",
                    adjustedByAdminId, userId, changeAmount, trustScore.Score, reason);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error manually adjusting score for User {UserId}", userId);
                throw;
            }
        }

        public async Task<List<TrustScoreHistory>> GetUserScoreHistoryAsync(int userId)
        {
            return await _historyRepo.GetByUserIdAsync(userId);
        }

        public async Task<List<TrustScoreHistory>> GetRecentUserScoreHistoryAsync(int userId, int count = 10)
        {
            return await _historyRepo.GetRecentByUserIdAsync(userId, count);
        }
    }
}
