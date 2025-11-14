using BookingService.DTOs;
using BookingService.Models;
using BookingService.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookingService.Services
{
    public class RiskCustomerService : IRiskCustomerService
    {
        private readonly ITrustScoreRepository _trustScoreRepo;
        private readonly ITrustScoreHistoryRepository _trustScoreHistoryRepo;
        private readonly IOrderRepository _orderRepo;
        private readonly ISettlementRepository _settlementRepo;
        private readonly IVehicleReturnRepository _vehicleReturnRepo;
        private readonly ILogger<RiskCustomerService> _logger;

        public RiskCustomerService(
            ITrustScoreRepository trustScoreRepo,
            ITrustScoreHistoryRepository trustScoreHistoryRepo,
            IOrderRepository orderRepo,
            ISettlementRepository settlementRepo,
            IVehicleReturnRepository vehicleReturnRepo,
            ILogger<RiskCustomerService> logger)
        {
            _trustScoreRepo = trustScoreRepo;
            _trustScoreHistoryRepo = trustScoreHistoryRepo;
            _orderRepo = orderRepo;
            _settlementRepo = settlementRepo;
            _vehicleReturnRepo = vehicleReturnRepo;
            _logger = logger;
        }

        public async Task<List<RiskCustomerDTO>> GetRiskCustomersAsync(string? riskLevel = null, int? minRiskScore = null)
        {
            _logger.LogInformation("Getting risk customers with filters: RiskLevel={RiskLevel}, MinRiskScore={MinRiskScore}", riskLevel, minRiskScore);

            // Get all users with trust scores
            var allTrustScores = await _trustScoreRepo.GetScoresByRangeAsync(0, 200); // Get all scores

            var riskCustomers = new List<RiskCustomerDTO>();

            foreach (var trustScore in allTrustScores)
            {
                var riskCustomer = await CalculateUserRiskAsync(trustScore.UserId);
                if (riskCustomer != null)
                {
                    // Apply filters
                    if (!string.IsNullOrEmpty(riskLevel) && riskCustomer.RiskLevel != riskLevel)
                        continue;

                    if (minRiskScore.HasValue && riskCustomer.RiskScore < minRiskScore.Value)
                        continue;

                    riskCustomers.Add(riskCustomer);
                }
            }

            // Sort by risk score descending (highest risk first)
            return riskCustomers.OrderByDescending(r => r.RiskScore).ToList();
        }

        public async Task<UserRiskProfileDTO?> GetUserRiskProfileAsync(int userId)
        {
            _logger.LogInformation("Getting detailed risk profile for User {UserId}", userId);

            var baseRisk = await CalculateUserRiskAsync(userId);
            if (baseRisk == null)
                return null;

            var profile = new UserRiskProfileDTO
            {
                UserId = baseRisk.UserId,
                UserName = baseRisk.UserName,
                Email = baseRisk.Email,
                TrustScore = baseRisk.TrustScore,
                RiskScore = baseRisk.RiskScore,
                RiskLevel = baseRisk.RiskLevel,
                RiskFactors = baseRisk.RiskFactors,
                TotalOrders = baseRisk.TotalOrders,
                CompletedOrders = baseRisk.CompletedOrders,
                CancelledOrders = baseRisk.CancelledOrders,
                LateReturnsCount = baseRisk.LateReturnsCount,
                DamageCount = baseRisk.DamageCount,
                TotalDamageAmount = baseRisk.TotalDamageAmount,
                NoShowCount = baseRisk.NoShowCount,
                PenaltyCount = baseRisk.PenaltyCount,
                LastOrderDate = baseRisk.LastOrderDate,
                LastViolationDate = baseRisk.LastViolationDate
            };

            // Get detailed violations
            profile.Violations = await GetViolationsAsync(userId);

            // Get recent orders with issues
            profile.RecentOrders = await GetRecentOrdersWithIssuesAsync(userId);

            // Get recent trust score changes
            profile.RecentScoreChanges = await GetRecentScoreChangesAsync(userId);

            return profile;
        }

        public async Task<RiskCustomerDTO?> CalculateUserRiskAsync(int userId)
        {
            try
            {
                // Get trust score
                var trustScore = await _trustScoreRepo.GetByUserIdAsync(userId);
                if (trustScore == null)
                {
                    // User has no trust score yet - consider as low risk
                    return null;
                }

                // Get all orders for this user
                var orders = (await _orderRepo.GetByUserIdAsync(userId)).ToList();
                var totalOrders = orders.Count;
                var completedOrders = orders.Count(o => o.Status == OrderStatus.Completed);
                var cancelledOrders = orders.Count(o => o.Status == OrderStatus.Cancelled);
                var lastOrderDate = orders.OrderByDescending(o => o.CreatedAt).FirstOrDefault()?.CreatedAt;

                // Get trust score history (penalties)
                var history = await _trustScoreHistoryRepo.GetByUserIdAsync(userId);
                var penalties = history.Where(h => h.ChangeType == "Penalty" && h.ChangeAmount < 0).ToList();
                var penaltyCount = penalties.Count;
                var lastViolationDate = penalties.OrderByDescending(p => p.CreatedAt).FirstOrDefault()?.CreatedAt;

                // Count violations from history
                var lateReturnCount = penalties.Count(p => p.Reason.Contains("Late return", StringComparison.OrdinalIgnoreCase));
                var damageCount = penalties.Count(p => p.Reason.Contains("Damage", StringComparison.OrdinalIgnoreCase));
                var noShowCount = penalties.Count(p => p.Reason.Contains("No-Show", StringComparison.OrdinalIgnoreCase) || p.Reason.Contains("NoShow", StringComparison.OrdinalIgnoreCase));

                // Get settlements to calculate damage amounts
                var settlements = new List<Settlement>();
                foreach (var order in orders.Where(o => o.Status == OrderStatus.Completed))
                {
                    var settlement = await _settlementRepo.GetByOrderIdAsync(order.OrderId);
                    if (settlement != null)
                        settlements.Add(settlement);
                }

                var totalDamageAmount = settlements.Sum(s => s.DamageCharge);
                var lateReturnsFromSettlements = settlements.Count(s => s.OvertimeHours > 0);

                // Use settlement data if available (more accurate)
                if (lateReturnsFromSettlements > 0)
                    lateReturnCount = lateReturnsFromSettlements;

                // Calculate risk score
                var riskScore = 0;
                var riskFactors = new List<string>();

                // 1. Trust Score Impact (inverse relationship)
                if (trustScore.Score < 30)
                {
                    riskScore += 50;
                    riskFactors.Add("Very Low Trust Score (<30)");
                }
                else if (trustScore.Score < 50)
                {
                    riskScore += 30;
                    riskFactors.Add("Low Trust Score (<50)");
                }
                else if (trustScore.Score < 70)
                {
                    riskScore += 10;
                    riskFactors.Add("Below Average Trust Score (<70)");
                }

                // 2. Late Returns
                if (lateReturnCount > 3)
                {
                    riskScore += 15;
                    riskFactors.Add($"Multiple Late Returns ({lateReturnCount} times)");
                }
                else if (lateReturnCount > 0)
                {
                    riskScore += 5 * lateReturnCount;
                    riskFactors.Add($"{lateReturnCount} Late Return(s)");
                }

                // 3. Damages
                if (damageCount > 2)
                {
                    riskScore += 20;
                    riskFactors.Add($"Multiple Damages ({damageCount} times)");
                }
                else if (damageCount > 0)
                {
                    riskScore += 10 * damageCount;
                    riskFactors.Add($"{damageCount} Damage(s)");
                }

                // Major damage check
                if (totalDamageAmount >= 1000000) // >= 1M VND
                {
                    riskScore += 20;
                    riskFactors.Add($"Major Damage (≥1M VND: {totalDamageAmount:N0} VND)");
                }
                else if (totalDamageAmount > 0)
                {
                    riskScore += 5;
                    riskFactors.Add($"Total Damage Amount: {totalDamageAmount:N0} VND");
                }

                // 4. No-Shows
                if (noShowCount > 0)
                {
                    riskScore += 15 * noShowCount;
                    riskFactors.Add($"{noShowCount} No-Show(s)");
                }

                // 5. Cancellation Rate
                if (totalOrders > 0)
                {
                    var cancelRate = (double)cancelledOrders / totalOrders;
                    if (cancelRate > 0.5)
                    {
                        riskScore += 10;
                        riskFactors.Add($"High Cancellation Rate ({cancelRate:P0})");
                    }
                    else if (cancelRate > 0.3)
                    {
                        riskScore += 5;
                        riskFactors.Add($"Moderate Cancellation Rate ({cancelRate:P0})");
                    }
                }

                // 6. Overall penalty count
                if (penaltyCount > 5)
                {
                    riskScore += 10;
                    riskFactors.Add($"Multiple Penalties ({penaltyCount} total)");
                }

                // Determine Risk Level
                string riskLevel;
                if (riskScore >= 70)
                    riskLevel = "Critical";
                else if (riskScore >= 50)
                    riskLevel = "High";
                else if (riskScore >= 30)
                    riskLevel = "Medium";
                else
                    riskLevel = "Low";

                // Only return if risk level is Medium or higher (or if explicitly requested)
                // For now, return all users but filter can be applied in GetRiskCustomersAsync

                return new RiskCustomerDTO
                {
                    UserId = userId,
                    TrustScore = trustScore.Score,
                    RiskScore = riskScore,
                    RiskLevel = riskLevel,
                    RiskFactors = riskFactors,
                    TotalOrders = totalOrders,
                    CompletedOrders = completedOrders,
                    CancelledOrders = cancelledOrders,
                    LateReturnsCount = lateReturnCount,
                    DamageCount = damageCount,
                    TotalDamageAmount = totalDamageAmount,
                    NoShowCount = noShowCount,
                    PenaltyCount = penaltyCount,
                    LastOrderDate = lastOrderDate,
                    LastViolationDate = lastViolationDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating risk for User {UserId}", userId);
                return null;
            }
        }

        private async Task<List<ViolationDetailDTO>> GetViolationsAsync(int userId)
        {
            var violations = new List<ViolationDetailDTO>();

            // Get penalty history
            var history = await _trustScoreHistoryRepo.GetByUserIdAsync(userId);
            var penalties = history.Where(h => h.ChangeType == "Penalty" && h.ChangeAmount < 0).ToList();

            foreach (var penalty in penalties)
            {
                string violationType = "Other";
                if (penalty.Reason.Contains("Late return", StringComparison.OrdinalIgnoreCase))
                    violationType = "LateReturn";
                else if (penalty.Reason.Contains("Damage", StringComparison.OrdinalIgnoreCase))
                    violationType = "Damage";
                else if (penalty.Reason.Contains("No-Show", StringComparison.OrdinalIgnoreCase) || penalty.Reason.Contains("NoShow", StringComparison.OrdinalIgnoreCase))
                    violationType = "NoShow";

                violations.Add(new ViolationDetailDTO
                {
                    OrderId = penalty.OrderId ?? 0,
                    ViolationType = violationType,
                    Description = penalty.Reason,
                    Amount = null, // Will be filled from settlement if available
                    ViolationDate = penalty.CreatedAt
                });
            }

            // Enrich with settlement data for amounts
            foreach (var violation in violations.Where(v => v.OrderId > 0))
            {
                var settlement = await _settlementRepo.GetByOrderIdAsync(violation.OrderId);
                if (settlement != null)
                {
                    if (violation.ViolationType == "LateReturn")
                        violation.Amount = settlement.OvertimeFee;
                    else if (violation.ViolationType == "Damage")
                        violation.Amount = settlement.DamageCharge;
                }
            }

            return violations.OrderByDescending(v => v.ViolationDate).ToList();
        }

        private async Task<List<OrderRiskInfoDTO>> GetRecentOrdersWithIssuesAsync(int userId, int limit = 10)
        {
            var orders = (await _orderRepo.GetByUserIdAsync(userId))
                .Where(o => o.Status == OrderStatus.Completed || o.Status == OrderStatus.Cancelled)
                .OrderByDescending(o => o.CreatedAt)
                .Take(limit)
                .ToList();

            var orderInfos = new List<OrderRiskInfoDTO>();

            foreach (var order in orders)
            {
                var settlement = await _settlementRepo.GetByOrderIdAsync(order.OrderId);
                var vehicleReturn = await _vehicleReturnRepo.GetByOrderIdAsync(order.OrderId);

                orderInfos.Add(new OrderRiskInfoDTO
                {
                    OrderId = order.OrderId,
                    Status = order.Status.ToString(),
                    FromDate = order.FromDate,
                    ToDate = order.ToDate,
                    HasLateReturn = settlement?.OvertimeHours > 0,
                    HasDamage = vehicleReturn?.HasDamage ?? false || settlement?.DamageCharge > 0,
                    DamageCharge = settlement?.DamageCharge ?? vehicleReturn?.DamageCharge ?? 0,
                    OvertimeHours = settlement?.OvertimeHours ?? 0,
                    CreatedAt = order.CreatedAt
                });
            }

            return orderInfos;
        }

        private async Task<List<TrustScoreChangeDTO>> GetRecentScoreChangesAsync(int userId, int limit = 10)
        {
            var history = await _trustScoreHistoryRepo.GetRecentByUserIdAsync(userId, limit);

            return history.Select(h => new TrustScoreChangeDTO
            {
                ChangeAmount = h.ChangeAmount,
                ChangeType = h.ChangeType,
                Reason = h.Reason,
                CreatedAt = h.CreatedAt,
                OrderId = h.OrderId
            }).ToList();
        }
    }
}

