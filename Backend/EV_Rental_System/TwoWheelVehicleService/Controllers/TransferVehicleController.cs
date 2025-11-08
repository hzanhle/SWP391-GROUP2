using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwoWheelVehicleService.DTOs;
using TwoWheelVehicleService.Services;

namespace TwoWheelVehicleService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Employee")]
    public class TransferVehicleController : ControllerBase
    {
        private readonly ITransferVehicleService _transferVehicleService;

        public TransferVehicleController(ITransferVehicleService transferVehicleService)
        {
            _transferVehicleService = transferVehicleService;
        }

        // GET: api/TransferVehicle
        [HttpGet]
        public async Task<ActionResult<ResponseDTO>> GetAllTransferVehicles()
        {
            try
            {
                var transfers = await _transferVehicleService.GetAllTransferVehicle();

                return Ok(new ResponseDTO
                {
                    Message = "Lấy danh sách chuyển xe thành công",
                    IsSuccess = true,
                    Data = transfers
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO
                {
                    Message = $"Lỗi server: {ex.Message}",
                    IsSuccess = false,
                    Data = null
                });
            }
        }

        // GET: api/TransferVehicle/model/{modelId}
        [HttpGet("model/{modelId}")]
        public async Task<ActionResult<ResponseDTO>> GetTransferVehiclesByModelId(int modelId)
        {
            try
            {
                var transfers = await _transferVehicleService.GetTransferVehiclesByModelId(modelId);

                return Ok(new ResponseDTO
                {
                    Message = "Lấy danh sách chuyển xe theo model thành công",
                    IsSuccess = true,
                    Data = transfers
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO
                {
                    Message = $"Lỗi server: {ex.Message}",
                    IsSuccess = false,
                    Data = null
                });
            }
        }

        // GET: api/TransferVehicle/vehicle/{vehicleId}
        [HttpGet("vehicle/{vehicleId}")]
        public async Task<ActionResult<ResponseDTO>> GetTransferVehicleByVehicleId(int vehicleId)
        {
            try
            {
                var transfer = await _transferVehicleService.GetTransferVehicleByVehicleId(vehicleId);

                if (transfer == null)
                {
                    return NotFound(new ResponseDTO
                    {
                        Message = $"Không tìm thấy yêu cầu chuyển xe cho vehicle {vehicleId}",
                        IsSuccess = false,
                        Data = null
                    });
                }

                return Ok(new ResponseDTO
                {
                    Message = "Lấy thông tin chuyển xe thành công",
                    IsSuccess = true,
                    Data = transfer
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO
                {
                    Message = $"Lỗi server: {ex.Message}",
                    IsSuccess = false,
                    Data = null
                });
            }
        }

        // GET: api/TransferVehicle/status/{status}
        [HttpGet("status/{status}")]
        public async Task<ActionResult<ResponseDTO>> GetTransferVehiclesByStatus(string status)
        {
            try
            {
                var transfers = await _transferVehicleService.GetTransferVehiclesByStatus(status);

                return Ok(new ResponseDTO
                {
                    Message = "Lấy danh sách chuyển xe theo trạng thái thành công",
                    IsSuccess = true,
                    Data = transfers
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO
                {
                    Message = $"Lỗi server: {ex.Message}",
                    IsSuccess = false,
                    Data = null
                });
            }
        }

        // POST: api/TransferVehicle
        [HttpPost]
        public async Task<ActionResult<ResponseDTO>> CreateTransferVehicles([FromBody] TransferRequest request)
        {
            try
            {
                if (request.VehicleIds == null || !request.VehicleIds.Any())
                {
                    return BadRequest(new ResponseDTO
                    {
                        Message = "Danh sách xe không được để trống",
                        IsSuccess = false,
                        Data = null
                    });
                }

                var successfulTransfers = new List<int>();
                var failedTransfers = new List<string>();

                foreach (var vehicleId in request.VehicleIds)
                {
                    try
                    {
                        await _transferVehicleService.AddTransferVehicle(
                            vehicleId,
                            request.ModelId,
                            request.CurrentStationId,
                            request.TargetStationId
                        );
                        successfulTransfers.Add(vehicleId);
                    }
                    catch (Exception ex)
                    {
                        failedTransfers.Add($"Vehicle {vehicleId}: {ex.Message}");
                    }
                }

                if (!successfulTransfers.Any())
                {
                    return BadRequest(new ResponseDTO
                    {
                        Message = "Không tạo được yêu cầu chuyển xe nào",
                        IsSuccess = false,
                        Data = failedTransfers
                    });
                }

                return Ok(new ResponseDTO
                {
                    Message = $"Tạo yêu cầu chuyển xe thành công cho {successfulTransfers.Count} xe",
                    IsSuccess = true,
                    Data = new
                    {
                        SuccessfulTransfers = successfulTransfers,
                        FailedTransfers = failedTransfers
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO
                {
                    Message = $"Lỗi server: {ex.Message}",
                    IsSuccess = false,
                    Data = null
                });
            }
        }

        // PUT: api/TransferVehicle/{vehicleId}/status
        [HttpPut("{vehicleId}/status")]
        public async Task<ActionResult<ResponseDTO>> UpdateTransferVehicleStatus(int vehicleId, [FromBody] string status)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(status))
                {
                    return BadRequest(new ResponseDTO
                    {
                        Message = "Trạng thái không được để trống",
                        IsSuccess = false,
                        Data = null
                    });
                }

                await _transferVehicleService.UpdateTransferVehicle(vehicleId, status);

                return Ok(new ResponseDTO
                {
                    Message = "Cập nhật trạng thái chuyển xe thành công",
                    IsSuccess = true,
                    Data = null
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ResponseDTO
                {
                    Message = ex.Message,
                    IsSuccess = false,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO
                {
                    Message = $"Lỗi server: {ex.Message}",
                    IsSuccess = false,
                    Data = null
                });
            }
        }

        // DELETE: api/TransferVehicle/{vehicleId}
        [HttpDelete("{vehicleId}")]
        [Authorize(Roles = "Admin")] // Chỉ Admin mới được xóa
        public async Task<ActionResult<ResponseDTO>> DeleteTransferVehicle(int vehicleId)
        {
            try
            {
                await _transferVehicleService.DeleteTransferVehicle(vehicleId);

                return Ok(new ResponseDTO
                {
                    Message = "Xóa yêu cầu chuyển xe thành công",
                    IsSuccess = true,
                    Data = null
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ResponseDTO
                {
                    Message = ex.Message,
                    IsSuccess = false,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO
                {
                    Message = $"Lỗi server: {ex.Message}",
                    IsSuccess = false,
                    Data = null
                });
            }
        }
    }
}