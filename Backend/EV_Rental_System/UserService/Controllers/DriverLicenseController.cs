using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserService.DTOs;
using UserService.Services;

namespace UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriverLicenseController : ControllerBase
    {
        private readonly IDriverLicenseService _driverLicenseService; // Fixed: Correct service

        public DriverLicenseController(IDriverLicenseService driverLicenseService) // Fixed: Correct interface and parameter name
        {
            _driverLicenseService = driverLicenseService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateDriverLicense([FromForm] DriverLicenseRequest request) // Fixed: Correct DTO and [FromForm]
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _driverLicenseService.AddDriverLicense(request); // Fixed: Correct method
                return Ok(new { message = "Driver license created successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateDriverLicense([FromForm] DriverLicenseRequest request) // Fixed: Correct DTO and [FromForm]
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _driverLicenseService.UpdateDriverLicense(request); // Fixed: Correct method
                return Ok(new { message = "Driver license updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetDriverLicenseByUserId(int userId)
        {
            try
            {
                var driverLicense = await _driverLicenseService.GetDriverLicenseByUserId(userId);
                if (driverLicense == null)
                {
                    return NotFound(new { message = "Driver license not found." });
                }

                return Ok(driverLicense);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Internal server error: {ex.Message}" });
            }
        }
    }
}