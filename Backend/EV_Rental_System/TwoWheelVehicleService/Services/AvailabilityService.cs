using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TwoWheelVehicleService.DTOs;
using TwoWheelVehicleService.Repositories;

namespace TwoWheelVehicleService.Services
{
    public class AvailabilityService : IAvailabilityService
    {
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IModelRepository _modelRepository;
        private readonly HttpClient _httpClient;
        private readonly ILogger<AvailabilityService> _logger;
        private readonly string _bookingServiceUrl;

        public AvailabilityService(
            IVehicleRepository vehicleRepository,
            IModelRepository modelRepository,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<AvailabilityService> logger)
        {
            _vehicleRepository = vehicleRepository;
            _modelRepository = modelRepository;
            _httpClient = httpClientFactory.CreateClient("BookingService");
            _logger = logger;
            _bookingServiceUrl = configuration["ServiceUrls:BookingService"]
                ?? throw new InvalidOperationException("BookingService URL not configured");
        }

        /// <summary>
        /// Check if a specific vehicle is available for the given date range
        /// </summary>
        public async Task<AvailabilityCheckResponse> CheckVehicleAvailabilityAsync(AvailabilityCheckRequest request)
        {
            try
            {
                // Validate dates
                if (request.ToDate <= request.FromDate)
                {
                    return new AvailabilityCheckResponse
                    {
                        VehicleId = request.VehicleId,
                        IsAvailable = false,
                        Message = "End date must be after start date",
                        FromDate = request.FromDate,
                        ToDate = request.ToDate
                    };
                }

                // Check if vehicle exists and is active
                var vehicle = await _vehicleRepository.GetVehicleById(request.VehicleId);
                if (vehicle == null)
                {
                    return new AvailabilityCheckResponse
                    {
                        VehicleId = request.VehicleId,
                        IsAvailable = false,
                        Message = "Vehicle not found",
                        FromDate = request.FromDate,
                        ToDate = request.ToDate
                    };
                }

                if (!vehicle.IsActive || vehicle.Status != "Available")
                {
                    return new AvailabilityCheckResponse
                    {
                        VehicleId = request.VehicleId,
                        IsAvailable = false,
                        Message = $"Vehicle is not available. Status: {vehicle.Status}",
                        FromDate = request.FromDate,
                        ToDate = request.ToDate
                    };
                }

                // Call BookingService to check for conflicting orders
                var conflictingOrders = await GetConflictingOrdersAsync(
                    request.VehicleId,
                    request.FromDate,
                    request.ToDate,
                    request.ExcludeOrderId);

                var isAvailable = !conflictingOrders.Any();

                return new AvailabilityCheckResponse
                {
                    VehicleId = request.VehicleId,
                    IsAvailable = isAvailable,
                    Message = isAvailable
                        ? "Vehicle is available for the selected dates"
                        : $"Vehicle is already booked for {conflictingOrders.Count} order(s) in this period",
                    FromDate = request.FromDate,
                    ToDate = request.ToDate,
                    ConflictingOrders = conflictingOrders
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking vehicle availability for VehicleId: {VehicleId}", request.VehicleId);
                return new AvailabilityCheckResponse
                {
                    VehicleId = request.VehicleId,
                    IsAvailable = false,
                    Message = "Error checking availability. Please try again later.",
                    FromDate = request.FromDate,
                    ToDate = request.ToDate
                };
            }
        }

        /// <summary>
        /// Get all available vehicles for a specific station and date range
        /// </summary>
        public async Task<List<VehicleAvailabilityDto>> GetAvailableVehiclesByStationAsync(int stationId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var allVehicles = await _vehicleRepository.GetAllVehicles();
                var stationVehicles = allVehicles.Where(v => v.StationId == stationId && v.IsActive).ToList();

                var result = new List<VehicleAvailabilityDto>();

                foreach (var vehicle in stationVehicles)
                {
                    var model = await _modelRepository.GetModelById(vehicle.ModelId);
                    if (model == null) continue;

                    var checkRequest = new AvailabilityCheckRequest
                    {
                        VehicleId = vehicle.VehicleId,
                        FromDate = fromDate,
                        ToDate = toDate
                    };

                    var availabilityCheck = await CheckVehicleAvailabilityAsync(checkRequest);

                    if (availabilityCheck.IsAvailable)
                    {
                        result.Add(new VehicleAvailabilityDto
                        {
                            VehicleId = vehicle.VehicleId,
                            ModelId = vehicle.ModelId,
                            ModelName = model.ModelName,
                            Manufacturer = model.Manufacturer,
                            Color = vehicle.Color,
                            StationId = vehicle.StationId,
                            IsActive = vehicle.IsActive,
                            Status = vehicle.Status,
                            IsAvailableForDates = true,
                            RentFeeForHour = model.RentFeeForHour,
                            ModelCost = model.ModelCost
                        });
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available vehicles for station {StationId}", stationId);
                return new List<VehicleAvailabilityDto>();
            }
        }

        /// <summary>
        /// Get all available vehicles (any station) for a date range
        /// </summary>
        public async Task<List<VehicleAvailabilityDto>> GetAllAvailableVehiclesAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var allVehicles = await _vehicleRepository.GetActiveVehicles();
                var result = new List<VehicleAvailabilityDto>();

                foreach (var vehicle in allVehicles)
                {
                    var model = await _modelRepository.GetModelById(vehicle.ModelId);
                    if (model == null) continue;

                    var checkRequest = new AvailabilityCheckRequest
                    {
                        VehicleId = vehicle.VehicleId,
                        FromDate = fromDate,
                        ToDate = toDate
                    };

                    var availabilityCheck = await CheckVehicleAvailabilityAsync(checkRequest);

                    if (availabilityCheck.IsAvailable)
                    {
                        result.Add(new VehicleAvailabilityDto
                        {
                            VehicleId = vehicle.VehicleId,
                            ModelId = vehicle.ModelId,
                            ModelName = model.ModelName,
                            Manufacturer = model.Manufacturer,
                            Color = vehicle.Color,
                            StationId = vehicle.StationId,
                            IsActive = vehicle.IsActive,
                            Status = vehicle.Status,
                            IsAvailableForDates = true,
                            RentFeeForHour = model.RentFeeForHour,
                            ModelCost = model.ModelCost
                        });
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all available vehicles");
                return new List<VehicleAvailabilityDto>();
            }
        }

        /// <summary>
        /// Bulk check availability for multiple vehicles
        /// </summary>
        public async Task<Dictionary<int, bool>> BulkCheckAvailabilityAsync(List<int> vehicleIds, DateTime fromDate, DateTime toDate)
        {
            var result = new Dictionary<int, bool>();

            try
            {
                foreach (var vehicleId in vehicleIds)
                {
                    var checkRequest = new AvailabilityCheckRequest
                    {
                        VehicleId = vehicleId,
                        FromDate = fromDate,
                        ToDate = toDate
                    };

                    var availabilityCheck = await CheckVehicleAvailabilityAsync(checkRequest);
                    result[vehicleId] = availabilityCheck.IsAvailable;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk availability check");
                return result;
            }
        }

        /// <summary>
        /// Call BookingService to get conflicting orders for a vehicle in a date range
        /// </summary>
        private async Task<List<ConflictingOrderDto>> GetConflictingOrdersAsync(
            int vehicleId,
            DateTime fromDate,
            DateTime toDate,
            int? excludeOrderId = null)
        {
            try
            {
                var url = $"{_bookingServiceUrl}/api/orders/check-conflicts?vehicleId={vehicleId}" +
                         $"&fromDate={fromDate:yyyy-MM-ddTHH:mm:ss}&toDate={toDate:yyyy-MM-ddTHH:mm:ss}";

                if (excludeOrderId.HasValue)
                {
                    url += $"&excludeOrderId={excludeOrderId.Value}";
                }

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var orders = JsonSerializer.Deserialize<List<ConflictingOrderDto>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return orders ?? new List<ConflictingOrderDto>();
                }

                // If endpoint doesn't exist yet or returns error, assume no conflicts (graceful degradation)
                _logger.LogWarning("BookingService returned {StatusCode} when checking conflicts", response.StatusCode);
                return new List<ConflictingOrderDto>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Could not connect to BookingService. Assuming no conflicts.");
                return new List<ConflictingOrderDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling BookingService to check conflicts");
                return new List<ConflictingOrderDto>();
            }
        }
    }
}
