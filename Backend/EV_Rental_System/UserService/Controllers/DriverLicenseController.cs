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
        private readonly ICitizenInfoService _citizenInfoService;
        public DriverLicenseController(CitizenInfoService citizenInfoService)
        {
            _citizenInfoService = citizenInfoService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateDriverLicense([FromBody] DriverLicenseRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.CitizenId) || string.IsNullOrEmpty(request.LicenseNumber))
            {
                return BadRequest("Invalid request data");
            }
            try
            {
                var result = await _citizenInfoService.AddCitizenInfo(request);
                if (result)
                {
                    return Ok("Driver license created successfully");
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create driver license");
                }
            }
            catch (Exception ex)
            {
                // Log the exception (not shown here for brevity)
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }
    }
}
