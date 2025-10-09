﻿using Microsoft.AspNetCore.Http;
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

                // FIXED: Await the async method
                var response = await _citizenInfoService.AddCitizenInfo(request);

                return Ok(response);
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

                await _citizenInfoService.UpdateCitizenInfo(request);
                return Ok(new { message = "CitizenInfo Update request send successfully." });
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCitizenInfo(int id)
        {
            await _citizenInfoService.DeleteCitizenInfo(id);
            return Ok();
        }

        [HttpPost("set-status/{userId}&{isApproved}")]
        public async Task<IActionResult> SetStatus(int userId, bool isApproved)
        {
            try
            {
                var notification = await _citizenInfoService.SetStatus(userId, isApproved);
                return Ok(notification);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Internal server error: {ex.Message}" });
            }
        }
    }
}