using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StationService.DTOs;
using StationService.Models;
using StationService.Services;
using System;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace StationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // URL sẽ là /api/station
    [Authorize(Roles = "Admin, Employee")]
    public class StationController : ControllerBase
    {
        private readonly IStationService _stationService;
        private readonly ILogger<StationController> _logger;

        public StationController(IStationService stationService, ILogger<StationController> logger)
        {
            _stationService = stationService;
            _logger = logger;
        }

        // GET: /api/station
        [HttpGet]
        [AllowAnonymous] // Cho phép khách vãng lai truy cập công khai
        public async Task<IActionResult> GetAllStations()
        {
            try
            {
                var stations = await _stationService.GetAllStationsAsync();
                return Ok(stations); // Trả về 200 OK và danh sách Station
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Đã xảy ra lỗi khi lấy danh sách trạm.");
                return StatusCode(500, "Đã xảy ra lỗi khi xử lý yêu cầu của bạn.");
            }
        }

        // GET: /api/station/active
        [HttpGet("active")]
        [AllowAnonymous] // Cho phép khách vãng lai xem các trạm đang hoạt động
        public async Task<IActionResult> GetActiveStations()
        {
            try
            {
                var activeStations = await _stationService.GetActiveStationsAsync();
                return Ok(activeStations); // Trả về 200 OK và danh sách Station đang hoạt động
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Đã xảy ra lỗi khi lấy danh sách trạm đang hoạt động.");
                return StatusCode(500, "Đã xảy ra lỗi khi xử lý yêu cầu của bạn.");
            }
        }

        // GET: /api/station/5
        [HttpGet("{id}")]
        [AllowAnonymous] // Cho phép khách vãng lai xem chi tiết thông tin trạm
        public async Task<IActionResult> GetStationById(int id)
        {
            try
            {
                var stationDto = await _stationService.GetStationByIdAsync(id);
                if (stationDto == null)
                {
                    return NotFound(); // Trả về 404 Not Found
                }
                return Ok(stationDto); // Trả về 200 OK và StationDTO
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Đã xảy ra lỗi khi lấy thông tin trạm với ID: {StationId}", id);
                return StatusCode(500, "Đã xảy ra lỗi khi xử lý yêu cầu của bạn.");

            }
        }

        // POST: /api/station
        [HttpPost]
        [Authorize(Roles = "Admin")] // Chỉ Admin mới có quyền tạo
        public async Task<IActionResult> CreateStation([FromBody] CreateStationRequest stationRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Trả về 400 Bad Request
            }

            try
            {
                // Gọi service để tạo và nhận lại station đã tạo
                var newStation = await _stationService.AddStationAsync(stationRequest);

                // Trả về 201 Created cùng với link đến tài nguyên mới tạo
                return CreatedAtAction(nameof(GetStationById), new { id = newStation.Id }, newStation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Đã xảy ra lỗi khi tạo trạm mới.");
                return StatusCode(500, "Đã xảy ra lỗi hệ thống, vui lòng thử lại sau.");
            }
        }

        // PUT: /api/station/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")] // Chỉ Admin mới có quyền cập nhật
        public async Task<IActionResult> UpdateStation(int id, [FromBody] UpdateStationRequest stationRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _stationService.UpdateStationAsync(id, stationRequest);
                return NoContent(); // Trả về 204 No Content (Update thành công)
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Đã xảy ra lỗi khi cập nhật trạm với ID: {StationId}", id);
                return StatusCode(500, "Đã xảy ra lỗi khi xử lý yêu cầu của bạn.");
            }

        }

        // DELETE: /api/station/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Chỉ Admin mới có quyền xóa
        public async Task<IActionResult> DeleteStation(int id)
        {
            try
            {
                await _stationService.DeleteStationAsync(id);
                return NoContent(); // Trả về 204 No Content (Delete thành công)
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Đã xảy ra lỗi khi xóa trạm với ID: {StationId}", id);
                return StatusCode(500, "Đã xảy ra lỗi khi xử lý yêu cầu của bạn.");
            }

        }

        // PATCH: /api/station/5/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> SetStatus(int id)
        {
            try
            {
                await _stationService.SetStatus(id);
                return Ok(); // Trả về 204 No Content (Update thành công)
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Đã xảy ra lỗi khi thay đổi trạng thái trạm với ID: {StationId}", id);
                return StatusCode(500, "Đã xảy ra lỗi hệ thống, vui lòng thử lại sau.");
            }
        }
    }
}