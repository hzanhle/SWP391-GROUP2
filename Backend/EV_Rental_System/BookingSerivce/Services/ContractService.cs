using BookingSerivce.DTOs;
using BookingSerivce.Repositories;
using BookingService.Models;

namespace BookingSerivce.Services
{
    public class ContractService : IContractService
    {
        private readonly IContractRepository _contractRepo;
        private readonly IOrderRepository _orderRepo;
        private readonly IPaymentRepository _paymentRepo;
        private readonly IPdfGeneratorService _pdfGenerator;
        private readonly IStorageService _storageService;
        private readonly ILogger<ContractService> _logger;

        public ContractService(
            IContractRepository contractRepo,
            IOrderRepository orderRepo,
            IPaymentRepository paymentRepo,
            IPdfGeneratorService pdfGenerator,
            IStorageService storageService,
            ILogger<ContractService> logger)
        {
            _contractRepo = contractRepo;
            _orderRepo = orderRepo;
            _paymentRepo = paymentRepo;
            _pdfGenerator = pdfGenerator;
            _storageService = storageService;
            _logger = logger;
        }

        public async Task<OnlineContract> GenerateContractAsync(int orderId, int templateVersion = 1)
        {
            // Check if contract already exists for this order
            var existingContract = await _contractRepo.GetByOrderIdAsync(orderId);
            if (existingContract != null)
            {
                throw new Exception("Contract already exists for this order");
            }

            // Get order details
            var order = await _orderRepo.GetByIdAsync(orderId);
            if (order == null)
                throw new Exception("Order not found");

            // Validate order status
            if (order.Status != "Pending" && order.Status != "AwaitingContract")
                throw new Exception($"Cannot generate contract for order with status: {order.Status}");

            // Generate contract number
            var contractNumber = await _contractRepo.GenerateContractNumberAsync();

            // Generate contract terms (HTML/Text)
            var terms = await GetContractTermsAsync(orderId);

            var contract = new OnlineContract
            {
                OrderId = orderId,
                ContractNumber = contractNumber,
                Terms = terms,
                TemplateVersion = templateVersion,
                Status = "Draft",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7) // 7 days to sign
            };

            var createdContract = await _contractRepo.AddAsync(contract);

            // Update order status
            order.Status = "AwaitingContract";
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepo.UpdateAsync(order);

            return createdContract;
        }

        public async Task<OnlineContract?> GetContractByIdAsync(int contractId)
        {
            return await _contractRepo.GetByIdAsync(contractId);
        }

        public async Task<OnlineContract?> GetContractByOrderIdAsync(int orderId)
        {
            return await _contractRepo.GetByOrderIdAsync(orderId);
        }

        public async Task<OnlineContract> SignContractAsync(int contractId, string signatureData, string ipAddress)
        {
            var contract = await _contractRepo.GetByIdAsync(contractId);
            if (contract == null)
                throw new Exception("Contract not found");

            // Validate contract status
            if (contract.Status == "Signed")
                throw new Exception("Contract has already been signed");

            if (contract.Status == "Expired")
                throw new Exception("Contract has expired");

            // Check expiration
            if (contract.ExpiresAt.HasValue && contract.ExpiresAt.Value < DateTime.UtcNow)
            {
                contract.Status = "Expired";
                await _contractRepo.UpdateAsync(contract);
                throw new Exception("Contract has expired");
            }

            // Sign the contract
            contract.SignedAt = DateTime.UtcNow;
            contract.SignatureData = signatureData;
            contract.SignedFromIpAddress = ipAddress;
            contract.Status = "Signed";
            contract.UpdatedAt = DateTime.UtcNow;

            await _contractRepo.UpdateAsync(contract);

            // Update order status
            if (contract.Order != null)
            {
                contract.Order.Status = "ContractSigned";
                contract.Order.UpdatedAt = DateTime.UtcNow;
                await _orderRepo.UpdateAsync(contract.Order);
            }

            return contract;
        }

