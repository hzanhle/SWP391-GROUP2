using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserService.DTOs;
using UserService.Services;

namespace UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CitizenInfoController : ControllerBase
    {
        private readonly ICitizenInfoService _citizenInfoService;

        public CitizenInfoController(ICitizenInfoService citizenInfoService)
        {
            _citizenInfoService = citizenInfoService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCitizenInfo([FromForm] CitizenInfoRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // FIXED: Await the async method
                var citizenInfo = await _citizenInfoService.AddCitizenInfo(request);

                return Ok(new
                {
                    message = "Citizen info created successfully.",
                    data = citizenInfo
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                // Log the exception here
                return StatusCode(500, new { error = "Internal server error occurred.", details = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCitizenInfo([FromForm] CitizenInfoRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _citizenInfoService.UpdateCitizenInfo(request);
                return Ok(new { message = "Citizen info updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCitizenInfoByUserId(int userId)
        {
            try
            {
                var citizenInfo = await _citizenInfoService.GetCitizenInfoByUserId(userId);
                if (citizenInfo == null)
                {
                    return NotFound(new { message = "Citizen info not found." });
                }

                return Ok(citizenInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpPost("set-status/{userId}")]
        public async Task<IActionResult> SetStatus(int userId)
        {
            try
            {
                await _citizenInfoService.SetStatus(userId);
                return Ok(new { message = "Status updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Internal server error: {ex.Message}" });
            }
        }
    }
}