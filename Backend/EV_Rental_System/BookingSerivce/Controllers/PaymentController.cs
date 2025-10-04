using BookingSerivce.Models;
using BookingSerivce.Repositories;
using BookingSerivce.Services;
using BookingService.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookingSerivce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IVNPayService _vnPayService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderRepository _orderRepo;

        public PaymentController(
            IVNPayService vnPayService,
            IPaymentService paymentService,
            IOrderRepository orderRepo)
        {
            _vnPayService = vnPayService;
            _paymentService = paymentService;
            _orderRepo = orderRepo;
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

        /// <summary>
        /// Create deposit payment for an order (Flow 1 - hold a spot)
        /// </summary>
        [HttpPost("create-deposit")]
        [Authorize]
        public async Task<IActionResult> CreateDepositPayment([FromBody] CreateDepositRequest request)
        {
            try
            {
                // Get order with details
                var order = await _orderRepo.GetByIdAsync(request.OrderId);
                if (order == null)
                    return NotFound(new { success = false, message = "Order not found" });

                // Validate order status
                if (order.Status != "ContractSigned")
                    return BadRequest(new
                    {
                        success = false,
                        message = $"Cannot create deposit payment for order with status: {order.Status}"
                    });

                // Create payment record
                var payment = await _paymentService.CreatePaymentAsync(request.OrderId, "VNPay");

                // Generate VNPay URL for deposit
                var paymentUrl = await _paymentService.CreateVNPayUrlAsync(payment.PaymentId, isDeposit: true);

                // Update order status
                order.Status = "AwaitingDeposit";
                order.UpdatedAt = DateTime.UtcNow;
                await _orderRepo.UpdateAsync(order);

                return Ok(new
                {
                    success = true,
                    message = "Deposit payment created",
                    data = new
                    {
                        paymentId = payment.PaymentId,
                        orderId = order.OrderId,
                        depositAmount = order.DepositAmount,
                        paymentUrl = paymentUrl
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// VNPay callback handler for deposit payment
        /// </summary>
        [HttpGet("vnpay-deposit-callback")]
        public async Task<IActionResult> VNPayDepositCallback()
        {
            try
            {
                var queryParams = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());
                var vnpayResponse = await _vnPayService.ProcessPaymentResponse(queryParams);

                if (!vnpayResponse.Success)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Deposit payment failed",
                        data = vnpayResponse
                    });
                }

                // Process the payment
                var payment = await _paymentService.ProcessVNPayCallbackAsync(vnpayResponse);

                // Update order status to Confirmed
                if (payment.Order != null && payment.IsDeposited)
                {
                    payment.Order.Status = "Confirmed";
                    payment.Order.UpdatedAt = DateTime.UtcNow;
                    await _orderRepo.UpdateAsync(payment.Order);
                }

                return Ok(new
                {
                    success = true,
                    message = "Deposit payment successful. Order confirmed!",
                    data = new
                    {
                        paymentId = payment.PaymentId,
                        orderId = payment.OrderId,
                        depositedAmount = payment.DepositedAmount,
                        transactionCode = payment.DepositTransactionCode,
                        orderStatus = payment.Order?.Status
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get payment details by order ID
        /// </summary>
        [HttpGet("order/{orderId}")]
        [Authorize]
        public async Task<IActionResult> GetPaymentByOrderId(int orderId)
        {
            try
            {
                var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);
                if (payment == null)
                    return NotFound(new { success = false, message = "Payment not found" });

                return Ok(new
                {
                    success = true,
                    data = payment
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Check payment status
        /// </summary>
        [HttpGet("{paymentId}/status")]
        [Authorize]
        public async Task<IActionResult> GetPaymentStatus(int paymentId)
        {
            try
            {
                var payment = await _paymentService.GetPaymentByOrderIdAsync(paymentId);
                if (payment == null)
                    return NotFound(new { success = false, message = "Payment not found" });

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        paymentId = payment.PaymentId,
                        status = payment.Status,
                        isDeposited = payment.IsDeposited,
                        isFullyPaid = payment.IsFullyPaid,
                        depositedAmount = payment.DepositedAmount,
                        paidAmount = payment.PaidAmount
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }

    public class CreateDepositRequest
    {
        public int OrderId { get; set; }
    }
}
