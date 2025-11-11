namespace BookingService.Models
{
    public enum AdditionalPaymentStatus
    {
        NotRequired = 0,  // No additional payment needed (deposit covers all charges)
        Pending = 1,      // Additional payment is required but not yet paid
        Completed = 2,    // Additional payment has been completed
        Failed = 3        // Payment attempt failed
    }
}
