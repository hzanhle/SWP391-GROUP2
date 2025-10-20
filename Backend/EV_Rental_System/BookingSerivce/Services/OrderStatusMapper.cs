namespace BookingSerivce.Services
{
    public class OrderStatusMapper : IOrderStatusMapper
    {
        public string GetCustomerDisplayStatus(string internalStatus)
        {
            return internalStatus switch
            {
                "Pending" => "Order Pending Payment",
                "PaymentInitiated" => "Payment in Progress",
                "Confirmed" => "Payment Confirmed",
                "ContractGenerated" => "Waiting for Vehicle Pickup",
                "InProgress" => "Waiting for Vehicle Return",
                "Returned" => "Vehicle Returned - Inspection in Progress",
                "InspectionComplete" => "Inspection Complete",
                "Completed" => "Order Completed",
                "Cancelled" => "Order Cancelled",
                "Expired" => "Order Expired",
                _ => internalStatus
            };
        }

        public string GetAdminDisplayStatus(string internalStatus)
        {
            return internalStatus switch
            {
                "Pending" => "Awaiting Payment",
                "PaymentInitiated" => "Payment Processing",
                "Confirmed" => "Payment Received",
                "ContractGenerated" => "Ready for Pickup",
                "InProgress" => "Vehicle In Use",
                "Returned" => "Vehicle Returned - Needs Inspection",
                "InspectionComplete" => "Inspection Done - Ready to Close",
                "Completed" => "Order Closed",
                "Cancelled" => "Cancelled",
                "Expired" => "Expired",
                _ => internalStatus
            };
        }

        public bool IsValidStatusTransition(string currentStatus, string newStatus)
        {
            // Define valid status transitions
            var validTransitions = new Dictionary<string, List<string>>
            {
                { "Pending", new List<string> { "PaymentInitiated", "Expired", "Cancelled" } },
                { "PaymentInitiated", new List<string> { "Confirmed", "Expired", "Cancelled" } },
                { "Confirmed", new List<string> { "ContractGenerated", "Cancelled" } },
                { "ContractGenerated", new List<string> { "InProgress", "Cancelled" } },
                { "InProgress", new List<string> { "Returned", "Cancelled" } },
                { "Returned", new List<string> { "InspectionComplete", "Cancelled" } },
                { "InspectionComplete", new List<string> { "Completed", "Cancelled" } },
                { "Completed", new List<string> { } }, // Terminal state
                { "Cancelled", new List<string> { } }, // Terminal state
                { "Expired", new List<string> { } }    // Terminal state
            };

            if (!validTransitions.ContainsKey(currentStatus))
                return false;

            return validTransitions[currentStatus].Contains(newStatus);
        }

        public IEnumerable<string> GetAvailableActions(string currentStatus)
        {
            return currentStatus switch
            {
                "ContractGenerated" => new[] { "ConfirmPickup", "Cancel" },
                "InProgress" => new[] { "ConfirmReturn" },
                "Returned" => new[] { "StartInspection" },
                "InspectionComplete" => new[] { "CompleteOrder" },
                _ => Array.Empty<string>()
            };
        }
    }
}
