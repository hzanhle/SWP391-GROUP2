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
        public async Task<IActionResult> CreateCitizenInfo([FromBody] CitizenInfoRequest request)
        {
            try
            {
                await _citizenInfoService.AddCitizenInfo(request);
                return Ok("Citizen info created successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCitizenInfo([FromBody] CitizenInfoRequest request)
        {
            try
            {
                await _citizenInfoService.UpdateCitizenInfo(request);
                return Ok("Citizen info updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
