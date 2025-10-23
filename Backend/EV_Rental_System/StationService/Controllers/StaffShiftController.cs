// StationService/Controllers/StaffShiftController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StationService.DTOs.StaffShift;
using StationService.Services;
using System.Security.Claims;

namespace StationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StaffShiftController : ControllerBase
    {
        private readonly IStaffShiftService _service;
        private readonly ILogger<StaffShiftController> _logger;

        public StaffShiftController(IStaffShiftService service, ILogger<StaffShiftController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // ==================== CRUD Operations (Admin Only) ====================
        /// [Admin] Tạo ca làm việc mới
        [HttpPost]
        [Authorize(Roles = "Admin")]
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

        /// [Admin] Cập nhật ca làm việc
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
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

        /// [Admin] Xóa ca làm việc
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
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

        /// [Admin/Employee] Lấy chi tiết ca làm việc
        [HttpGet("{id}")]
        public async Task<IActionResult> GetShiftById(int id)
        {
            try
            {
                var shift = await _service.GetShiftByIdAsync(id);
                if (shift == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy ca làm việc" });
                }

                // Check authorization: Admin or shift owner
                var userRole = User.FindFirst("roleName")?.Value;
                var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");

                if (userRole != "Admin" && userRole != "Employee")
                {
                    return Forbid();
                }

                if (userRole == "Employee" && shift.UserId != userId)
                {
                    return Forbid();
                }

                return Ok(new
                {
                    success = true,
                    data = shift
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shift {Id}", id);
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        /// [Admin] Lấy tất cả ca làm việc
        [HttpGet]
        [Authorize(Roles = "Admin")]
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

        // ==================== Query Operations ====================
        /// [Employee] Xem lịch làm việc của mình
        [HttpGet("my-shifts")]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> GetMyShifts([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
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
                _logger.LogError(ex, "Error getting user shifts");
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        /// [Admin] Xem lịch làm việc của một nhân viên
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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

        // ==================== Check-in/Check-out ===================
        /// [Employee] Check-in ca làm việc
        [HttpPost("check-in")]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> CheckIn([FromBody] CheckInOutDTO dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                dto.UserId = userId;

                var result = await _service.CheckInAsync(dto);
                return Ok(new
                {
                    success = true,
                    message = "Check-in thành công"
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking in");
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        /// [Employee] Check-out ca làm việc
        [HttpPost("check-out")]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> CheckOut([FromBody] CheckInOutDTO dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                dto.UserId = userId;

                var result = await _service.CheckOutAsync(dto);
                return Ok(new
                {
                    success = true,
                    message = "Check-out thành công"
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking out");
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        //VÔ HIỆU HÓA TẠM THỜI YÊU CẦU VÀ DUYỆT ĐỔI CA
        // ==================== Shift Swap ==================== 
        /// [Employee] Yêu cầu đổi ca
        //[HttpPost("swap-request")]
        //[Authorize(Roles = "Employee")]
        //public async Task<IActionResult> CreateSwapRequest([FromBody] ShiftSwapRequestDTO dto)
        //{
        //    try
        //    {
        //        var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
        //        var requestId = await _service.CreateSwapRequestAsync(dto, userId);

        //        return Ok(new
        //        {
        //            success = true,
        //            message = "Yêu cầu đổi ca đã được gửi",
        //            requestId = requestId
        //        });
        //    }
        //    catch (KeyNotFoundException ex)
        //    {
        //        return NotFound(new { success = false, message = ex.Message });
        //    }
        //    catch (UnauthorizedAccessException ex)
        //    {
        //        return Forbid();
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        return BadRequest(new { success = false, message = ex.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error creating swap request");
        //        return StatusCode(500, new { success = false, message = "Có lỗi xảy ra" });
        //    }
        //}

        //Yêu cầu đổi ca tạm thời bị vô hiệu hóa
        ///// [Admin] Duyệt/Từ chối yêu cầu đổi ca
        //[HttpPost("swap-request/approve")]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> ApproveSwapRequest([FromBody] ApproveSwapDTO dto)
        //{
        //    try
        //    {
        //        var adminId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
        //        var result = await _service.ApproveSwapRequestAsync(dto, adminId);

        //        return Ok(new
        //        {
        //            success = true,
        //            message = dto.IsApproved ? "Đã duyệt yêu cầu đổi ca" : "Đã từ chối yêu cầu đổi ca"
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error approving swap request");
        //        return StatusCode(500, new { success = false, message = "Có lỗi xảy ra" });
        //    }
        //} 

        /// [Employee] Xem bảng công tháng
        [HttpGet("timesheet")]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> GetMyTimesheet([FromQuery] int month, [FromQuery] int year)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var timesheet = await _service.GetMonthlyTimesheetAsync(userId, month, year);

                return Ok(new
                {
                    success = true,
                    data = timesheet
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting timesheet");
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        /// [Admin] Xem bảng công của nhân viên
        [HttpGet("timesheet/user/{userId}")]
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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