        public async Task<string> GetContractTermsAsync(int orderId)
        {
            var order = await _orderRepo.GetByIdAsync(orderId);
            if (order == null)
                throw new Exception("Order not found");

            // Generate contract HTML template
            // In production, you might load this from a database or file
            var terms = $@"
<!DOCTYPE html>
<html>
<head>
    <title>Rental Contract - {order.OrderId}</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; padding: 20px; }}
        h1 {{ color: #333; }}
        .section {{ margin: 20px 0; }}
        .info {{ background: #f4f4f4; padding: 10px; margin: 10px 0; }}
    </style>
</head>
<body>
    <h1>ELECTRIC VEHICLE RENTAL CONTRACT</h1>

    <div class='section'>
        <h2>1. RENTAL INFORMATION</h2>
        <div class='info'>
            <p><strong>Order ID:</strong> {order.OrderId}</p>
            <p><strong>User ID:</strong> {order.UserId}</p>
            <p><strong>Vehicle ID:</strong> {order.VehicleId}</p>
            <p><strong>Rental Period:</strong> {order.FromDate:dd/MM/yyyy HH:mm} to {order.ToDate:dd/MM/yyyy HH:mm}</p>
            <p><strong>Total Days:</strong> {order.TotalDays}</p>
            <p><strong>Total Cost:</strong> {order.TotalCost:N0} VND</p>
            <p><strong>Deposit Required:</strong> {order.DepositAmount:N0} VND (30%)</p>
        </div>
    </div>

    <div class='section'>
        <h2>2. TERMS AND CONDITIONS</h2>
        <p><strong>2.1 Vehicle Usage:</strong> The renter agrees to use the electric vehicle solely for personal transportation purposes and in compliance with all traffic laws.</p>
        <p><strong>2.2 Maintenance:</strong> The renter is responsible for keeping the vehicle clean and in good condition during the rental period.</p>
        <p><strong>2.3 Charging:</strong> The renter must return the vehicle with at least 50% battery charge.</p>
        <p><strong>2.4 Prohibited Activities:</strong> The vehicle may not be used for racing, off-road driving, or any illegal activities.</p>
    </div>

    <div class='section'>
        <h2>3. PAYMENT TERMS</h2>
        <p><strong>3.1 Deposit:</strong> A deposit of 30% ({order.DepositAmount:N0} VND) is required to confirm the booking.</p>
        <p><strong>3.2 Full Payment:</strong> The remaining 70% must be paid before vehicle pickup.</p>
        <p><strong>3.3 Late Fees:</strong> A fee of 50,000 VND per hour applies for late returns.</p>
    </div>

    <div class='section'>
        <h2>4. DAMAGE AND LIABILITY</h2>
        <p><strong>4.1 Inspection:</strong> Vehicle condition will be documented at pickup and return.</p>
        <p><strong>4.2 Damage Responsibility:</strong> The renter is responsible for any damage beyond normal wear and tear.</p>
        <p><strong>4.3 Compensation:</strong> Damage costs will be assessed and must be paid before contract completion.</p>
    </div>

    <div class='section'>
        <h2>5. CANCELLATION POLICY</h2>
        <p><strong>5.1 By Customer:</strong> Cancellation more than 24 hours before pickup: full refund. Less than 24 hours: 50% refund.</p>
        <p><strong>5.2 By Company:</strong> Full refund if we cancel for any reason.</p>
    </div>

    <div class='section'>
        <h2>6. AGREEMENT</h2>
        <p>By signing this contract, you agree to all terms and conditions stated above.</p>
        <p><strong>Contract Date:</strong> {DateTime.UtcNow:dd/MM/yyyy}</p>
    </div>
</body>
</html>";

            return terms;
        }

        // ===== Stage 2 Enhancement Methods =====

        /// <summary>
        /// Generates contract with PDF automatically after payment confirmation.
        /// This is called by OrderService.ConfirmPaymentAsync.
        /// </summary>
        public async Task<OnlineContract> GenerateContractWithPdfAsync(int orderId)
        {
            try
            {
                // Check if contract already exists (idempotency)
                var existingContract = await _contractRepo.GetByOrderIdAsync(orderId);
                if (existingContract != null)
                {
                    _logger.LogWarning("Contract already exists for Order {OrderId}", orderId);
                    return existingContract;
                }

                // Fetch all required data from database (NOT from frontend - security!)
                var order = await _orderRepo.GetByIdAsync(orderId);
                if (order == null)
                    throw new InvalidOperationException($"Order {orderId} not found");

                if (order.Status != "Confirmed")
                    throw new InvalidOperationException($"Cannot generate contract for order with status: {order.Status}");

                var payment = await _paymentRepo.GetByOrderIdAsync(orderId);
                if (payment == null || payment.Status != "Completed")
                    throw new InvalidOperationException($"Payment not completed for order {orderId}");

                // Note: In production, you would fetch User and Vehicle data from their respective services
                // For now, we'll use placeholder data - you'll need to integrate with UserService and VehicleService

                // Build contract data
                var contractData = new ContractData
                {
                    ContractNumber = await _contractRepo.GenerateContractNumberAsync(),
                    ContractDate = DateTime.UtcNow,

                    // TODO: Fetch from UserService
                    UserFullName = $"User {order.UserId}",
                    UserEmail = $"user{order.UserId}@example.com",
                    UserPhone = "0123456789",
                    UserIdCard = "123456789",
                    UserAddress = "User Address",

                    // TODO: Fetch from VehicleService
                    VehicleBrand = "Tesla",
                    VehicleModel = "Model 3",
                    VehiclePlateNumber = $"VEH-{order.VehicleId}",
                    VehicleColor = "White",
                    HourlyRate = order.ModelPrice,

                    // Order data
                    FromDate = order.FromDate,
                    ToDate = order.ToDate,
                    TotalDays = order.TotalDays,
                    TotalCost = order.TotalCost,
                    DepositAmount = order.DepositAmount,
                    DepositPercentage = order.DepositPercentage,

                    // Payment data
                    TransactionId = payment.TransactionId ?? "N/A",
                    PaidAmount = payment.Amount ?? order.DepositAmount,
                    PaymentMethod = payment.Method ?? "VNPay",
                    PaidAt = payment.CompletedAt ?? payment.CreatedAt
                };

                // Generate PDF
                var pdfBytes = await _pdfGenerator.GenerateContractPdfAsync(contractData);

                // Upload PDF to storage
                var pdfFileName = $"{contractData.ContractNumber}.pdf";
                var pdfUrl = await _storageService.UploadContractAsync(pdfFileName, pdfBytes);

                // Generate HTML terms
                var terms = await GetContractTermsAsync(orderId);

                // Create contract record
                var contract = new OnlineContract
                {
                    OrderId = orderId,
                    ContractNumber = contractData.ContractNumber,
                    Terms = terms,
                    PdfFilePath = pdfUrl,
                    TemplateVersion = 1,
                    Status = "Generated",
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(30) // 30 days validity
                };

                var createdContract = await _contractRepo.AddAsync(contract);

                _logger.LogInformation("Contract {ContractNumber} generated for Order {OrderId}",
                    contractData.ContractNumber, orderId);

                return createdContract;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate contract with PDF for Order {OrderId}", orderId);
                throw;
            }
        }

        /// <summary>
        /// Gets the PDF bytes for a contract.
        /// </summary>
        public async Task<byte[]> GetContractPdfAsync(int contractId)
        {
            var contract = await _contractRepo.GetByIdAsync(contractId);
            if (contract == null)
                throw new InvalidOperationException($"Contract {contractId} not found");

            if (string.IsNullOrEmpty(contract.PdfFilePath))
                throw new InvalidOperationException($"Contract {contractId} does not have a PDF file");

            return await _storageService.DownloadContractAsync(contract.PdfFilePath);
        }
    }
}
