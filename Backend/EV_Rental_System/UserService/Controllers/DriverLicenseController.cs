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
        private readonly IDriverLicenseService _driverLicenseService; 

        public DriverLicenseController(IDriverLicenseService driverLicenseService) 
        {
            _driverLicenseService = driverLicenseService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateDriverLicense([FromForm] DriverLicenseRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _driverLicenseService.AddDriverLicense(request);
                return Ok(new { message = "Driver license create request send successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Internal server error: {ex.Message}" });
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDriverLicense(int id)
        {
            await _driverLicenseService.DeleteDriverLicense(id);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> UpdateDriverLicense([FromForm] DriverLicenseRequest request) 
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _driverLicenseService.UpdateDriverLicense(request);
                return Ok(new { message = "Driver license update request send successfully." });
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

        [HttpPost("set-status/{userId}/{isApproved}")]
        public async Task<IActionResult> SetStatus(int userId, bool isApproved)
        {
            try
            {
                var notification = await _driverLicenseService.SetStatus(userId, isApproved);
                return Ok(notification);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Internal server error: {ex.Message}" });
            }
        }
    }
}