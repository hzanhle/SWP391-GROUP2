using BookingService.DTOs.Fees;
using BookingService.Models;
using BookingService.Models.Enums;
using BookingService.Repositories;
using Microsoft.Extensions.Options;

namespace BookingService.Services
{
    public class FeeCalculationService : IFeeCalculationService
    {
        private readonly IAdditionalFeeRepository _feeRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IInspectionRepository _inspectionRepository;
        private readonly FeeSettings _feeSettings;

        public FeeCalculationService(
            IAdditionalFeeRepository feeRepository,
            IOrderRepository orderRepository,
            IInspectionRepository inspectionRepository,
            IOptions<FeeSettings> feeSettings)
        {
            _feeRepository = feeRepository;
            _orderRepository = orderRepository;
            _inspectionRepository = inspectionRepository;
            _feeSettings = feeSettings.Value;
        }

        public async Task<FeeCalculationResponse> CalculateFeesAsync(FeeCalculationRequest request)
        {
            var response = new FeeCalculationResponse
            {
                OrderId = request.OrderId
            };

            // Get order details
            var order = await _orderRepository.GetByIdAsync(request.OrderId);
            if (order == null)
            {
                response.Message = "Order not found";
                return response;
            }

            var calculatedFees = new List<AdditionalFee>();

            // 1. Calculate Late Return Fee
            if (request.ActualReturnDate > order.ToDate)
            {
                var lateReturnFee = CalculateLateReturnFee(order, request.ActualReturnDate);
                if (lateReturnFee != null)
                {
                    calculatedFees.Add(lateReturnFee);
                    response.CalculationDetails["LateReturn"] =
                        $"Late by {(request.ActualReturnDate - order.ToDate).TotalHours:F1} hours (Grace period: {_feeSettings.LateReturnGracePeriodMinutes} minutes)";
                }
            }

            // 2. Calculate Damage Fees from Return Inspection
            var returnInspection = await _inspectionRepository.GetInspectionByOrderAndTypeAsync(
                request.OrderId, InspectionType.Return);

            if (returnInspection != null && returnInspection.Damages.Any())
            {
                var damageFees = CalculateDamageFees(order, returnInspection);
                calculatedFees.AddRange(damageFees);
                response.CalculationDetails["Damage"] =
                    $"Found {returnInspection.Damages.Count} damage(s) during return inspection";
            }

            // 3. Calculate Excess Mileage Fee
            if (request.ReturnMileage.HasValue && request.PickupMileage.HasValue)
            {
                var excessMileageFee = CalculateExcessMileageFee(order, request.PickupMileage.Value, request.ReturnMileage.Value);
                if (excessMileageFee != null)
                {
                    calculatedFees.Add(excessMileageFee);
                    var totalKm = request.ReturnMileage.Value - request.PickupMileage.Value;
                    var rentalDays = (order.ToDate - order.FromDate).Days + 1;
                    var includedKm = rentalDays * _feeSettings.IncludedKmPerDay;
                    response.CalculationDetails["ExcessMileage"] =
                        $"Traveled {totalKm} km, included {includedKm} km";
                }
            }

            // Save all calculated fees to database
            foreach (var fee in calculatedFees)
            {
                await _feeRepository.AddAsync(fee);
                response.CalculatedFees.Add(MapToDto(fee));
            }

            response.TotalFees = calculatedFees.Sum(f => f.Amount);
            response.Message = calculatedFees.Any()
                ? $"Successfully calculated {calculatedFees.Count} fee(s)"
                : "No additional fees calculated";

            return response;
        }

        private AdditionalFee? CalculateLateReturnFee(Order order, DateTime actualReturnDate)
        {
            var lateMinutes = (actualReturnDate - order.ToDate).TotalMinutes;

            // Check if within grace period
            if (lateMinutes <= _feeSettings.LateReturnGracePeriodMinutes)
                return null;

            // Calculate late hours beyond grace period
            var chargeableMinutes = lateMinutes - _feeSettings.LateReturnGracePeriodMinutes;
            var chargeableHours = Math.Ceiling(chargeableMinutes / 60);

            // Calculate hourly rate from daily rate
            var dailyRate = order.TotalPrice / ((order.ToDate - order.FromDate).Days + 1);
            var hourlyRate = dailyRate / 24;

            var lateReturnAmount = hourlyRate * (decimal)chargeableHours * _feeSettings.LateReturnPenaltyMultiplier;

            return new AdditionalFee
            {
                OrderId = order.OrderId,
                FeeType = FeeType.LateReturn,
                Amount = Math.Round(lateReturnAmount, 2),
                Description = $"Late return by {chargeableHours} hour(s) (Grace period: {_feeSettings.LateReturnGracePeriodMinutes} min)",
                CreatedAt = DateTime.UtcNow
            };
        }

