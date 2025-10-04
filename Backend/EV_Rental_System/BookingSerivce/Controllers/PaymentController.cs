using BookingSerivce.Models;
using BookingSerivce.Services;
using BookingService.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookingSerivce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IVNPayService _vnPayService;

        public PaymentController(IVNPayService vnPayService)
        {
            _vnPayService = vnPayService;
        }

        [HttpPost("create-payment")]
        public IActionResult CreatePayment([FromBody] PaymentInformationModel model)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var paymentUrl = _vnPayService.CreatePaymentUrl(model, ipAddress);

            return Ok(new { paymentUrl });
        }

        [HttpGet("vnpay-return")]
        public async Task<IActionResult> VNPayReturn()
        {
            var queryParams = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());
            var response = await _vnPayService.ProcessPaymentResponse(queryParams);

            if (response.Success)
            {
                return Ok(new { message = "Payment successful", data = response });
            }

            return BadRequest(new { message = "Payment failed", data = response });
        }
    }
}
