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

        
    }
}