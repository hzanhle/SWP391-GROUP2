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
                // Kiểm tra ModelState
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Any())
                        .Select(x => new
                        {
                            Field = x.Key,
                            Errors = x.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        })
                        .ToList();

                    return BadRequest(new ResponseDTO
                    {
                        Message = "Dữ liệu không hợp lệ",
                        Data = errors
                    });
                }

                // Await method async và nhận ResponseDTO
                var response = await _driverLicenseService.AddDriverLicense(request);

                // Trả về Ok với dữ liệu từ service
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                // Có thể log ex ở đây
                return StatusCode(500, new { error = "Internal server error occurred.", details = ex.Message });
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
                // Kiểm tra ModelState
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Any())
                        .Select(x => new
                        {
                            Field = x.Key,
                            Errors = x.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        })
                        .ToList();

                    return BadRequest(new ResponseDTO
                    {
                        Message = "Dữ liệu không hợp lệ",
                        Data = errors
                    });
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