        private List<AdditionalFee> CalculateDamageFees(Order order, VehicleInspection returnInspection)
        {
            var damageFees = new List<AdditionalFee>();

            foreach (var damage in returnInspection.Damages)
            {
                // Use estimated cost from inspection if available
                decimal baseCost = damage.EstimatedCost ?? 0;

                // If no estimated cost, use a percentage of daily rate based on severity
                if (baseCost == 0)
                {
                    var dailyRate = order.TotalPrice / ((order.ToDate - order.FromDate).Days + 1);
                    baseCost = damage.Severity switch
                    {
                        DamageSeverity.Minor => dailyRate * 0.1m,
                        DamageSeverity.Moderate => dailyRate * 0.3m,
                        DamageSeverity.Major => dailyRate * 0.5m,
                        _ => 0
                    };
                }

                // Apply severity multiplier
                var multiplier = damage.Severity switch
                {
                    DamageSeverity.Minor => _feeSettings.DamageMinorMultiplier,
                    DamageSeverity.Moderate => _feeSettings.DamageModerateMultiplier,
                    DamageSeverity.Major => _feeSettings.DamageMajorMultiplier,
                    _ => 1.0m
                };

                var feeAmount = baseCost * multiplier;

                damageFees.Add(new AdditionalFee
                {
                    OrderId = order.OrderId,
                    FeeType = FeeType.Damage,
                    Amount = Math.Round(feeAmount, 2),
                    Description = $"{damage.Severity} damage: {damage.DamageType} at {damage.Location}",
                    CreatedAt = DateTime.UtcNow
                });
            }

            return damageFees;
        }

        private AdditionalFee? CalculateExcessMileageFee(Order order, int pickupMileage, int returnMileage)
        {
            var totalKm = returnMileage - pickupMileage;
            var rentalDays = (order.ToDate - order.FromDate).Days + 1;
            var includedKm = rentalDays * _feeSettings.IncludedKmPerDay;
            var excessKm = totalKm - includedKm;

            if (excessKm <= 0)
                return null;

            var excessMileageAmount = excessKm * _feeSettings.ExcessMileageFeePerKm;

            return new AdditionalFee
            {
                OrderId = order.OrderId,
                FeeType = FeeType.ExcessMileage,
                Amount = Math.Round(excessMileageAmount, 2),
                Description = $"Excess mileage: {excessKm} km over {includedKm} km included ({totalKm} km total)",
                CreatedAt = DateTime.UtcNow
            };
        }

        public async Task<FeeDto> AddFeeAsync(AddFeeRequest request)
        {
            var fee = new AdditionalFee
            {
                OrderId = request.OrderId,
                FeeType = request.FeeType,
                Amount = request.Amount,
                Description = request.Description,
                CalculatedBy = request.CalculatedBy,
                CreatedAt = DateTime.UtcNow
            };

            var addedFee = await _feeRepository.AddAsync(fee);
            return MapToDto(addedFee);
        }

        public async Task<List<FeeDto>> GetFeesByOrderIdAsync(int orderId)
        {
            var fees = await _feeRepository.GetByOrderIdAsync(orderId);
            return fees.Select(MapToDto).ToList();
        }

        public async Task<FeeDto?> GetFeeByIdAsync(int feeId)
        {
            var fee = await _feeRepository.GetByIdAsync(feeId);
            return fee != null ? MapToDto(fee) : null;
        }

        public async Task<bool> MarkFeeAsPaidAsync(int feeId)
        {
            return await _feeRepository.MarkAsPaidAsync(feeId);
        }

        public async Task<bool> DeleteFeeAsync(int feeId)
        {
            var fee = await _feeRepository.GetByIdAsync(feeId);
            if (fee == null || fee.IsPaid)
                return false;

            return await _feeRepository.DeleteAsync(feeId);
        }

        public async Task<decimal> GetTotalFeesAsync(int orderId)
        {
            return await _feeRepository.GetTotalFeesByOrderIdAsync(orderId);
        }

        private FeeDto MapToDto(AdditionalFee fee)
        {
            return new FeeDto
            {
                FeeId = fee.FeeId,
                OrderId = fee.OrderId,
                FeeType = fee.FeeType,
                FeeTypeName = fee.FeeType.ToString(),
                Amount = fee.Amount,
                Description = fee.Description,
                CalculatedBy = fee.CalculatedBy,
                IsPaid = fee.IsPaid,
                CreatedAt = fee.CreatedAt
            };
        }
    }
}
