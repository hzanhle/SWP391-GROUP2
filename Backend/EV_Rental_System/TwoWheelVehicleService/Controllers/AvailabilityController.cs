using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwoWheelVehicleService.DTOs;
using TwoWheelVehicleService.Services;

namespace TwoWheelVehicleService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AvailabilityController : ControllerBase
    {
        private readonly IAvailabilityService _availabilityService;
        private readonly ILogger<AvailabilityController> _logger;

        public AvailabilityController(
            IAvailabilityService availabilityService,
            ILogger<AvailabilityController> logger)
        {
            _availabilityService = availabilityService;
            _logger = logger;
        }

        /// <summary>
        /// Check if a specific vehicle is available for a date range
        /// GET: /api/availability/check?vehicleId=1&fromDate=2025-01-01&toDate=2025-01-05
        /// </summary>
        [HttpGet("check")]
        [AllowAnonymous] // Public endpoint for customers to check availability
        public async Task<IActionResult> CheckVehicleAvailability(
            [FromQuery] int vehicleId,
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            [FromQuery] int? excludeOrderId = null)
        {
            try
            {
                if (vehicleId <= 0)
                {
                    return BadRequest(new ResponseDTO
                    {
                        Message = "Invalid vehicle ID"
                    });
                }

                var request = new AvailabilityCheckRequest
                {
                    VehicleId = vehicleId,
                    FromDate = fromDate,
                    ToDate = toDate,
                    ExcludeOrderId = excludeOrderId
                };

                var result = await _availabilityService.CheckVehicleAvailabilityAsync(request);

                if (result.IsAvailable)
                {
                    return Ok(new
                    {
                        success = true,
                        data = result
                    });
                }
                else
                {
                    return Ok(new
                    {
                        success = false,
                        data = result
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking vehicle availability");
                return StatusCode(500, new ResponseDTO
                {
                    Message = "Error checking availability"
                });
            }
        }

        /// <summary>
        /// Get all available vehicles for a specific station and date range
        /// GET: /api/availability/station/1?fromDate=2025-01-01&toDate=2025-01-05
        /// </summary>
        [HttpGet("station/{stationId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableVehiclesByStation(
            int stationId,
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate)
        {
            try
            {
                if (stationId <= 0)
                {
                    return BadRequest(new ResponseDTO
                    {
                        Message = "Invalid station ID"
                    });
                }

                var vehicles = await _availabilityService.GetAvailableVehiclesByStationAsync(
                    stationId,
                    fromDate,
                    toDate);

                return Ok(new
                {
                    success = true,
                    count = vehicles.Count,
                    data = vehicles
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available vehicles for station {StationId}", stationId);
                return StatusCode(500, new ResponseDTO
                {
                    Message = "Error getting available vehicles"
                });
            }
        }

        /// <summary>
        /// Get all available vehicles (any station) for a date range
        /// GET: /api/availability/all?fromDate=2025-01-01&toDate=2025-01-05
        /// </summary>
        [HttpGet("all")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllAvailableVehicles(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate)
        {
            try
            {
                var vehicles = await _availabilityService.GetAllAvailableVehiclesAsync(
                    fromDate,
                    toDate);

                return Ok(new
                {
                    success = true,
                    count = vehicles.Count,
                    data = vehicles
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all available vehicles");
                return StatusCode(500, new ResponseDTO
                {
                    Message = "Error getting available vehicles"
                });
            }
        }

        /// <summary>
        /// Bulk check availability for multiple vehicles
        /// POST: /api/availability/bulk-check
        /// Body: { "vehicleIds": [1, 2, 3], "fromDate": "2025-01-01", "toDate": "2025-01-05" }
        /// </summary>
        [HttpPost("bulk-check")]
        [AllowAnonymous]
        public async Task<IActionResult> BulkCheckAvailability([FromBody] BulkAvailabilityRequest request)
        {
            try
            {
                if (request.VehicleIds == null || !request.VehicleIds.Any())
                {
                    return BadRequest(new ResponseDTO
                    {
                        Message = "Vehicle IDs are required"
                    });
                }

                var result = await _availabilityService.BulkCheckAvailabilityAsync(
                    request.VehicleIds,
                    request.FromDate,
                    request.ToDate);

                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk availability check");
                return StatusCode(500, new ResponseDTO
                {
                    Message = "Error checking availability"
                });
            }
        }
    }

    /// <summary>
    /// Request DTO for bulk availability check
    /// </summary>
    public class BulkAvailabilityRequest
    {
        public List<int> VehicleIds { get; set; } = new();
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
