using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwoWheelVehicleService.DTOs;
using TwoWheelVehicleService.Services;

namespace TwoWheelVehicleService.Controllers
{
    [ApiController]
    [Route("api/staff/vehicles")]
    [Authorize(Roles = "Staff,Admin")]
    public class StaffVehicleController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;
        private readonly ILogger<StaffVehicleController> _logger;

        public StaffVehicleController(
            IVehicleService vehicleService,
            ILogger<StaffVehicleController> logger)
        {
            _vehicleService = vehicleService;
            _logger = logger;
        }

        // Lấy danh sách xe với filter
        [HttpGet]
        public async Task<ActionResult<ResponseDTO>> GetVehicles(
            [FromQuery] string? status,
            [FromQuery] string? battery,
            [FromQuery] string? search)
        {
            try
            {
                int? stationId = null; 

                _logger.LogInformation($"Fetching vehicles with filters: status={status}, battery={battery}, search={search}");

                var vehicles = await _vehicleService.GetVehiclesWithStatusAsync(
                    status, battery, search, stationId);

                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = $"Retrieved {vehicles.Count} vehicles IsSuccessfully",
                    Data = vehicles
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching vehicles");
                return StatusCode(500, new ResponseDTO
                {
                    IsSuccess = false,
                    Message = "Failed to fetch vehicles: " + ex.Message,
                    Data = null
                });
            }
        }
        // Cập nhật trạng thái xe
        [HttpPut("{vehicleId}/status")]
        public async Task<ActionResult<ResponseDTO>> UpdateVehicleStatus(
            int vehicleId,
            [FromBody] UpdateVehicleStatusRequest request)
        {
            try
            {
                // Validate model
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Invalid request data",
                        Data = ModelState
                    });
                }

                // TODO: Get staffId from JWT token/Claims
                int staffId = 1;

                _logger.LogInformation($"Staff {staffId} updating vehicle {vehicleId} status");

                var response = await _vehicleService.UpdateVehicleStatusAsync(
                    vehicleId, request, staffId);

                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Vehicle status updated IsSuccessfully",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating vehicle {vehicleId} status");

                // Check if vehicle not found
                if (ex.Message.Contains("not found"))
                {
                    return NotFound(new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = ex.Message,
                        Data = null
                    });
                }

                return StatusCode(500, new ResponseDTO
                {
                    IsSuccess = false,
                    Message = "Failed to update vehicle status: " + ex.Message,
                    Data = null
                });
            }
        }

        // Lấy lịch sử cập nhật của xe
        [HttpGet("{vehicleId}/history")]
        public async Task<ActionResult<ResponseDTO>> GetVehicleHistory(int vehicleId)
        {
            try
            {
                _logger.LogInformation($"Fetching history for vehicle {vehicleId}");

                var history = await _vehicleService.GetVehicleHistoryAsync(vehicleId);

                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = $"Retrieved {history.StatusHistory.Count} history records",
                    Data = history
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching vehicle {vehicleId} history");

                if (ex.Message.Contains("not found"))
                {
                    return NotFound(new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = ex.Message,
                        Data = null
                    });
                }

                return StatusCode(500, new ResponseDTO
                {
                    IsSuccess = false,
                    Message = "Failed to fetch vehicle history: " + ex.Message,
                    Data = null
                });
            }
        }

        // Lấy thống kê xe
        [HttpGet("stats")]
        public async Task<ActionResult<ResponseDTO>> GetVehicleStats()
        {
            try
            {
                int? stationId = null;

                _logger.LogInformation($"Calculating stats for station: {stationId}");

                var stats = await _vehicleService.GetVehicleStatsAsync(stationId);

                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Stats retrieved IsSuccessfully",
                    Data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating vehicle stats");
                return StatusCode(500, new ResponseDTO
                {
                    IsSuccess = false,
                    Message = "Failed to calculate stats: " + ex.Message,
                    Data = null
                });
            }
        }

        // Lấy chi tiết 1 xe theo ID
        [HttpGet("{vehicleId}")]
        public async Task<ActionResult<ResponseDTO>> GetVehicleById(int vehicleId)
        {
            try
            {
                var vehicleDto = await _vehicleService.GetVehicleByIdAsync((int)vehicleId);

                if (vehicleDto == null)
                {
                    return NotFound(new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = $"Vehicle not found: {vehicleId}",
                        Data = null
                    });
                }

                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Vehicle retrieved IsSuccessfully",
                    Data = vehicleDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting vehicle {vehicleId}");
                return StatusCode(500, new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    Data = null
                });
            }
        }

    }
}
