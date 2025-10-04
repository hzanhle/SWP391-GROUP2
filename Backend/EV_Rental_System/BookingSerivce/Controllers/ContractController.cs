using BookingSerivce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingSerivce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContractController : ControllerBase
    {
        private readonly IContractService _contractService;

        public ContractController(IContractService contractService)
        {
            _contractService = contractService;
        }

        /// <summary>
        /// Generate contract for an order
        /// </summary>
        [HttpPost("generate/{orderId}")]
        [Authorize]
        public async Task<IActionResult> GenerateContract(int orderId, [FromBody] GenerateContractRequest? request)
        {
            try
            {
                int templateVersion = request?.TemplateVersion ?? 1;
                var contract = await _contractService.GenerateContractAsync(orderId, templateVersion);

                return Ok(new
                {
                    success = true,
                    message = "Contract generated successfully",
                    data = contract
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Get contract by ID
        /// </summary>
        [HttpGet("{contractId}")]
        [Authorize]
        public async Task<IActionResult> GetContractById(int contractId)
        {
            try
            {
                var contract = await _contractService.GetContractByIdAsync(contractId);
                if (contract == null)
                    return NotFound(new
                    {
                        success = false,
                        message = "Contract not found"
                    });

                return Ok(new
                {
                    success = true,
                    data = contract
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Get contract by order ID
        /// </summary>
        [HttpGet("by-order/{orderId}")]
        [Authorize]
        public async Task<IActionResult> GetContractByOrderId(int orderId)
        {
            try
            {
                var contract = await _contractService.GetContractByOrderIdAsync(orderId);
                if (contract == null)
                    return NotFound(new
                    {
                        success = false,
                        message = "Contract not found for this order"
                    });

                return Ok(new
                {
                    success = true,
                    data = contract
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Sign a contract
        /// </summary>
        [HttpPost("{contractId}/sign")]
        [Authorize]
        public async Task<IActionResult> SignContract(int contractId, [FromBody] SignContractRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.SignatureData))
                    return BadRequest(new
                    {
                        success = false,
                        message = "Signature data is required"
                    });

                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

                var contract = await _contractService.SignContractAsync(
                    contractId,
                    request.SignatureData,
                    ipAddress);

                return Ok(new
                {
                    success = true,
                    message = "Contract signed successfully",
                    data = contract
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Get contract terms/content
        /// </summary>
        [HttpGet("terms/{orderId}")]
        [Authorize]
        public async Task<IActionResult> GetContractTerms(int orderId)
        {
            try
            {
                var terms = await _contractService.GetContractTermsAsync(orderId);
                return Ok(new
                {
                    success = true,
                    data = new { terms }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }

    public class GenerateContractRequest
    {
        public int TemplateVersion { get; set; } = 1;
    }

    public class SignContractRequest
    {
        public string SignatureData { get; set; } = string.Empty;
    }
}
