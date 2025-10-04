using BookingSerivce.Models;
using BookingSerivce.Repositories;
using BookingService.DTOs;
using BookingService.Models;

namespace BookingSerivce.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IVNPayService _vnPayService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PaymentService(
            IPaymentRepository paymentRepository,
            IVNPayService vnPayService,
            IHttpContextAccessor httpContextAccessor)
        {
            _paymentRepository = paymentRepository;
            _vnPayService = vnPayService;
            _httpContextAccessor = httpContextAccessor;
        }

        // Tạo payment record mới
        public async Task<Payment> CreatePaymentAsync(int orderId, string paymentMethod)
        {
            // Check xem đã có payment cho order này chưa (1-1 relationship)
            var existingPayment = await _paymentRepository.GetByOrderIdAsync(orderId);
            if (existingPayment != null)
            {
                throw new Exception("Payment already exists for this order");
            }

            // Lấy thông tin Order (cần có IOrderRepository hoặc DbContext)
            // Giả sử bạn có IOrderRepository
            // var order = await _orderRepository.GetByIdAsync(orderId);
            // if (order == null)
            //     throw new Exception("Order not found");

            var payment = new Payment
            {
                OrderId = orderId,
                PaymentMethod = paymentMethod,
                Status = "Pending",
                IsDeposited = false,
                IsFullyPaid = false,
                DepositedAmount = 0,
                PaidAmount = 0,
                CreatedAt = DateTime.UtcNow,
                // Có thể thêm thông tin khác từ Order nếu cần
                // Notes = $"Payment for Order #{orderId}"
            };

            await _paymentRepository.AddAsync(payment);
            return payment;
        }

        // Tạo URL thanh toán VNPay
        public async Task<string?> CreateVNPayUrlAsync(int paymentId, bool isDeposit = false)
        {
            // Load payment với Order
            var payment = await _paymentRepository.GetPaymentWithOrderByIdAsync(paymentId);

            if (payment == null)
                throw new Exception("Payment not found");

            if (payment.Order == null)
                throw new Exception("Order not found for this payment");

            // Validate trạng thái
            if (isDeposit && payment.IsDeposited)
                throw new Exception("Deposit has already been paid");

            if (!isDeposit && payment.IsFullyPaid)
                throw new Exception("Payment has already been fully paid");

            // Tính số tiền cần thanh toán
            decimal amount;
            string orderDescription;

            if (isDeposit)
            {
                // Đặt cọc 30%
                amount = payment.Order.TotalCost * 0.3m;
                orderDescription = $"Dat coc don hang {payment.OrderId}";
            }
            else
            {
                // Thanh toán phần còn lại
                decimal remainingAmount = payment.Order.TotalCost - payment.DepositedAmount;

                if (remainingAmount <= 0)
                    throw new Exception("No remaining amount to pay");

                amount = remainingAmount;
                orderDescription = $"Thanh toan don hang {payment.OrderId}";
            }

            var vnpayRequest = new PaymentInformationModel
            {
                OrderType = "billpayment",
                Amount = amount,
                OrderDescription = orderDescription,
                Name = $"Payment_{paymentId}"
            };

            var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

            var paymentUrl = await _vnPayService.CreatePaymentUrl(vnpayRequest, ipAddress);

            // Cập nhật status
            payment.Status = isDeposit ? "PendingDeposit" : "PendingFullPayment";
            payment.UpdatedAt = DateTime.UtcNow;

            await _paymentRepository.UpdateAsync(payment);

            return paymentUrl;
        }

        // Xử lý callback từ VNPay
        public async Task<Payment> ProcessVNPayCallbackAsync(PaymentResponseModel vnpayResponse)
        {
            if (!vnpayResponse.Success)
            {
                throw new Exception($"Payment failed with VNPay code: {vnpayResponse.VnPayResponseCode}");
            }

            // Parse OrderId từ OrderDescription
            // OrderDescription format: "Dat coc don hang 123" hoặc "Thanh toan don hang 123"
            var parts = vnpayResponse.OrderDescription.Split(' ');
            var orderId = int.Parse(parts.Last());

            // Load payment với Order
            var payment = await _paymentRepository.GetByOrderIdAsync(orderId);

            if (payment == null)
                throw new Exception($"Payment not found for OrderId: {orderId}");

            // Kiểm tra duplicate transaction
            if (payment.TransactionCode == vnpayResponse.TransactionId ||
                payment.DepositTransactionCode == vnpayResponse.TransactionId)
            {
                throw new Exception("This transaction has already been processed");
            }

            var now = DateTime.UtcNow;

            // Xác định loại thanh toán
            bool isDepositPayment = vnpayResponse.OrderDescription.Contains("Dat coc");

            if (isDepositPayment)
            {
                // Validate chưa đặt cọc
                if (payment.IsDeposited)
                    throw new Exception("Deposit has already been paid");

                // Tính số tiền đặt cọc (30% của Order)
                decimal depositAmount = payment.Order?.TotalCost * 0.3m ?? 0;

                // Cập nhật thông tin đặt cọc
                payment.IsDeposited = true;
                payment.DepositedAmount = depositAmount;
                payment.DepositDate = now;
                payment.DepositTransactionCode = vnpayResponse.TransactionId;
                payment.Status = "DepositPaid";
                payment.PaidAmount = depositAmount; // Set, không cộng dồn
            }
            else
            {
                // Validate đã đặt cọc trước đó
                if (!payment.IsDeposited)
                    throw new Exception("Deposit must be paid before full payment");

                // Validate chưa thanh toán đủ
                if (payment.IsFullyPaid)
                    throw new Exception("Payment has already been fully paid");

                // Tính số tiền còn lại
                decimal remainingAmount = (payment.Order?.TotalCost ?? 0) - payment.DepositedAmount;

                // Cập nhật thông tin thanh toán đủ
                payment.IsFullyPaid = true;
                payment.FullPaymentDate = now;
                payment.TransactionCode = vnpayResponse.TransactionId;
                payment.Status = "FullyPaid";
                payment.PaidAmount = payment.DepositedAmount + remainingAmount; // Cộng dồn tổng

                // Cập nhật trạng thái Order
                if (payment.Order != null)
                {
                    payment.Order.Status = "Confirmed"; // hoặc "Paid"
                    payment.Order.UpdatedAt = now;
                }
            }

            payment.PaymentMethod = "VNPay";
            payment.UpdatedAt = now;

            await _paymentRepository.UpdateAsync(payment);

            return payment;
        }

        public async Task<Payment?> GetPaymentByOrderIdAsync(int orderId)
        {
            return await _paymentRepository.GetByOrderIdAsync(orderId);
        }

        public async Task<Payment?> GetPaymentByTransactionCodeAsync(string transactionCode)
        {
            return await _paymentRepository.GetByTransactionCodeAsync(transactionCode);
        }

        public async Task UpdatePaymentStatusAsync(int paymentId, string status)
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            if (payment == null)
                throw new Exception("Payment not found");

            // Validate status transitions
            var validStatuses = new[] { "Pending", "PendingDeposit", "PendingFullPayment",
                                       "DepositPaid", "FullyPaid", "Refunded", "PartialRefund" };

            if (!validStatuses.Contains(status))
                throw new Exception($"Invalid payment status: {status}");

            payment.Status = status;
            payment.UpdatedAt = DateTime.UtcNow;

            await _paymentRepository.UpdateAsync(payment);
        }

        // Helper method: Tính số tiền còn phải trả
        public async Task<decimal> GetRemainingAmountAsync(int paymentId)
        {
            var payment = await _paymentRepository.GetPaymentWithOrderByIdAsync(paymentId);

            if (payment == null || payment.Order == null)
                return 0;

            return payment.Order.TotalCost - payment.PaidAmount;
        }

        // Helper method: Check xem có thể thanh toán tiếp không
        public async Task<bool> CanProcessPaymentAsync(int paymentId, bool isDeposit)
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId);

            if (payment == null)
                return false;

            if (isDeposit)
                return !payment.IsDeposited;
            else
                return payment.IsDeposited && !payment.IsFullyPaid;
        }
    }
}