using Microsoft.AspNetCore.Mvc;
using StationService.DTOs; // Sử dụng CreateStationRequest
using StationService.Models; // Sử dụng Station model
using StationService.Services;
using System;
using System.Threading.Tasks;

namespace StationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // URL sẽ là /api/station
    public class StationController : ControllerBase
    {
        private readonly IStationService _stationService;

        public StationController(IStationService stationService)
        {
            _stationService = stationService;
        }

        // GET: /api/station
        [HttpGet]
        public async Task<IActionResult> GetAllStations()
        {
            var stations = await _stationService.GetAllStationsAsync();
            return Ok(stations); // Trả về 200 OK và danh sách Station
        }

        // GET: /api/station/active
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveStations()
        {
            var activeStations = await _stationService.GetActiveStationsAsync();
            return Ok(activeStations); // Trả về 200 OK và danh sách Station đang hoạt động
        }

        // GET: /api/station/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetStationById(int id)
        {
            var stationDto = await _stationService.GetStationByIdAsync(id);
            if (stationDto == null)
            {
                return NotFound(); // Trả về 404 Not Found
            }
            return Ok(stationDto); // Trả về 200 OK và StationDTO
        }

        // POST: /api/station
        [HttpPost]
        public async Task<IActionResult> CreateStation([FromBody] CreateStationRequest stationRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Trả về 400 Bad Request
            }
            await _stationService.AddStationAsync(stationRequest);
            return StatusCode(201, "Station created successfully."); // Trả về 201 Created
        }

        // PUT: /api/station/5
        [HttpPut]
        public async Task<IActionResult> UpdateStation([FromBody] Station station)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _stationService.UpdateStationAsync(station);
            return NoContent(); // Trả về 204 No Content (Update thành công)
        }

        // DELETE: /api/station/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStation(int id)
        {
            await _stationService.DeleteStationAsync(id);
            return NoContent(); // Trả về 204 No Content (Delete thành công)
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> SetStatus(int id)
        {
            try
            {
                await _stationService.SetStatus(id);
                return Ok(); // Trả về 204 No Content (Update thành công)
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); // Trả về 400 Bad Request nếu có lỗi
            }
        }
    }
}