using BookingSerivce.Repositories;
using BookingService.Models;

namespace BookingSerivce.Services
{
    public class ContractService : IContractService
    {
        private readonly IContractRepository _contractRepo;
        private readonly IOrderRepository _orderRepo;

        public ContractService(IContractRepository contractRepo, IOrderRepository orderRepo)
        {
            _contractRepo = contractRepo;
            _orderRepo = orderRepo;
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
    }
}
