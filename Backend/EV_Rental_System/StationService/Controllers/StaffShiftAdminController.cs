using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StationService.DTOs.StaffShift;
using StationService.Services;

namespace StationService.Controllers
{
    [ApiController]
    [Route("api/admin/staffshift")]
    [Authorize(Roles = "Admin")]
    public class StaffShiftAdminController : ControllerBase
    {
        private readonly IStaffShiftService _service;
        private readonly ILogger<StaffShiftAdminController> _logger;

        public StaffShiftAdminController(IStaffShiftService service, ILogger<StaffShiftAdminController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // ==================== CRUD Operations (Admin Only) ====================
        /// [Admin] Tạo ca làm việc mới
        [HttpPost]
        public async Task<IActionResult> CreateShift([FromBody] CreateStaffShiftDTO dto)
        {
            try
            {
                var shift = await _service.CreateShiftAsync(dto);
                return Ok(new
                {
                    success = true,
                    message = "Tạo ca làm việc thành công",
                    data = shift
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating shift");
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra khi tạo ca làm việc" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateShift(int id, [FromBody] UpdateStaffShiftDTO dto)
        {
            try
            {
                var shift = await _service.UpdateShiftAsync(id, dto);
                return Ok(new
                {
                    success = true,
                    message = "Cập nhật ca làm việc thành công",
                    data = shift
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating shift {Id}", id);
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra khi cập nhật ca" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShift(int id)
        {
            try
            {
                var result = await _service.DeleteShiftAsync(id);
                return Ok(new
                {
                    success = true,
                    message = "Xóa ca làm việc thành công"
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting shift {Id}", id);
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra khi xóa ca" });
            }
        }

        /// [Admin] Lấy tất cả ca làm việc
        [HttpGet]
        public async Task<IActionResult> GetAllShifts()
        {
            try
            {
                var shifts = await _service.GetAllShiftsAsync();
                return Ok(new
                {
                    success = true,
                    data = shifts,
                    total = shifts.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all shifts");
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        /// [Admin] Xem lịch làm việc của một nhân viên
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetShiftsByUserId(int userId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                var shifts = await _service.GetShiftsByUserIdAsync(userId, fromDate, toDate);
                return Ok(new
                {
                    success = true,
                    data = shifts,
                    total = shifts.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shifts for user {UserId}", userId);
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        /// [Admin] Xem lịch làm việc của một trạm
        [HttpGet("station/{stationId}")]
        public async Task<IActionResult> GetShiftsByStationId(int stationId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                var shifts = await _service.GetShiftsByStationIdAsync(stationId, fromDate, toDate);
                return Ok(new
                {
                    success = true,
                    data = shifts,
                    total = shifts.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shifts for station {StationId}", stationId);
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        /// [Admin] Xem bảng công của nhân viên
        [HttpGet("timesheet/user/{userId}")]
        public async Task<IActionResult> GetUserTimesheet(int userId, [FromQuery] int month, [FromQuery] int year)
        {
            try
            {
                var timesheet = await _service.GetMonthlyTimesheetAsync(userId, month, year);
                return Ok(new
                {
                    success = true,
                    data = timesheet
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user timesheet");
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        /// [Admin] Thống kê ca làm việc của trạm
        [HttpGet("statistics/station/{stationId}")]
        public async Task<IActionResult> GetStationStatistics(int stationId, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            try
            {
                var statistics = await _service.GetStationStatisticsAsync(stationId, fromDate, toDate);
                return Ok(new
                {
                    success = true,
                    data = statistics
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting station statistics");
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra" });
            }
        }
    }
}
